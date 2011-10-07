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

            // Declare a wait object that will be used for synchronization.
            var waitObject = new ManualResetEvent(false);

            // Declare a timeout value during which the messages are expected to be sent.
            var sentTimeout = TimeSpan.FromMinutes(2);

            Exception failureException = null;

            SendAsync<T>(obj, null, (result) => {
                waitObject.Set();
                failureException = result.ThrownException;
            });

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

        public void SendAsync<T>(T obj, object state, Action<IMessageSentResult<T>> resultCallBack, IServiceBusSerializer serializer = null, IDictionary<string, object> metadata = null) {
            Guard.ArgumentNotNull(obj, "obj");
            Guard.ArgumentNotNull(resultCallBack, "resultCallBack");

            serializer = serializer ?? configuration.DefaultSerializer.Create();

            Exception failureException = null;
            BrokeredMessage message = null;
            bool resultSent = false; //I am not able to determine when the exception block is called.
            var sw = new Stopwatch();
            sw.Start();

            // Use a retry policy to execute the Send action in an asynchronous and reliable fashion.
            retryPolicy.ExecuteAction
            (
                (cb) => {
                    // A new BrokeredMessage instance must be created each time we send it. Reusing the original BrokeredMessage instance may not 
                    // work as the state of its BodyStream cannot be guaranteed to be readable from the beginning.
                    message = new BrokeredMessage(serializer.Serialize(obj), false);

                    message.MessageId = Guid.NewGuid().ToString();
                    message.Properties.Add(TYPE_HEADER_NAME, obj.GetType().FullName.Replace('.', '_'));

                    if (metadata != null) {
                        foreach (var item in metadata) {
                            message.Properties.Add(item.Key, item.Value);
                        }
                    }

                    logger.Info("Send Type={0} Serializer={1} MessageId={2}", obj.GetType().FullName, serializer.GetType().FullName, message.MessageId);

                    // Send the event asynchronously.
                    client.BeginSend(message, cb, null);
                },
                (ar) => {
                    try {
                        // Complete the asynchronous operation. This may throw an exception that will be handled internally by the retry policy.
                        client.EndSend(ar);
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
                            ExtensionMethods.ExecuteAndReturn(() => resultCallBack(new MessageSentResult<T>() {
                                IsSuccess = failureException == null,
                                State = state,
                                ThrownException = failureException,
                                TimeSpent = sw.Elapsed
                            }));
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
                        ExtensionMethods.ExecuteAndReturn(() => resultCallBack(new MessageSentResult<T>() {
                            IsSuccess = failureException == null,
                            State = state,
                            ThrownException = failureException,
                            TimeSpent = sw.Elapsed
                        }));
                    }
                }
            );
        }

        public override void Dispose(bool disposing) {
            Close();
        }

    }
}
