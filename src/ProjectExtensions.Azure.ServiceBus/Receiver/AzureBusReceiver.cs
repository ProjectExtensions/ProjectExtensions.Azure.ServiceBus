using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Threading.Tasks;
using System.IO;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using System.Threading;
using System.Reflection;
using NLog;
using Microsoft.Practices.TransientFaultHandling;
using System.Net;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using ProjectExtensions.Azure.ServiceBus.Wrappers;

namespace ProjectExtensions.Azure.ServiceBus.Receiver {

    /// <summary>
    /// Receiver of Service Bus messages.
    /// </summary>
    class AzureBusReceiver : AzureSenderReceiverBase, IAzureBusReceiver {

        static Logger logger = LogManager.GetCurrentClassLogger();

        object lockObject = new object();

        List<AzureReceiverHelper> mappings = new List<AzureReceiverHelper>();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="configuration">The configuration data</param>
        public AzureBusReceiver(IBusConfiguration configuration)
            : base(configuration) {
            Guard.ArgumentNotNull(configuration, "configuration");
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

                SubscriptionDescription desc = null;

                bool createNew = false;

                try {
                    logger.Info("CreateSubscription Try {0} ", value.SubscriptionName);
                    // First, let's see if a item with the specified name already exists.
                    verifyRetryPolicy.ExecuteAction(() => {
                        desc = namespaceManager.GetSubscription(topic.Path, value.SubscriptionName);
                    });

                    createNew = (desc == null);
                }
                catch (MessagingEntityNotFoundException) {
                    logger.Info("CreateSubscription Does Not Exist {0} ", value.SubscriptionName);
                    // Looks like the item does not exist. We should create a new one.
                    createNew = true;
                }

                // If a item with the specified name doesn't exist, it will be auto-created.
                if (createNew) {
                    var descriptionToCreate = new SubscriptionDescription(topic.Path, value.SubscriptionName);

                    if (value.AttributeData != null) {
                        var attr = value.AttributeData;
                        if (attr.DefaultMessageTimeToLiveSet()) {
                            descriptionToCreate.DefaultMessageTimeToLive = new TimeSpan(0, 0, attr.DefaultMessageTimeToLive);
                        }
                        descriptionToCreate.EnableBatchedOperations = attr.EnableBatchedOperations;
                        descriptionToCreate.EnableDeadLetteringOnMessageExpiration = attr.EnableDeadLetteringOnMessageExpiration;
                        if (attr.LockDurationSet()) {
                            descriptionToCreate.LockDuration = new TimeSpan(0, 0, attr.LockDuration);
                        }
                    }

                    try {
                        logger.Info("CreateSubscription {0} ", value.SubscriptionName);
                        var filter = new SqlFilter(string.Format(TYPE_HEADER_NAME + " = '{0}'", value.MessageType.FullName.Replace('.', '_')));
                        retryPolicy.ExecuteAction(() => {
                            desc = namespaceManager.CreateSubscription(descriptionToCreate, filter);
                        });
                    }
                    catch (MessagingEntityAlreadyExistsException) {
                        logger.Info("CreateSubscription {0} ", value.SubscriptionName);
                        // A item under the same name was already created by someone else, perhaps by another instance. Let's just use it.
                        retryPolicy.ExecuteAction(() => {
                            desc = namespaceManager.GetSubscription(topic.Path, value.SubscriptionName);
                        });
                    }
                }

                SubscriptionClient subscriptionClient = null;
                var rm = ReceiveMode.PeekLock;

                if (value.AttributeData != null) {
                    rm = value.AttributeData.ReceiveMode;
                }

                retryPolicy.ExecuteAction(() => {
                    subscriptionClient = factory.CreateSubscriptionClient(topic.Path, value.SubscriptionName, rm);
                });

                if (value.AttributeData != null && value.AttributeData.PrefetchCountSet()) {
                    subscriptionClient.PrefetchCount = value.AttributeData.PrefetchCount;
                }

                var state = new AzureBusReceiverState() {
                    Client = new SubscriptionClientWrapper(subscriptionClient),
                    EndPointData = value
                };

                var helper = new AzureReceiverHelper(configuration, retryPolicy, state);
                mappings.Add(helper);
                helper.ProcessMessagesForSubscription();

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
                logger.Info("CancelSubscription Does not exist {0}", value.SubscriptionName);
                return;
            }

            subscription.Data.Cancel();

            Task t = Task.Factory.StartNew(() => {
                //HACK find better way to wait for a cancel request so we are not blocking.
                logger.Info("CancelSubscription Deleting {0}", value.SubscriptionName);
                for (int i = 0; i < 100; i++) {
                    if (!subscription.Data.Cancelled) {
                        Thread.Sleep(1000);
                    }
                    else {
                        break;
                    }
                }

                if (namespaceManager.SubscriptionExists(topic.Path, value.SubscriptionName)) {
                    var filter = new SqlFilter(string.Format(TYPE_HEADER_NAME + " = '{0}'", value.MessageType.FullName.Replace('.', '_')));
                    retryPolicy.ExecuteAction(() => namespaceManager.DeleteSubscription(topic.Path, value.SubscriptionName));
                    logger.Info("CancelSubscription Deleted {0}", value.SubscriptionName);
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

        /// <summary>
        /// Dispose
        /// </summary>
        /// <param name="disposing"></param>
        public override void Dispose(bool disposing) {
            foreach (var item in mappings) {
                item.Data.Client.Close();
                if (item is IDisposable) {
                    (item as IDisposable).Dispose();
                }
                item.Data.Client = null;
            }
            mappings.Clear();
        }

    }
}
