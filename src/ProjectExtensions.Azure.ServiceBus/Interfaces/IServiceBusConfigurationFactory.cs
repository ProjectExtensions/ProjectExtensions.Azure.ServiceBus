namespace ProjectExtensions.Azure.ServiceBus.Interfaces {

    interface IServiceBusConfigurationFactory {

        IMessagingFactory MessageFactory {
            get;
        }

        INamespaceManager NamespaceManager {
            get;
        }

    }
}
