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
using Microsoft.AzureCAT.Samples.TransientFaultHandling;

namespace ProjectExtensions.Azure.ServiceBus {

    class AzureBusReceiver : AzureSenderReceiverBase, IAzureBusReceiver {

        static Logger logger = LogManager.GetCurrentClassLogger();

        object lockObject = new object();

        List<AzureBusReceiverState> mappings = new List<AzureBusReceiverState>();

        public AzureBusReceiver(BusConfiguration configuration)
            : base(configuration) {
            Guard.ArgumentNotNull(configuration, "configuration");
        }

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
                    retryPolicy.ExecuteAction(() => {
                        desc = namespaceManager.GetSubscription(topic.Path, value.SubscriptionName);
                    });

                    createNew = (topic == null);
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
                        if (attr.MaxDeliveryCountSet()) {
                            descriptionToCreate.MaxDeliveryCount = attr.MaxRetries;
                        }
                    }

                    try {
                        logger.Info("CreateSubscription CreateTopic {0} ", value.SubscriptionName);
                        var filter = new SqlFilter(string.Format(TYPE_HEADER_NAME + " = '{0}'", value.MessageType.FullName.Replace('.', '_')));
                        retryPolicy.ExecuteAction(() => {
                            desc = namespaceManager.CreateSubscription(descriptionToCreate, filter);
                        });
                    }
                    catch (MessagingEntityAlreadyExistsException) {
                        logger.Info("CreateSubscription GetTopic {0} ", value.SubscriptionName);
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
                    Client = subscriptionClient,
                    EndPointData = value,
                    Subscription = desc
                };
                mappings.Add(state);

                var helper = new AzureReceiverHelper(retryPolicy, state);
                helper.ProcessMessagesForSubscription();

            } //lock end


        }

        public void CancelSubscription(ServiceBusEnpointData value) {
            Guard.ArgumentNotNull(value, "value");

            logger.Info("CancelSubscription {0} Declared {1} MessageTytpe {2}, IsReusable {3}", value.SubscriptionName, value.DeclaredType.ToString(), value.MessageType.ToString(), value.IsReusable);

            var subscription = mappings.FirstOrDefault(item => item.EndPointData.SubscriptionName.Equals(value.SubscriptionName, StringComparison.OrdinalIgnoreCase));

            if (subscription == null) {
                logger.Info("CancelSubscription Does not exist {0}", value.SubscriptionName);
                return;
            }

            subscription.Cancel();

            Task t = Task.Factory.StartNew(() => {
                //HACK find better way to wait for a cancel request so we are not blocking.
                logger.Info("CancelSubscription Deleting {0}", value.SubscriptionName);
                for (int i = 0; i < 100; i++) {
                    if (!subscription.Cancelled) {
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

            Task.WaitAny(t);
        }

        public override void Dispose(bool disposing) {
            foreach (var item in mappings) {
                item.Client.Close();
                if (item is IDisposable) {
                    (item as IDisposable).Dispose();
                }
                item.Client = null;
            }
            mappings.Clear();
        }

        private class AzureReceiverHelper {

            RetryPolicy retryPolicy;
            AzureBusReceiverState data;

            public AzureReceiverHelper(RetryPolicy retryPolicy, AzureBusReceiverState data) {
                this.retryPolicy = retryPolicy;
                this.data = data;
            }

            public void ProcessMessagesForSubscription() {
                Guard.ArgumentNotNull(data, "data");

                logger.Info("ProcessMessagesForSubscription Message Start {0} Declared {1} MessageTytpe {2}, IsReusable {3}", data.EndPointData.SubscriptionName,
                        data.EndPointData.DeclaredType.ToString(), data.EndPointData.MessageType.ToString(), data.EndPointData.IsReusable);

                //TODO create a cache for object creation.
                var gt = typeof(IReceivedMessage<>).MakeGenericType(data.EndPointData.MessageType);

                //set up the methodinfo
                var methodInfo = data.EndPointData.DeclaredType.GetMethod("Handle",
                    new Type[] { gt, typeof(IDictionary<string, object>) });

                var serializer = BusConfiguration.Container.Resolve<IServiceBusSerializer>();

                var waitTimeout = TimeSpan.FromSeconds(30);

                // Declare an action acting as a callback whenever a message arrives on a queue.
                AsyncCallback completeReceive = null;

                // Declare an action acting as a callback whenever a non-transient exception occurs while receiving or processing messages.
                Action<Exception> recoverReceive = null;

                // Declare an action implementing the main processing logic for received messages.
                Action<AzureReceiveState> processMessage = ((receiveState) => {
                    // Put your custom processing logic here. DO NOT swallow any exceptions.
                    ProcessMessageCallBack(receiveState);
                });

                var client = data.Client;

                // Declare an action responsible for the core operations in the message receive loop.
                Action receiveMessage = (() => {
                    // Use a retry policy to execute the Receive action in an asynchronous and reliable fashion.
                    retryPolicy.ExecuteAction
                    (
                        (cb) => {
                            // Start receiving a new message asynchronously.
                            client.BeginReceive(waitTimeout, cb, null);
                        },
                        (ar) => {
                            // Make sure we are not told to stop receiving while we were waiting for a new message.
                            if (!data.CancelToken.IsCancellationRequested) {
                                // Complete the asynchronous operation. This may throw an exception that will be handled internally by retry policy.
                                BrokeredMessage msg = client.EndReceive(ar);

                                // Check if we actually received any messages.
                                if (msg != null) {
                                    // Make sure we are not told to stop receiving while we were waiting for a new message.
                                    if (!data.CancelToken.IsCancellationRequested) {
                                        try {
                                            // Process the received message.

                                            logger.Info("ProcessMessagesForSubscription Start received new message: {0}", data.EndPointData.SubscriptionName);
                                            var receiveState = new AzureReceiveState(data, methodInfo, serializer, msg);
                                            processMessage(receiveState);
                                            logger.Info("ProcessMessagesForSubscription End received new message: {0}", data.EndPointData.SubscriptionName);

                                            // With PeekLock mode, we should mark the processed message as completed.
                                            if (client.Mode == ReceiveMode.PeekLock) {
                                                // Mark brokered message as completed at which point it's removed from the queue.
                                                SafeComplete(msg);
                                            }
                                        }
                                        catch {
                                            // With PeekLock mode, we should mark the failed message as abandoned.
                                            if (client.Mode == ReceiveMode.PeekLock) {
                                                // Abandons a brokered message. This will cause Service Bus to unlock the message and make it available 
                                                // to be received again, either by the same consumer or by another completing consumer.
                                                SafeAbandon(msg);
                                            }

                                            // Re-throw the exception so that we can report it in the fault handler.
                                            throw;
                                        }
                                        finally {
                                            // Ensure that any resources allocated by a BrokeredMessage instance are released.
                                            msg.Dispose();
                                        }
                                    }
                                    else {
                                        // If we were told to stop processing, the current message needs to be unlocked and return back to the queue.
                                        if (client.Mode == ReceiveMode.PeekLock) {
                                            SafeAbandon(msg);
                                        }
                                    }
                                }
                            }

                            // Invoke a custom callback method to indicate that we have completed an iteration in the message receive loop.
                            completeReceive(ar);
                        },
                        (ex) => {
                            // Invoke a custom action to indicate that we have encountered an exception and
                            // need further decision as to whether to continue receiving messages.
                            recoverReceive(ex);
                        });
                });

                // Initialize a custom action acting as a callback whenever a message arrives on a queue.
                completeReceive = ((ar) => {
                    if (!data.CancelToken.IsCancellationRequested) {
                        // Continue receiving and processing new messages until we are told to stop.
                        receiveMessage();
                    }
                    data.Cancelled = true;

                    logger.Info("ProcessMessagesForSubscription Message Complete={0} Declared={1} MessageTytpe={2} IsReusable={3}", data.EndPointData.SubscriptionName,
                        data.EndPointData.DeclaredType.ToString(), data.EndPointData.MessageType.ToString(), data.EndPointData.IsReusable);
                });

                // Initialize a custom action acting as a callback whenever a non-transient exception occurs while receiving or processing messages.
                recoverReceive = ((ex) => {
                    // Just log an exception. Do not allow an unhandled exception to terminate the message receive loop abnormally.

                    logger.Error(string.Format("ProcessMessagesForSubscription Message Error={0} Declared={1} MessageTytpe={2} IsReusable={3} Error={4}",
                        data.EndPointData.SubscriptionName,
                        data.EndPointData.DeclaredType.ToString(),
                        data.EndPointData.MessageType.ToString(),
                        data.EndPointData.IsReusable,
                        ex.ToString()));

                    if (!data.CancelToken.IsCancellationRequested) {
                        // Continue receiving and processing new messages until we are told to stop regardless of any exceptions.
                        receiveMessage();
                    }
                    data.Cancelled = true;

                    logger.Info("ProcessMessagesForSubscription Message Complete={0} Declared={1} MessageTytpe={2} IsReusable={3}", data.EndPointData.SubscriptionName,
                        data.EndPointData.DeclaredType.ToString(), data.EndPointData.MessageType.ToString(), data.EndPointData.IsReusable);
                });

                // Start receiving messages asynchronously.
                receiveMessage();
            }

            void ProcessMessageCallBack(AzureReceiveState state) {
                Guard.ArgumentNotNull(state, "state");
                logger.Info("ProcessMessage Start received new message={0} Thread={1} MessageId={2}",
                    state.Data.EndPointData.SubscriptionName, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId);

                string objectTypeName = string.Empty;

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

                        objectTypeName = receivedMessage.GetType().FullName;

                        logger.Info("ProcessMessage invoke callback message start Type={0} message={1} Thread={2} MessageId={3}", objectTypeName, state.Data.EndPointData.SubscriptionName, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId);

                        var handler = BusConfiguration.Container.Resolve(state.Data.EndPointData.DeclaredType);
                        state.MethodInfo.Invoke(handler, new object[] { receivedMessage, values });
                        logger.Info("ProcessMessage invoke callback message end Type={0} message={1} Thread={2} MessageId={3}", objectTypeName, state.Data.EndPointData.SubscriptionName, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId);
                    }
                }
                catch (Exception ex) {
                    logger.Log(LogLevel.Error, "ProcessMessage invoke callback message failed Type={0} message={1} Thread={2} MessageId={3} Exception={4}", objectTypeName, state.Data.EndPointData.SubscriptionName, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId, ex.ToString());

                    //TODO remove hard code dead letter value
                    if (state.Message.DeliveryCount == 5) {
                        retryPolicy.ExecuteAction(() => state.Message.DeadLetter(ex.ToString(), "Died"));
                    }
                    throw;
                }

                logger.Info("ProcessMessage End received new message={0} Thread={1} MessageId={2}",
                    state.Data.EndPointData.SubscriptionName, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId);
            }

            static bool SafeComplete(BrokeredMessage msg) {
                try {
                    // Mark brokered message as complete.
                    msg.Complete();

                    // Return a result indicating that the message has been completed successfully.
                    return true;
                }
                catch (MessageLockLostException) {
                    // It's too late to compensate the loss of a message lock. We should just ignore it so that it does not break the receive loop.
                    // We should be prepared to receive the same message again.
                }
                catch (MessagingException) {
                    // There is nothing we can do as the connection may have been lost, or the underlying topic/subscription may have been removed.
                    // If Complete() fails with this exception, the only recourse is to prepare to receive another message (possibly the same one).
                }

                return false;
            }

            static bool SafeAbandon(BrokeredMessage msg) {
                try {
                    // Abandons a brokered message. This will cause the Service Bus to unlock the message and make it available to be received again, 
                    // either by the same consumer or by another competing consumer.
                    msg.Abandon();

                    // Return a result indicating that the message has been abandoned successfully.
                    return true;
                }
                catch (MessageLockLostException) {
                    // It's too late to compensate the loss of a message lock. We should just ignore it so that it does not break the receive loop.
                    // We should be prepared to receive the same message again.
                }
                catch (MessagingException) {
                    // There is nothing we can do as the connection may have been lost, or the underlying topic/subscription may have been removed.
                    // If Abandon() fails with this exception, the only recourse is to receive another message (possibly the same one).
                }

                return false;
            }

        }

        private class AzureReceiveState {

            public AzureReceiveState(AzureBusReceiverState data, MethodInfo methodInfo,
                IServiceBusSerializer serializer, BrokeredMessage message) {
                Guard.ArgumentNotNull(data, "data");
                Guard.ArgumentNotNull(methodInfo, "methodInfo");
                Guard.ArgumentNotNull(serializer, "serializer");
                Guard.ArgumentNotNull(message, "message");
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

            CancellationTokenSource cancelToken = new CancellationTokenSource();

            public CancellationTokenSource CancelToken {
                get {
                    return cancelToken;
                }
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

            public void Cancel() {
                // Stop the message receive loop gracefully.
                cancelToken.Cancel();
            }
        }

    }
}
