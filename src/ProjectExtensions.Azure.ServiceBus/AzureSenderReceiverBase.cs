using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;
using NLog;
using Microsoft.AzureCAT.Samples.TransientFaultHandling;
using Microsoft.AzureCAT.Samples.TransientFaultHandling.ServiceBus;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Base class for the sender and receiver client.
    /// </summary>
    abstract class AzureSenderReceiverBase : IDisposable {
        static Logger logger = LogManager.GetCurrentClassLogger();

        protected static string TYPE_HEADER_NAME = "x_proj_ext_type"; //- are not allowed if you filter.

        protected BusConfiguration configuration;
        protected MessagingFactory factory;
        protected NamespaceManager namespaceManager;
        protected RetryPolicy<ServiceBusTransientErrorDetectionStrategy> retryPolicy
            = new RetryPolicy<ServiceBusTransientErrorDetectionStrategy>(RetryPolicy.DefaultClientRetryCount);
        protected TokenProvider tokenProvider;
        protected TopicDescription topic;
        protected Uri serviceUri;

        /// <summary>
        /// Base class used to send and receive messages.
        /// </summary>
        /// <param name="configuration"></param>
        public AzureSenderReceiverBase(BusConfiguration configuration) {
            if (configuration == null) {
                throw new ArgumentNullException("configuration");
            }
            this.configuration = configuration;

            tokenProvider = TokenProvider.CreateSharedSecretTokenProvider(configuration.ServiceBusIssuerName, configuration.ServiceBusIssuerKey);

            var servicePath = string.Empty;

            if (!string.IsNullOrWhiteSpace(configuration.ServicePath)) {
                servicePath = configuration.ServicePath;
            }

            serviceUri = ServiceBusEnvironment.CreateServiceUri("sb", configuration.ServiceBusNamespace, servicePath);
            factory = MessagingFactory.Create(serviceUri, tokenProvider);
            namespaceManager = new NamespaceManager(serviceUri, tokenProvider);
            EnsureTopic(configuration.TopicName);
        }

        protected void EnsureTopic(string topicName) {

            bool createNew = false;

            try {
                logger.Log(LogLevel.Info, "EnsureTopic Try {0} ", topicName);
                // First, let's see if a topic with the specified name already exists.
                topic = retryPolicy.ExecuteAction<TopicDescription>(() => {
                    return namespaceManager.GetTopic(topicName);
                });

                createNew = (topic == null);
            }
            catch (MessagingEntityNotFoundException) {
                logger.Log(LogLevel.Info, "EnsureTopic Does Not Exist {0} ", topicName);
                // Looks like the topic does not exist. We should create a new one.
                createNew = true;
            }

            // If a topic with the specified name doesn't exist, it will be auto-created.
            if (createNew) {
                try {
                    logger.Log(LogLevel.Info, "EnsureTopic CreateTopic {0} ", topicName);
                    var newTopic = new TopicDescription(topicName);

                    topic = retryPolicy.ExecuteAction<TopicDescription>(() => {
                        return namespaceManager.CreateTopic(newTopic);
                    });
                }
                catch (MessagingEntityAlreadyExistsException) {
                    logger.Log(LogLevel.Info, "EnsureTopic GetTopic {0} ", topicName);
                    // A topic under the same name was already created by someone else, perhaps by another instance. Let's just use it.
                    topic = retryPolicy.ExecuteAction<TopicDescription>(() => {
                        return namespaceManager.GetTopic(topicName);
                    });
                }
            }

        }

        public void Dispose() {
            Dispose(true);
        }

        public abstract void Dispose(bool disposing);
    }
}
