using Microsoft.Practices.TransientFaultHandling;
using ProjectExtensions.Azure.ServiceBus.Interfaces;

namespace ProjectExtensions.Azure.ServiceBus.Factories {

    class GenericServiceBusConfigurationFactory : IServiceBusConfigurationFactory {

        IBusConfiguration configuration;

        public GenericServiceBusConfigurationFactory(IBusConfiguration configuration, IMessagingFactory messageFactory, INamespaceManager namespaceManager) {
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
