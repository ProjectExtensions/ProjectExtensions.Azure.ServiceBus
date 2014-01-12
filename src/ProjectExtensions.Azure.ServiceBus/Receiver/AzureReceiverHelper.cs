using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.TransientFaultHandling;
using NLog;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using System.Threading;
using Microsoft.ServiceBus.Messaging;
using System.IO;
using System.Diagnostics;
using ProjectExtensions.Azure.ServiceBus.TransientFaultHandling.ServiceBus;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using System.Reflection;

namespace ProjectExtensions.Azure.ServiceBus.Receiver {

    class AzureReceiverHelper {

        static Logger logger = LogManager.GetCurrentClassLogger();

        readonly TopicDescription topic;
        readonly IServiceBusConfigurationFactory configurationFactory;
        readonly IBusConfiguration config;
        readonly RetryPolicy retryPolicy;
        readonly RetryPolicy verifyRetryPolicy;
        AzureBusReceiverState data;
        ServiceBusEnpointData endpoint;
        readonly IServiceBusSerializer serializer;
        MethodInfo methodInfo;
        OnMessageOptions options;

        int failCounter = 0;

        public AzureBusReceiverState Data {
            get {
                return data;
            }
        }

        public AzureReceiverHelper(TopicDescription topic, IServiceBusConfigurationFactory configurationFactory, IBusConfiguration config,
            IServiceBusSerializer serializer, RetryPolicy verifyRetryPolicy, RetryPolicy retryPolicy, ServiceBusEnpointData endpoint) {
            Guard.ArgumentNotNull(topic, "topic");
            Guard.ArgumentNotNull(configurationFactory, "configurationFactory");
            Guard.ArgumentNotNull(config, "config");
            Guard.ArgumentNotNull(serializer, "serializer");
            Guard.ArgumentNotNull(retryPolicy, "retryPolicy");
            Guard.ArgumentNotNull(endpoint, "endpoint");
            this.topic = topic;
            this.configurationFactory = configurationFactory;
            this.config = config;
            this.serializer = serializer;
            this.verifyRetryPolicy = verifyRetryPolicy;
            this.retryPolicy = retryPolicy;
            this.endpoint = endpoint;

            Configure(endpoint);
        }

        public void ProcessMessagesForSubscription() {
            var gt = typeof(IReceivedMessage<>).MakeGenericType(data.EndPointData.MessageType);
            methodInfo = data.EndPointData.DeclaredType.GetMethod("Handle", new Type[] { gt });

            options = new OnMessageOptions() {
                AutoComplete = false,
                MaxConcurrentCalls = 8 //Make the app webscale
            };
            options.ExceptionReceived += options_ExceptionReceived;

            //ProcessMessagesForSubscriptionLegacy();
            ProcessMessagesForSubscriptionAzure();
        }

        private bool OnReceiveProcess(AzureReceiveState receiveState) {
            var retVal = false;
            try {
                ProcessMessageCallBack(receiveState);
            }
            catch (Exception ex) {
                //TODO log and whatnot.
                retVal = true;
            }
            finally {
                try {
                    // With PeekLock mode, we should mark the failed message as abandoned.
                    if (receiveState.Message != null) {
                        if (data.Client.Mode == ReceiveMode.PeekLock) {
                            if (retVal) {
                                // Abandons a brokered message. This will cause Service Bus to unlock the message and make it available 
                                // to be received again, either by the same consumer or by another completing consumer.
                                SafeAbandon(receiveState.Message);
                            }
                            else {
                                // Mark brokered message as completed at which point it's removed from the queue.
                                SafeComplete(receiveState.Message);
                            }
                        }
                        receiveState.Message.Dispose();
                    }
                }
                catch {
                    //Do not fail :)
                }
            }
            return retVal;
        }

        public void OnMessageHandler(IBrokeredMessage msg) {
            //receive loop begin
            bool failed = false;
            if (msg != null) {
                // Make sure we are not told to stop receiving while we were waiting for a new message.
                if (!data.CancelToken.IsCancellationRequested) {
                    // Process the received message.
                    logger.Debug("ProcessMessagesForSubscription Start received new message: {0}", data.EndPointData.SubscriptionNameDebug);
                    var receiveState = new AzureReceiveState(data, methodInfo, serializer, msg);
                    failed = OnReceiveProcess(receiveState);
                    logger.Debug("ProcessMessagesForSubscription End received new message: {0}", data.EndPointData.SubscriptionNameDebug);
                }
            }

            if (data.CancelToken.IsCancellationRequested) {
                //Cancel the message pump.
                data.SetMessageLoopCompleted();
                try {
                    data.Client.Close();
                }
                catch {
                }
            }
            else if (failed) {
                //close the pump.
                data.Client.Close();
                if (data.EndPointData.AttributeData.PauseTimeIfErrorWasThrown > 0) {
                    Thread.Sleep(data.EndPointData.AttributeData.PauseTimeIfErrorWasThrown);
                }
                else {
                    Thread.Sleep(1000);
                }
                data.Client.OnMessage(OnMessageHandler, options);
            }
        }

        private void ProcessMessagesForSubscriptionAzure() {
            logger.Info("ProcessMessagesForSubscription Message Start {0} Declared {1} MessageTytpe {2}, IsReusable {3}", data.EndPointData.SubscriptionName,
                         data.EndPointData.DeclaredType.ToString(), data.EndPointData.MessageType.ToString(), data.EndPointData.IsReusable);

            data.Client.OnMessage(OnMessageHandler, options);
        }

        void options_ExceptionReceived(object sender, ExceptionReceivedEventArgs ex) {

            if (!data.CancelToken.IsCancellationRequested && typeof(ThreadAbortException) != ex.GetType()) {

                //The subscription may have been deleted. If it was, then we want to recreate it.
                var subException = ex.Exception as MessagingEntityNotFoundException;

                if (subException != null && subException.Detail != null && subException.Detail.Message.IndexOf("40400") > -1) {
                    logger.Info("Subscription was deleted. Attempting to Recreate.");
                    Configure(endpoint);
                    data.Client.OnMessage(OnMessageHandler, options);
                    logger.Info("Subscription was deleted. Recreated.");
                }
                else {
                    logger.Error(string.Format("ProcessMessagesForSubscription Message Error={0} Declared={1} MessageTytpe={2} IsReusable={3} Error={4}",
                        data.EndPointData.SubscriptionName,
                        data.EndPointData.DeclaredType.ToString(),
                        data.EndPointData.MessageType.ToString(),
                        data.EndPointData.IsReusable,
                        ex.ToString()));
                }
            }
            else {
                //data.SetMessageLoopCompleted();
            }

            //TODO do something with the error we just received.
            logger.Error("Message Pump Error: Start {0} Declared {1} MessageTytpe {2}, Error {3}", data.EndPointData.SubscriptionName,
                         data.EndPointData.DeclaredType.ToString(), data.EndPointData.MessageType.ToString(), ex.Exception.ToString());
        }

        private void ProcessMessagesForSubscriptionLegacy() {

            try {

                logger.Info("ProcessMessagesForSubscription Message Start {0} Declared {1} MessageTytpe {2}, IsReusable {3}", data.EndPointData.SubscriptionName,
                        data.EndPointData.DeclaredType.ToString(), data.EndPointData.MessageType.ToString(), data.EndPointData.IsReusable);

                var gt = typeof(IReceivedMessage<>).MakeGenericType(data.EndPointData.MessageType);
                var methodInfo = data.EndPointData.DeclaredType.GetMethod("Handle", new Type[] { gt });

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

                bool messageReceived = false;

                bool lastAttemptWasError = false;

                // Declare an action responsible for the core operations in the message receive loop.
                Action receiveMessage = (() => {
                    // Use a retry policy to execute the Receive action in an asynchronous and reliable fashion.
                    retryPolicy.ExecuteAction
                    (
                        (cb) => {
                            if (lastAttemptWasError) {
                                if (data.EndPointData.AttributeData.PauseTimeIfErrorWasThrown > 0) {
                                    Thread.Sleep(data.EndPointData.AttributeData.PauseTimeIfErrorWasThrown);
                                }
                                else {
                                    Thread.Sleep(1000);
                                }
                            }
                            // Start receiving a new message asynchronously.
                            data.Client.BeginReceive(waitTimeout, cb, null);
                        },
                        (ar) => {
                            messageReceived = false;
                            // Make sure we are not told to stop receiving while we were waiting for a new message.
                            if (!data.CancelToken.IsCancellationRequested) {
                                IBrokeredMessage msg = null;
                                bool isError = false;
                                try {
                                    // Complete the asynchronous operation. This may throw an exception that will be handled internally by retry policy.
                                    msg = data.Client.EndReceive(ar);

                                    // Check if we actually received any messages.
                                    if (msg != null) {
                                        // Make sure we are not told to stop receiving while we were waiting for a new message.
                                        if (!data.CancelToken.IsCancellationRequested) {
                                            // Process the received message.
                                            messageReceived = true;
                                            logger.Debug("ProcessMessagesForSubscription Start received new message: {0}", data.EndPointData.SubscriptionNameDebug);
                                            var receiveState = new AzureReceiveState(data, methodInfo, serializer, msg);
                                            processMessage(receiveState);
                                            logger.Debug("ProcessMessagesForSubscription End received new message: {0}", data.EndPointData.SubscriptionNameDebug);
                                        }
                                    }
                                }
                                catch (Exception) {
                                    isError = true;
                                    throw;
                                }
                                finally {
                                    // With PeekLock mode, we should mark the failed message as abandoned.
                                    if (msg != null) {
                                        if (data.Client.Mode == ReceiveMode.PeekLock) {
                                            if (isError) {
                                                // Abandons a brokered message. This will cause Service Bus to unlock the message and make it available 
                                                // to be received again, either by the same consumer or by another completing consumer.
                                                SafeAbandon(msg);
                                            }
                                            else {
                                                // Mark brokered message as completed at which point it's removed from the queue.
                                                SafeComplete(msg);
                                            }
                                        }
                                        msg.Dispose();
                                    }
                                }
                            }
                            // Invoke a custom callback method to indicate that we have completed an iteration in the message receive loop.
                            completeReceive(ar);
                        },
                        () => {
                            //do nothing, we completed.
                        },
                        (ex) => {
                            // Invoke a custom action to indicate that we have encountered an exception and
                            // need further decision as to whether to continue receiving messages.
                            recoverReceive(ex);
                        });
                });

                // Initialize a custom action acting as a callback whenever a message arrives on a queue.
                completeReceive = ((ar) => {
                    lastAttemptWasError = false;
                    if (!data.CancelToken.IsCancellationRequested) {
                        // Continue receiving and processing new messages until we are told to stop.
                        receiveMessage();
                    }
                    else {
                        data.SetMessageLoopCompleted();
                    }

                    if (messageReceived) {
                        logger.Debug("ProcessMessagesForSubscription Message Complete={0} Declared={1} MessageTytpe={2} IsReusable={3}", data.EndPointData.SubscriptionName,
                            data.EndPointData.DeclaredType.ToString(), data.EndPointData.MessageType.ToString(), data.EndPointData.IsReusable);
                    }
                });

                // Initialize a custom action acting as a callback whenever a non-transient exception occurs while receiving or processing messages.
                recoverReceive = ((ex) => {
                    // Just log an exception. Do not allow an unhandled exception to terminate the message receive loop abnormally.
                    lastAttemptWasError = true;

                    if (!data.CancelToken.IsCancellationRequested && typeof(ThreadAbortException) != ex.GetType()) {

                        //The subscription may have been deleted. If it was, then we want to recreate it.
                        var subException = ex as MessagingEntityNotFoundException;

                        if (subException != null && subException.Detail != null && subException.Detail.Message.IndexOf("40400") > -1) {
                            logger.Info("Subscription was deleted. Attempting to Recreate.");
                            Configure(endpoint);
                            logger.Info("Subscription was deleted. Recreated.");
                        }
                        else {
                            logger.Error(string.Format("ProcessMessagesForSubscription Message Error={0} Declared={1} MessageTytpe={2} IsReusable={3} Error={4}",
                                data.EndPointData.SubscriptionName,
                                data.EndPointData.DeclaredType.ToString(),
                                data.EndPointData.MessageType.ToString(),
                                data.EndPointData.IsReusable,
                                ex.ToString()));
                        }

                        // Continue receiving and processing new messages until we are told to stop regardless of any exceptions.
                        receiveMessage();
                    }
                    else {
                        data.SetMessageLoopCompleted();
                    }

                    if (messageReceived) {
                        logger.Debug("ProcessMessagesForSubscription Message Complete={0} Declared={1} MessageTytpe={2} IsReusable={3}", data.EndPointData.SubscriptionName,
                            data.EndPointData.DeclaredType.ToString(), data.EndPointData.MessageType.ToString(), data.EndPointData.IsReusable);
                    }
                });

                // Start receiving messages asynchronously.
                receiveMessage();
            }
            catch (ThreadAbortException) {
                //do nothing, let it exit
            }
            catch (Exception ex) {
                failCounter++;
                logger.Error("ProcessMessagesForSubscription: Error during receive during loop. See details and fix: {0}", ex);
                if (failCounter < 100) {
                    //try again
                    ProcessMessagesForSubscription();
                }
            }
        }

        void Configure(ServiceBusEnpointData value) {

            SubscriptionDescription desc = null;

            bool createNew = false;

            try {
                logger.Info("CreateSubscription Try {0} ", value.SubscriptionNameDebug);
                // First, let's see if a item with the specified name already exists.
                verifyRetryPolicy.ExecuteAction(() => {
                    desc = configurationFactory.NamespaceManager.GetSubscription(topic.Path, value.SubscriptionName);
                });

                createNew = (desc == null);
            }
            catch (MessagingEntityNotFoundException) {
                logger.Info("CreateSubscription Does Not Exist {0} ", value.SubscriptionNameDebug);
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
                    logger.Info("CreateSubscription {0} ", value.SubscriptionNameDebug);
                    var filter = new SqlFilter(string.Format(AzureSenderReceiverBase.TYPE_HEADER_NAME + " = '{0}'", value.MessageType.FullName.Replace('.', '_')));
                    retryPolicy.ExecuteAction(() => {
                        desc = configurationFactory.NamespaceManager.CreateSubscription(descriptionToCreate, filter);
                    });
                }
                catch (MessagingEntityAlreadyExistsException) {
                    logger.Info("CreateSubscription {0} ", value.SubscriptionNameDebug);
                    // A item under the same name was already created by someone else, perhaps by another instance. Let's just use it.
                    retryPolicy.ExecuteAction(() => {
                        desc = configurationFactory.NamespaceManager.GetSubscription(topic.Path, value.SubscriptionName);
                    });
                }
            }

            ISubscriptionClient subscriptionClient = null;
            var rm = ReceiveMode.PeekLock;

            if (value.AttributeData != null) {
                rm = value.AttributeData.ReceiveMode;
            }

            retryPolicy.ExecuteAction(() => {
                subscriptionClient = configurationFactory.MessageFactory.CreateSubscriptionClient(topic.Path, value.SubscriptionName, rm);
            });

            if (value.AttributeData != null && value.AttributeData.PrefetchCountSet()) {
                subscriptionClient.PrefetchCount = value.AttributeData.PrefetchCount;
            }

            if (data == null) {
                data = new AzureBusReceiverState() {
                    EndPointData = value,
                    Client = subscriptionClient
                };
            }
            else {
                //we need to clean up the deleted subscription
                var oldClient = this.data.Client;

                data.Client = subscriptionClient;

                //now lets dispose the client.
                ExtensionMethods.ExecuteAndReturn(() => {
                    if (oldClient != null) {
                        oldClient.Close();
                    }
                });
            }
        }

        void ProcessMessageCallBack(AzureReceiveState state) {
            Guard.ArgumentNotNull(state, "state");
            logger.Debug("ProcessMessage Start received new message={0} Thread={1} MessageId={2}",
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

                    var receivedMessage = state.Data.EndPointData.GetReceivedMessage(new object[] { state.Message, msg, values });

                    objectTypeName = receivedMessage.GetType().FullName;

                    logger.Debug("ProcessMessage invoke callback message start Type={0} message={1} Thread={2} MessageId={3}", objectTypeName, state.Data.EndPointData.SubscriptionNameDebug, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId);

                    var handler = config.Container.Resolve(state.Data.EndPointData.DeclaredType);

                    logger.Debug("ProcessMessage reflection callback message start MethodInfo Type={0} Declared={1} handler={2} MethodInfo={3} Thread={4} MessageId={5}", objectTypeName, state.Data.EndPointData.DeclaredType, handler.GetType().FullName, state.MethodInfo.Name, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId);

                    state.MethodInfo.Invoke(handler, new object[] { receivedMessage });
                    logger.Debug("ProcessMessage invoke callback message end Type={0} message={1} Thread={2} MessageId={3}", objectTypeName, state.Data.EndPointData.SubscriptionNameDebug, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId);
                }
            }
            catch (Exception ex) {
                logger.Error("ProcessMessage invoke callback message failed Type={0} message={1} Thread={2} MessageId={3} Exception={4}", objectTypeName, state.Data.EndPointData.SubscriptionNameDebug, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId, ex.ToString());

                if (state.Message.DeliveryCount >= state.Data.EndPointData.AttributeData.MaxRetries) {
                    if (state.Data.EndPointData.AttributeData.DeadLetterAfterMaxRetries) {
                        SafeDeadLetter(state.Message, ex.Message);
                    }
                    else {
                        SafeComplete(state.Message);
                    }
                }
                throw;
            }

            logger.Debug("ProcessMessage End received new message={0} Thread={1} MessageId={2}",
                state.Data.EndPointData.SubscriptionNameDebug, Thread.CurrentThread.ManagedThreadId, state.Message.MessageId);
        }

        static bool SafeDeadLetter(IBrokeredMessage msg, string reason) {
            try {
                // Mark brokered message as complete.
                msg.DeadLetter(reason, "Max retries Exceeded.");

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

        static bool SafeComplete(IBrokeredMessage msg) {
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

        static bool SafeAbandon(IBrokeredMessage msg) {
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
}
