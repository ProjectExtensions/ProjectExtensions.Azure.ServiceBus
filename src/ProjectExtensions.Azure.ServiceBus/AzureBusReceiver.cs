using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System.Threading.Tasks;
using System.IO;
using Autofac;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using System.Threading;
using System.Reflection;
using NLog;

namespace ProjectExtensions.Azure.ServiceBus {

    class AzureBusReceiver : AzureSenderReceiverBase, IAzureBusReceiver {

        static Logger logger = LogManager.GetCurrentClassLogger();

        List<AzureBusReceiverState> mappings = new List<AzureBusReceiverState>();

        public AzureBusReceiver(BusConfiguration configuration)
            : base(configuration) {
        }

        public void CreateSubscription(ServiceBusEnpointData value) {
            //TODO determine how we can change the filters for an existing registered item

            logger.Log(LogLevel.Info, "CreateSubscription {0} Declared {1} MessageTytpe {2}, IsReusable {3}", value.SubscriptionName, value.DeclaredType.ToString(), value.MessageType.ToString(), value.IsReusable);

            SubscriptionDescription desc = null;

            var data = value.MessageType.FullName;

            if (!namespaceManager.SubscriptionExists(topic.Path, value.SubscriptionName)) {
                logger.Log(LogLevel.Info, "CreateSubscription Creating {0}", value.SubscriptionName);

                var filter = new SqlFilter(string.Format(TYPE_HEADER_NAME + " = '{0}'", value.MessageType.FullName.Replace('.', '_')));
                Helpers.Execute(() => {
                    desc = namespaceManager.CreateSubscription(topic.Path, value.SubscriptionName, filter);
                });
            }
            else {
                logger.Log(LogLevel.Info, "CreateSubscription Exists {0}", value.SubscriptionName);
                desc = namespaceManager.GetSubscription(topic.Path, value.SubscriptionName);
            }

            SubscriptionClient subscriptionClient = factory.CreateSubscriptionClient(topic.Path, value.SubscriptionName, ReceiveMode.PeekLock);

            var state = new AzureBusReceiverState() {
                Client = subscriptionClient,
                EndPointData = value,
                Subscription = desc
            };
            mappings.Add(state);

            Task t = new Task(ProcessMessagesForSubscription, state);
            t.Start();
        }

        public void CancelSubscription(ServiceBusEnpointData value) {

            logger.Log(LogLevel.Info, "CancelSubscription {0} Declared {1} MessageTytpe {2}, IsReusable {3}", value.SubscriptionName, value.DeclaredType.ToString(), value.MessageType.ToString(), value.IsReusable);

            var subscription = mappings.FirstOrDefault(item => item.EndPointData.SubscriptionName.Equals(value.SubscriptionName, StringComparison.OrdinalIgnoreCase));

            if (subscription == null) {
                logger.Log(LogLevel.Info, "CancelSubscription Does not exist {0}", value.SubscriptionName);
                return;
            }

            subscription.Cancel = true;

            Task t = new Task(() => {
                //HACK find better way to wait for a cancel request so we are not blocking.
                logger.Log(LogLevel.Info, "CancelSubscription Deleting {0}", value.SubscriptionName);
                for (int i = 0; i < 100; i++) {
                    if (!subscription.Cancelled) {
                        Thread.Sleep(3000);
                    }
                    else {
                        break;
                    }
                }

                if (namespaceManager.SubscriptionExists(topic.Path, value.SubscriptionName)) {
                    var filter = new SqlFilter(string.Format(TYPE_HEADER_NAME + " = '{0}'", value.MessageType.FullName.Replace('.', '_')));
                    Helpers.Execute(() => {
                        namespaceManager.DeleteSubscription(topic.Path, value.SubscriptionName);
                    });
                    logger.Log(LogLevel.Info, "CancelSubscription Deleted {0}", value.SubscriptionName);
                }
            });
            t.Start();
        }

        public override void Dispose(bool disposing) {
            foreach (var item in mappings) {
                item.Client.Close();
                if (item is IDisposable) {
                    (item as IDisposable).Dispose();
                }
            }
        }

        static void ProcessMessagesForSubscription(object state) {

            var data = state as AzureBusReceiverState;

            if (data == null) {
                throw new ArgumentNullException("state");
            }

            logger.Log(LogLevel.Info, "ProcessMessagesForSubscription Message Start {0} Declared {1} MessageTytpe {2}, IsReusable {3}", data.EndPointData.SubscriptionName,
                data.EndPointData.DeclaredType.ToString(), data.EndPointData.MessageType.ToString(), data.EndPointData.IsReusable);

            //TODO create a cache for object creation.
            var gt = typeof(IReceivedMessage<>).MakeGenericType(data.EndPointData.MessageType);

            //set up the methodinfo
            var methodInfo = data.EndPointData.DeclaredType.GetMethod("Handle",
                new Type[] { gt, typeof(IDictionary<string, object>) });

            var serializer = BusConfiguration.Container.Resolve<IServiceBusSerializer>();

            BrokeredMessage message;
            while (!data.Cancel) {
                while ((message = data.Client.Receive(TimeSpan.FromSeconds(55))) != null) {
                    logger.Log(LogLevel.Info, "ProcessMessagesForSubscription Start received new message: {0}", data.EndPointData.SubscriptionName);
                    var receiveState = new AzureReceiveState(data, methodInfo, serializer, message);
                    ProcessMessage(receiveState);
                    logger.Log(LogLevel.Info, "ProcessMessagesForSubscription End received new message: {0}", data.EndPointData.SubscriptionName);
                }
                logger.Log(LogLevel.Info, "ProcessMessagesForSubscription No Messages Received in past 55 seconds: {0}", data.EndPointData.SubscriptionName);
            }

            data.Cancelled = true;

            logger.Log(LogLevel.Info, "ProcessMessagesForSubscription Message Complete={0} Declared={1} MessageTytpe={2} IsReusable={3}", data.EndPointData.SubscriptionName,
                data.EndPointData.DeclaredType.ToString(), data.EndPointData.MessageType.ToString(), data.EndPointData.IsReusable);
        }

        static void ProcessMessage(AzureReceiveState state) {

            logger.Log(LogLevel.Info, "ProcessMessage Start received new message={0} Thread={1} MessageId={2}",
                state.Data.EndPointData.SubscriptionName, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId);

            using (var usingMesssage = state.Message) {
                try {

                    IDictionary<string, object> values = new Dictionary<string, object>();

                    if (state.Message.Properties != null) {
                        foreach (var item in state.Message.Properties) {
                            if (item.Key != AzureSenderReceiverBase.TYPE_HEADER_NAME) {
                                values.Add(item);
                            }
                        }
                    }

                    using (var serial = state.CreateSerializer()) {
                        var stream = state.Message.GetBody<Stream>();
                        stream.Position = 0;
                        object msg = serial.Deserialize(stream, state.Data.EndPointData.MessageType);

                        //TODO create a cache for object creation.
                        var gt = typeof(ReceivedMessage<>).MakeGenericType(state.Data.EndPointData.MessageType);

                        object receivedMessage = Activator.CreateInstance(gt, new object[] { state.Message, msg });

                        logger.Log(LogLevel.Info, "ProcessMessage invoke callback message start message={0} Thread={1} MessageId={2}", state.Data.EndPointData.SubscriptionName, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId);

                        var handler = BusConfiguration.Container.Resolve(state.Data.EndPointData.DeclaredType);
                        state.MethodInfo.Invoke(handler, new object[] {receivedMessage, values});
                        logger.Log(LogLevel.Info, "ProcessMessage invoke callback message end message={0} Thread={1} MessageId={2}", state.Data.EndPointData.SubscriptionName, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId);
                    }
                    state.Message.Complete();
                }
                catch (Exception ex) {
                    logger.Log(LogLevel.Error, "ProcessMessage invoke callback message failed message={0} Thread={1} MessageId={2} Exception={3}", state.Data.EndPointData.SubscriptionName, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId, ex.ToString());
                    //TODO remove hard code dead letter value
                    if (state.Message.DeliveryCount == 5) {
                        Helpers.Execute(() => state.Message.DeadLetter(ex.ToString(), "Died"));
                    }
                }

                logger.Log(LogLevel.Info, "ProcessMessage End received new message={0} Thread={1} MessageId={2}",
                    state.Data.EndPointData.SubscriptionName, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId);

                usingMesssage.Dispose();
            }
        }

        private class AzureReceiveState {

            public AzureReceiveState(AzureBusReceiverState data, MethodInfo methodInfo,
                IServiceBusSerializer serializer, BrokeredMessage message) {
                this.Data = data;
                this.MethodInfo = methodInfo;
                this.Serializer = serializer;
                this.Message = message;
            }

            public AzureBusReceiverState Data {
                get;
                set;
            }
            public MethodInfo MethodInfo {
                get;
                set;
            }
            private IServiceBusSerializer Serializer {
                get;
                set;
            }
            public BrokeredMessage Message {
                get;
                set;
            }

            public IServiceBusSerializer CreateSerializer() {
                return Serializer.Create();
            }
            /*

               //TODO create a cache for object creation.
            var gt = typeof(IReceivedMessage<>).MakeGenericType(data.EndPointData.MessageType);

            //set up the methodinfo
            var methodInfo = data.EndPointData.DeclaredType.GetMethod("Handle",
                new Type[] { gt, typeof(IDictionary<string, object>) });
             
             */
        }

        /// <summary>
        /// Class used to store everything needed in the state and also used so we can cancel.
        /// </summary>
        private class AzureBusReceiverState {

            /// <summary>
            /// Set to true to have the thread clean itself up
            /// </summary>
            public bool Cancel {
                get;
                set;
            }

            /// <summary>
            /// Once the item has stopped running, it marks the state as cancelled.
            /// </summary>
            public bool Cancelled {
                get;
                set;
            }

            public SubscriptionClient Client {
                get;
                set;
            }

            public ServiceBusEnpointData EndPointData {
                get;
                set;
            }

            public SubscriptionDescription Subscription {
                get;
                set;
            }
        }

    }
}
