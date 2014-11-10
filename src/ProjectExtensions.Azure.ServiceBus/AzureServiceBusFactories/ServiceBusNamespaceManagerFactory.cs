using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;
using Microsoft.Practices.TransientFaultHandling;
using NLog;

namespace ProjectExtensions.Azure.ServiceBus.AzureServiceBusFactories {

    class ServiceBusNamespaceManagerFactory : INamespaceManager {

        static Logger logger = LogManager.GetCurrentClassLogger();

        NamespaceManager namespaceManager;

        public ServiceBusNamespaceManagerFactory(IServiceBusTokenProvider tokenProvider) {
            Guard.ArgumentNotNull(tokenProvider, "tokenProvider");
            try {
                namespaceManager = NamespaceManager.Create();
                return;
            }
            catch (Exception ex) {
                logger.Warn("Attempted to parse app.config setting Microsoft.ServiceBus.ConnectionString and it failed. Falling Back to default app settings:" + ex.Message);
            }
            namespaceManager = new NamespaceManager(tokenProvider.ServiceUri, tokenProvider.TokenProvider);
        }

        public SubscriptionDescription CreateSubscription(SubscriptionDescription description, Filter filter) {
            return namespaceManager.CreateSubscription(description, filter);
        }

        public TopicDescription CreateTopic(TopicDescription description) {
            return namespaceManager.CreateTopic(description);
        }

        public void DeleteSubscription(string topicPath, string name) {
            namespaceManager.DeleteSubscription(topicPath, name);
        }

        public SubscriptionDescription GetSubscription(string topicPath, string name) {
            return namespaceManager.GetSubscription(topicPath, name);
        }

        public TopicDescription GetTopic(string path) {
            return namespaceManager.GetTopic(path);
        }

        public bool SubscriptionExists(string topicPath, string name) {
            return namespaceManager.SubscriptionExists(topicPath, name);
        }

    }
}
