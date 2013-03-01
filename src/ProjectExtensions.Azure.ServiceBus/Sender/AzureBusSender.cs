using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using NLog;
using Microsoft.Practices.TransientFaultHandling;
using System.Diagnostics;
using System.Threading;
using ProjectExtensions.Azure.ServiceBus.Interfaces;

namespace ProjectExtensions.Azure.ServiceBus.Sender {

    /// <summary>
    /// Sender class that publishes messages to the bus
    /// </summary>
    class AzureBusSender : AzureSenderReceiverBase, IAzureBusSender {
        static Logger logger = LogManager.GetCurrentClassLogger();
        ITopicClient client;

        public AzureBusSender(IBusConfiguration configuration, IServiceBusConfigurationFactory configurationFactory)
            : base(configuration, configurationFactory) {
            retryPolicy.ExecuteAction(() => {
                client = configurationFactory.MessageFactory.CreateTopicClient(topic.Path);
            });
        }

        public void Close() {
            if (client != null) {
                client.Close();
                client = null;
            }
        }

        public void Send<T>(T obj) {
            Send<T>(obj, null);
        }

        public void Send<T>(T obj, IDictionary<string, object> metadata) {
            Send<T>(obj, configuration.DefaultSerializer.Create(), metadata);
        }

        public void Send<T>(T obj, IServiceBusSerializer serializer = null, IDictionary<string, object> metadata = null) {
            Guard.ArgumentNotNull(obj, "obj");

            // Declare a wait object that will be used for synchronization.
            var waitObject = new ManualResetEvent(false);

            // Declare a timeout value during which the messages are expected to be sent.
            var sentTimeout = TimeSpan.FromSeconds(30);

            Exception failureException = null;

            SendAsync<T>(obj, null, (result) => {
                waitObject.Set();
                failureException = result.ThrownException;
            }, serializer, metadata);

            // Wait until the messaging operations are completed.
            bool completed = waitObject.WaitOne(sentTimeout);
            waitObject.Dispose();

            if (failureException != null) {
                throw failureException;
            }

            if (!completed) {
                throw new Exception("Failed to Send Message. Reason was timeout.");
            }
        }

        public void SendAsync<T>(T obj, object state, Action<IMessageSentResult<T>> resultCallBack) {
            SendAsync<T>(obj, state, resultCallBack, configuration.DefaultSerializer.Create());
        }

        public void SendAsync<T>(T obj, object state, Action<IMessageSentResult<T>> resultCallBack, IDictionary<string, object> metadata) {
            SendAsync<T>(obj, state, resultCallBack, configuration.DefaultSerializer.Create(), metadata);
        }

        public void SendAsync<T>(T methodObj, object methodState, Action<IMessageSentResult<T>> methodResultCallBack,
            IServiceBusSerializer methodSerializer = null, IDictionary<string, object> methodMetadata = null) {

            Guard.ArgumentNotNull(methodObj, "obj");
            Guard.ArgumentNotNull(methodResultCallBack, "resultCallBack");

            methodSerializer = methodSerializer ?? configuration.DefaultSerializer.Create();

            var sw = new Stopwatch();
            sw.Start();

            Action<T, object, Action<IMessageSentResult<T>>, IServiceBusSerializer, IDictionary<string, object>> sendAction = null;

            sendAction = ((obj, state, resultCallBack, serializer, metadata) => {

                IBrokeredMessage message = null;
                Exception failureException = null;
                bool resultSent = false; //I am not able to determine when the exception block is called.

                // Use a retry policy to execute the Send action in an asynchronous and reliable fashion.
                retryPolicy.ExecuteAction
                (
                    (cb) => {
                        failureException = null; //we may retry so we must null out the error.
                        try {
                            // A new BrokeredMessage instance must be created each time we send it. Reusing the original BrokeredMessage instance may not 
                            // work as the state of its BodyStream cannot be guaranteed to be readable from the beginning.
                            message = configurationFactory.MessageFactory.CreateBrokeredMessage(serializer.Serialize(obj));

                            message.MessageId = Guid.NewGuid().ToString();
                            message.Properties.Add(TYPE_HEADER_NAME, obj.GetType().FullName.Replace('.', '_'));

                            if (metadata != null) {
                                foreach (var item in metadata) {
                                    message.Properties.Add(item.Key, item.Value);
                                }
                            }

                            logger.Debug("sendAction BeginSend Type={0} Serializer={1} MessageId={2}", obj.GetType().FullName, serializer.GetType().FullName, message.MessageId);

                            // Send the event asynchronously.
                            client.BeginSend(message, cb, null);
                        }
                        catch (Exception ex) {
                            failureException = ex;
                            throw;
                        }
                    },
                    (ar) => {
                        try {
                            failureException = null; //we may retry so we must null out the error.
                            // Complete the asynchronous operation. This may throw an exception that will be handled internally by the retry policy.
                            logger.Debug("sendAction EndSend Begin Type={0} Serializer={1} MessageId={2}", obj.GetType().FullName, serializer.GetType().FullName, message.MessageId);
                            client.EndSend(ar);
                            logger.Debug("sendAction EndSend End Type={0} Serializer={1} MessageId={2}", obj.GetType().FullName, serializer.GetType().FullName, message.MessageId);
                        }
                        catch (Exception ex) {
                            failureException = ex;
                            throw;
                        }
                    },
                    () => {
                        // Ensure that any resources allocated by a BrokeredMessage instance are released.
                        if (message != null) {
                            message.Dispose();
                            message = null;
                        }
                        if (serializer != null) {
                            serializer.Dispose();
                            serializer = null;
                        }
                        sw.Stop();
                        if (!resultSent) {
                            resultSent = true;
                            ExtensionMethods.ExecuteAndReturn(() => resultCallBack(new MessageSentResult<T>() {
                                IsSuccess = failureException == null,
                                State = state,
                                ThrownException = failureException,
                                TimeSpent = sw.Elapsed
                            }));
                        }
                    },
                    (ex) => {
                        // Always dispose the BrokeredMessage instance even if the send operation has completed unsuccessfully.
                        if (message != null) {
                            message.Dispose();
                            message = null;
                        }
                        if (serializer != null) {
                            serializer.Dispose();
                            serializer = null;
                        }
                        failureException = ex;

                        // Always log exceptions.
                        logger.Error<Exception>("Send failed {0}", ex);

                        sw.Stop();
                        if (!resultSent) {
                            resultSent = true;
                            ExtensionMethods.ExecuteAndReturn(() => resultCallBack(new MessageSentResult<T>() {
                                IsSuccess = failureException == null,
                                State = state,
                                ThrownException = failureException,
                                TimeSpent = sw.Elapsed
                            }));
                        }
                    }
                ); //asyc
            }); //action

            sendAction(methodObj, methodState, methodResultCallBack, methodSerializer, methodMetadata);
        }

        public override void Dispose(bool disposing) {
            Close();
        }

    }
}
