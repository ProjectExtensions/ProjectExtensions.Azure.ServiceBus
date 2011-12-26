using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus;
using ProjectExtensions.Azure.ServiceBus.Wrappers;

namespace ProjectExtensions.Azure.ServiceBus.AzureServiceBusFactories {

    class ServiceBusMessagingFactoryFactory : IMessagingFactory {

        MessagingFactory messagingFactory;

        public ServiceBusMessagingFactoryFactory(IServiceBusTokenProvider tokenProvider) {
            Guard.ArgumentNotNull(tokenProvider, "tokenProvider");
            messagingFactory = MessagingFactory.Create(tokenProvider.ServiceUri, tokenProvider.TokenProvider);
        }

        public ISubscriptionClient CreateSubscriptionClient(string topicPath, string name, ReceiveMode receiveMode) {
            return new SubscriptionClientWrapper(messagingFactory.CreateSubscriptionClient(topicPath, name, receiveMode));
        }

        public ITopicClient CreateTopicClient(string path) {
            return new TopicClientWrapper(messagingFactory.CreateTopicClient(path));
        }

        public void Close() {
            messagingFactory.Close();
        }

    }
}
