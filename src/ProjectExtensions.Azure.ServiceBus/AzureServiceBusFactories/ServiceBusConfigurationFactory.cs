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
        MessagingFactory messageFactory;
        NamespaceManager namespaceManager;
        string servicePath;
        Uri serviceUri;
        TokenProvider tokenProvider;

        public ServiceBusConfigurationFactory(IBusConfiguration configuration) {
            Guard.ArgumentNotNull(configuration, "configuration");

            this.configuration = configuration;
            servicePath = string.Empty;
            if (!string.IsNullOrWhiteSpace(configuration.ServicePath)) {
                servicePath = configuration.ServicePath;
            }

        }

        public MessagingFactory MessageFactory {
            get {
                if (messageFactory == null) {
                    messageFactory = MessagingFactory.Create(ServiceUri, TokenProvider);
                }
                return messageFactory;
            }
        }

        public NamespaceManager NamespaceManager {
            get {
                if (namespaceManager == null) {
                    namespaceManager = new NamespaceManager(ServiceUri, TokenProvider);
                }
                return namespaceManager;
            }
        }

        public TokenProvider TokenProvider {
            get {
                if (tokenProvider == null) {
                    tokenProvider = TokenProvider.CreateSharedSecretTokenProvider(configuration.ServiceBusIssuerName, configuration.ServiceBusIssuerKey);
                }
                return tokenProvider;
            }
        }

        public Uri ServiceUri {
            get {
                if (serviceUri == null) {
                    serviceUri = ServiceBusEnvironment.CreateServiceUri("sb", configuration.ServiceBusNamespace, servicePath);
                }
                return serviceUri;
            }
        }
    }
}
