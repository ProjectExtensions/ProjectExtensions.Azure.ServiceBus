using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using NLog;
using Microsoft.AzureCAT.Samples.TransientFaultHandling.ServiceBus;
using Microsoft.AzureCAT.Samples.TransientFaultHandling;
using System.Diagnostics;
using System.Threading;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Sender class that publishes messages to the bus
    /// </summary>
    class AzureBusSender : AzureSenderReceiverBase, IAzureBusSender {
        static Logger logger = LogManager.GetCurrentClassLogger();
        TopicClient client;

        object lockObject = new object();

        public AzureBusSender(BusConfiguration configuration)
            : base(configuration) {
            Guard.ArgumentNotNull(configuration, "configuration");
            retryPolicy.ExecuteAction(() => {
                client = factory.CreateTopicClient(topic.Path);
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
            logger.Debug("Send start Type={0} Thread={1}", obj.GetType().FullName, System.Threading.Thread.CurrentThread.ManagedThreadId);
            //messages get dropped with multi threads otherwise.
            lock (lockObject) {
                logger.Debug("Send lockObject start Type={0} Thread={1}", obj.GetType().FullName, System.Threading.Thread.CurrentThread.ManagedThreadId);
                serializer = serializer ?? configuration.DefaultSerializer.Create();
                var helper = new SenderHelper<T>(client, retryPolicy, logger, obj, null, null, serializer, metadata);
                helper.Send();
                logger.Debug("Send lockObject end Type={0} Thread={1}", obj.GetType().FullName, System.Threading.Thread.CurrentThread.ManagedThreadId);
            }
            logger.Debug("Send end Type={0} Thread={1}", obj.GetType().FullName, System.Threading.Thread.CurrentThread.ManagedThreadId);
        }

        public void SendAsync<T>(T obj, object state, Action<IMessageSentResult<T>> resultCallBack) {
            SendAsync<T>(obj, state, resultCallBack, configuration.DefaultSerializer.Create());
        }

        public void SendAsync<T>(T obj, object state, Action<IMessageSentResult<T>> resultCallBack, IDictionary<string, object> metadata) {
            SendAsync<T>(obj, state, resultCallBack, configuration.DefaultSerializer.Create(), metadata);
        }

        public void SendAsync<T>(T obj, object state, Action<IMessageSentResult<T>> resultCallBack, IServiceBusSerializer serializer = null, IDictionary<string, object> metadata = null) {
            serializer = serializer ?? configuration.DefaultSerializer.Create();
            var helper = new SenderHelper<T>(client, retryPolicy, logger, obj, state, resultCallBack, serializer, metadata);
            helper.SendAsync();
        }

        public override void Dispose(bool disposing) {
            Close();
        }

        private class SenderHelper<T> {

            TopicClient client;
            Logger logger;
            RetryPolicy retryPolicy;
            T obj;
            object state;
            Action<IMessageSentResult<T>> resultCallBack;
            IServiceBusSerializer serializer;
            string serializerType;
            IDictionary<string, object> metadata;

            public SenderHelper(TopicClient client, RetryPolicy retryPolicy, Logger logger, T obj, object state,
                Action<IMessageSentResult<T>> resultCallBack,
                IServiceBusSerializer serializer = null, IDictionary<string, object> metadata = null) {

                Guard.ArgumentNotNull(client, "client");
                Guard.ArgumentNotNull(retryPolicy, "retryPolicy");
                Guard.ArgumentNotNull(logger, "logger");
                Guard.ArgumentNotNull(obj, "obj");
                Guard.ArgumentNotNull(serializer, "serializer");

                this.client = client;
                this.retryPolicy = retryPolicy;
                this.logger = logger;
                this.obj = obj;
                this.state = state;
                this.resultCallBack = resultCallBack;
                this.serializer = serializer;
                serializerType = serializer.GetType().FullName;
                this.metadata = metadata;
            }

            public void Send() {

                logger.Debug("SenderHelper Send Start Type={0} Serializer={1} Thread={2}", obj.GetType().FullName, serializerType, System.Threading.Thread.CurrentThread.ManagedThreadId);

                var sw = new Stopwatch();
                sw.Start();

                string messageId = string.Empty;

                // A new BrokeredMessage instance must be created each time we send it. Reusing the original BrokeredMessage instance may not 
                // work as the state of its BodyStream cannot be guaranteed to be readable from the beginning.
                using (var localSerializer = serializer) {
                    using (var message = new BrokeredMessage(serializer.Serialize(obj), false)) {
                        message.MessageId = Guid.NewGuid().ToString();
                        messageId = message.MessageId;
                        message.Properties.Add(TYPE_HEADER_NAME, obj.GetType().FullName.Replace('.', '_'));

                        if (metadata != null) {
                            foreach (var item in metadata) {
                                message.Properties.Add(item.Key, item.Value);
                            }
                        }

                        logger.Debug("SenderHelper Send Start Type={0} Serializer={1} MessageId={2} Thread={3}", obj.GetType().FullName, serializerType, messageId, System.Threading.Thread.CurrentThread.ManagedThreadId);

                        retryPolicy.ExecuteAction(() => {
                            client.Send(message);
                        });

                        sw.Stop();
                        logger.Debug("SenderHelper Send End Type={0} Serializer={1} MessageId={2} Thread={3}", obj.GetType().FullName, serializerType, messageId, System.Threading.Thread.CurrentThread.ManagedThreadId);
                    }
                }
            }

            public void SendAsync() {

                logger.Debug("SenderHelper SendAsync Start Type={0} Serializer={1} Thread={2}", obj.GetType().FullName, serializerType, System.Threading.Thread.CurrentThread.ManagedThreadId);

                Guard.ArgumentNotNull(resultCallBack, "resultCallBack");

                Exception failureException = null;
                BrokeredMessage message = null;
                bool resultSent = false; //I am not able to determine when the exception block is called.
                var sw = new Stopwatch();
                sw.Start();

                var messageId = string.Empty;

                // Use a retry policy to execute the Send action in an asynchronous and reliable fashion.
                retryPolicy.ExecuteAction
                (
                    (cb) => {
                        // A new BrokeredMessage instance must be created each time we send it. Reusing the original BrokeredMessage instance may not 
                        // work as the state of its BodyStream cannot be guaranteed to be readable from the beginning.
                        message = new BrokeredMessage(serializer.Serialize(obj), false);

                        message.MessageId = Guid.NewGuid().ToString();
                        message.Properties.Add(TYPE_HEADER_NAME, obj.GetType().FullName.Replace('.', '_'));
                        messageId = message.MessageId;

                        if (metadata != null) {
                            foreach (var item in metadata) {
                                message.Properties.Add(item.Key, item.Value);
                            }
                        }

                        logger.Debug("SenderHelper BeginSend Type={0} Serializer={1} MessageId={2} Thread={3}", obj.GetType().FullName, serializerType, messageId, System.Threading.Thread.CurrentThread.ManagedThreadId);

                        // Send the event asynchronously.
                        client.BeginSend(message, cb, null);
                    },
                    (ar) => {
                        try {
                            // Complete the asynchronous operation. This may throw an exception that will be handled internally by the retry policy.
                            logger.Debug("SenderHelper EndSend Start Type={0} Serializer={1} MessageId={2} Thread={3}", obj.GetType().FullName, serializerType, messageId, System.Threading.Thread.CurrentThread.ManagedThreadId);
                            client.EndSend(ar);
                            logger.Debug("SenderHelper EndSend End Type={0} Serializer={1} MessageId={2} Thread={3}", obj.GetType().FullName, serializerType, messageId, System.Threading.Thread.CurrentThread.ManagedThreadId);
                        }
                        catch (Exception ex) {
                            failureException = ex;
                        }
                        finally {
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
                                logger.Debug("SenderHelper resultCallBack start Type={0} Serializer={1} MessageId={2} Thread={3}", obj.GetType().FullName, serializerType, messageId, System.Threading.Thread.CurrentThread.ManagedThreadId);
                                ExtensionMethods.ExecuteAndReturn(() => resultCallBack(new MessageSentResult<T>() {
                                    IsSuccess = failureException == null,
                                    State = state,
                                    ThrownException = failureException,
                                    TimeSpent = sw.Elapsed
                                }));
                                logger.Debug("SenderHelper resultCallBack end Type={0} Serializer={1} MessageId={2} Thread={3}", obj.GetType().FullName, serializerType, messageId, System.Threading.Thread.CurrentThread.ManagedThreadId);

                            }
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
                            logger.Debug("SenderHelper failed resultCallBack start Type={0} Serializer={1} MessageId={2} Thread={3}", obj.GetType().FullName, serializerType, messageId, System.Threading.Thread.CurrentThread.ManagedThreadId);
                            ExtensionMethods.ExecuteAndReturn(() => resultCallBack(new MessageSentResult<T>() {
                                IsSuccess = failureException == null,
                                State = state,
                                ThrownException = failureException,
                                TimeSpent = sw.Elapsed
                            }));
                            logger.Debug("SenderHelper failed resultCallBack end Type={0} Serializer={1} MessageId={2} Thread={3}", obj.GetType().FullName, serializerType, messageId, System.Threading.Thread.CurrentThread.ManagedThreadId);
                        }
                    }
                );
            }
        }
    }
}
