using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace ProjectExtensions.Azure.ServiceBus.AzureServiceBusFactories {

    class ServiceBusConfigurationFactory : IServiceBusConfigurationFactory {

        IBusConfiguration configuration;

        public ServiceBusConfigurationFactory(IBusConfiguration configuration, IMessagingFactory messageFactory, INamespaceManager namespaceManager) {
            Guard.ArgumentNotNull(configuration, "configuration");
            Guard.ArgumentNotNull(messageFactory, "messageFactory");
            Guard.ArgumentNotNull(namespaceManager, "namespaceManager");
            this.configuration = configuration;
            this.MessageFactory = messageFactory;
            this.NamespaceManager = namespaceManager;
        }

        public IMessagingFactory MessageFactory {
            get;
            private set;
        }

        public INamespaceManager NamespaceManager {
            get;
            private set;
        }
    }
}
