﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus;
using ProjectExtensions.Azure.ServiceBus.Wrappers;
using System.IO;
using NLog;

namespace ProjectExtensions.Azure.ServiceBus.AzureServiceBusFactories {

    class ServiceBusMessagingFactoryFactory : IMessagingFactory {

        static Logger logger = LogManager.GetCurrentClassLogger();
        MessagingFactory messagingFactory;

        public ServiceBusMessagingFactoryFactory(IServiceBusTokenProvider tokenProvider) {
            Guard.ArgumentNotNull(tokenProvider, "tokenProvider");
            try {
                messagingFactory = MessagingFactory.Create();
                return;
            }
            catch (Exception ex) {
                logger.Warn("Attempted to parse app.config setting Microsoft.ServiceBus.ConnectionString and it failed. Falling Back to default app settings:" + ex.Message);
            }
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

        public IBrokeredMessage CreateBrokeredMessage(Stream messageBodyStream) {
            return new BrokeredMessageWrapper(new BrokeredMessage(messageBodyStream, false));
        }
    }
}
