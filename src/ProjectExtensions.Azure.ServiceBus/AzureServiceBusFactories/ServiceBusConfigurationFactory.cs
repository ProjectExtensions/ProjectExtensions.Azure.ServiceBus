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
        IMessagingFactory messageFactory;
        INamespaceManager namespaceManager;

        public ServiceBusConfigurationFactory(IBusConfiguration configuration) {
            Guard.ArgumentNotNull(configuration, "configuration");

            this.configuration = configuration;
        }

        public IMessagingFactory MessageFactory {
            get {
                if (messageFactory == null) {
                    messageFactory = configuration.Container.Resolve<IMessagingFactory>();
                }
                return messageFactory;
            }
        }

        public INamespaceManager NamespaceManager {
            get {
                if (namespaceManager == null) {
                    namespaceManager = configuration.Container.Resolve<INamespaceManager>();
                }
                return namespaceManager;
            }
        }

    }
}
