using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using Microsoft.ServiceBus;
using Microsoft.Practices.TransientFaultHandling;

namespace ProjectExtensions.Azure.ServiceBus.AzureServiceBusFactories {

    class ServiceBusTokenProvider : IServiceBusTokenProvider {

        IBusConfiguration configuration;
        string servicePath;
        Uri serviceUri;
        TokenProvider tokenProvider;

        public ServiceBusTokenProvider(IBusConfiguration configuration) {
            Guard.ArgumentNotNull(configuration, "configuration");

            this.configuration = configuration;
            servicePath = string.Empty;
            if (!string.IsNullOrWhiteSpace(configuration.ServicePath)) {
                servicePath = configuration.ServicePath;
            }
        }

        public TokenProvider TokenProvider {
            get {
                if (tokenProvider == null) {
                    tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(configuration.ServiceBusIssuerName, configuration.ServiceBusIssuerKey);
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
