using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus.Messaging;
using NLog;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectExtensions.Azure.ServiceBus.Receiver {

    /// <summary>
    /// Receiver of Service Bus messages.
    /// </summary>
    class AzureBusReceiver : AzureSenderReceiverBase, IAzureBusReceiver {

        static Logger logger = LogManager.GetCurrentClassLogger();

        object lockObject = new object();

        List<AzureReceiverHelper> mappings = new List<AzureReceiverHelper>();

        IServiceBusSerializer serializer;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="configuration">The configuration data</param>
        /// <param name="configurationFactory"></param>
        /// <param name="serializer"></param>
        public AzureBusReceiver(IBusConfiguration configuration, IServiceBusConfigurationFactory configurationFactory, IServiceBusSerializer serializer)
            : base(configuration, configurationFactory) {
            Guard.ArgumentNotNull(serializer, "serializer");
            this.serializer = serializer;
        }

        /// <summary>
        /// Create a new Subscription.
        /// </summary>
        /// <param name="value">The data used to create the subscription</param>
        public void CreateSubscription(ServiceBusEnpointData value) {
            Guard.ArgumentNotNull(value, "value");

            //TODO determine how we can change the filters for an existing registered item
            //ServiceBusNamespaceClient

            lock (lockObject) {

                logger.Info("CreateSubscription {0} Declared {1} MessageTytpe {2}, IsReusable {3} Custom Attribute {4}",
                    value.SubscriptionName,
                    value.DeclaredType.ToString(),
                    value.MessageType.ToString(),
                    value.IsReusable,
                    value.AttributeData != null ? value.AttributeData.ToString() : string.Empty);

                var helper = new AzureReceiverHelper(defaultTopic, configurationFactory, configuration, serializer, verifyRetryPolicy, retryPolicy, value);
                mappings.Add(helper);
                //helper.ProcessMessagesForSubscription();

                //TODO make a config setting to allow us to run subscriptions on different threads or not.
                //create a new thread for processing of the messages.
                var t = new Thread(helper.ProcessMessagesForSubscription);
                t.Name = value.SubscriptionName;
                t.IsBackground = false;
                t.Start();

            } //lock end

        }

        /// <summary>
        /// Cancel a subscription
        /// </summary>
        /// <param name="value">The data used to cancel the subscription</param>
        public void CancelSubscription(ServiceBusEnpointData value) {
            Guard.ArgumentNotNull(value, "value");

            logger.Info("CancelSubscription {0} Declared {1} MessageTytpe {2}, IsReusable {3}", value.SubscriptionName, value.DeclaredType.ToString(), value.MessageType.ToString(), value.IsReusable);

            var subscription = mappings.FirstOrDefault(item => item.Data.EndPointData.SubscriptionName.Equals(value.SubscriptionName, StringComparison.OrdinalIgnoreCase));

            if (subscription == null) {
                logger.Info("CancelSubscription Does not exist {0}", value.SubscriptionNameDebug);
                return;
            }

            subscription.Data.Cancel();

            Task t = Task.Factory.StartNew(() => {
                //HACK find better way to wait for a cancel request so we are not blocking.
                logger.Info("CancelSubscription Deleting {0}", value.SubscriptionNameDebug);
                for (int i = 0; i < 100; i++) {
                    if (!subscription.Data.Cancelled) {
                        Thread.Sleep(1000);
                    }
                    else {
                        break;
                    }
                }

                if (configurationFactory.NamespaceManager.SubscriptionExists(defaultTopic.Path, value.SubscriptionName)) {
                    var filter = new SqlFilter(string.Format(TYPE_HEADER_NAME + " = '{0}'", value.MessageType.FullName.Replace('.', '_')));
                    retryPolicy.ExecuteAction(() => configurationFactory.NamespaceManager.DeleteSubscription(defaultTopic.Path, value.SubscriptionName));
                    logger.Info("CancelSubscription Deleted {0}", value.SubscriptionNameDebug);
                }
            });

            try {
                Task.WaitAny(t);
            }
            catch (Exception ex) {
                if (ex is AggregateException) {
                    //do nothing
                }
                else {
                    throw;
                }
            }
        }

        public long MessageCountForType(Type type) {
            var found = mappings.FirstOrDefault(item => item.Data.EndPointData.DeclaredType == type);
            if (found != null) {
                var retVal = retryPolicy.ExecuteAction(() => {
                    var desc = configurationFactory.NamespaceManager.GetSubscription(defaultTopic.Path, found.Data.EndPointData.SubscriptionName);
                    return desc.MessageCount;
                });
                return retVal;
            }
            return 0;
        }

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        public override void Dispose(bool disposing) {
            foreach (var item in mappings) {
                ExtensionMethods.ExecuteAndReturn(() => {
                    if (item.Data.Client != null) {
                        item.Data.Client.Close();
                    }
                });
                ExtensionMethods.ExecuteAndReturn(() => {
                    if (item is IDisposable) {
                        (item as IDisposable).Dispose();
                    }
                });
            }
            mappings.Clear();
        }

    }
}
