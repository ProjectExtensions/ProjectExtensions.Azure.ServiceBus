﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;
using Microsoft.Practices.TransientFaultHandling;

namespace ProjectExtensions.Azure.ServiceBus.AzureServiceBusFactories {

    class ServiceBusNamespaceManagerFactory : INamespaceManager {

        NamespaceManager namespaceManager;

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

        public void Initialize(Uri serviceUri, TokenProvider tokenProvider) {
            Guard.ArgumentNotNull(serviceUri, "tokenProvider");
            Guard.ArgumentNotNull(serviceUri, "tokenProvider");
            namespaceManager = new NamespaceManager(serviceUri, tokenProvider);
        }
    }
}
