using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus;

namespace ProjectExtensions.Azure.ServiceBus.AzureServiceBusFactories {

    class ServiceBusMessagingFactoryFactory : IMessagingFactory {

        MessagingFactory messagingFactory;

        public SubscriptionClient CreateSubscriptionClient(string topicPath, string name, ReceiveMode receiveMode) {
            return messagingFactory.CreateSubscriptionClient(topicPath, name, receiveMode);
        }

        public TopicClient CreateTopicClient(string path) {
           return messagingFactory.CreateTopicClient(path);
        }

        public void Close() {
            messagingFactory.Close();
        }

        public void Initialize(Uri serviceUri, TokenProvider tokenProvider) {
            Guard.ArgumentNotNull(serviceUri, "tokenProvider");
            Guard.ArgumentNotNull(serviceUri, "tokenProvider");
            messagingFactory = MessagingFactory.Create(serviceUri, tokenProvider);
        }
    }
}
