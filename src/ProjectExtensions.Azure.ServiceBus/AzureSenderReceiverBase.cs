using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;
using NLog;

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
            Helpers.Execute(() => EnsureTopic(configuration.TopicName));
        }

        protected void EnsureTopic(string topicName) {
            if (!namespaceManager.TopicExists(topicName)) {
                topic = namespaceManager.CreateTopic(topicName);
                logger.Log(LogLevel.Info, "EnsureTopic Create {0} ", topicName);
            }
            else {
                topic = namespaceManager.GetTopic(topicName);
                logger.Log(LogLevel.Info, "EnsureTopic Exists {0} ", topicName);
            }
        }

        public void Dispose() {
            Dispose(true);
        }

        public abstract void Dispose(bool disposing);
    }
}
