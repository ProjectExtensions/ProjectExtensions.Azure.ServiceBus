using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using ProjectExtensions.Azure.ServiceBus.Serialization;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// ServiceBusSetupConfiguration (Default Implementation)
    /// </summary>
    public class ServiceBusSetupConfiguration : IServiceBusSetupConfiguration {

        /// <summary>
        /// new ServiceBusSetupConfiguration
        /// </summary>
        public ServiceBusSetupConfiguration() {
            AssembliesToRegister = new List<Assembly>();
            TypesToRegister = new List<Type>();
        }

        /// <summary>
        /// Set the DefaultSerializer
        /// </summary>
        public IServiceBusSerializer DefaultSerializer {
            get;
            set;
        }

        /// <summary>
        /// Auto discover all of the Subscribers in the assembly.
        /// </summary>
        public List<Assembly> AssembliesToRegister {
            get;
            private set;
        }

        /// <summary>
        /// Register subscriber by type
        /// </summary>
        public List<Type> TypesToRegister {
            get;
            private set;
        }

        /// <summary>
        /// ServiceBusApplicationId
        /// </summary>
        public string ServiceBusApplicationId {
            get;
            set;
        }

        /// <summary>
        /// ServiceBusNamespace (required)
        /// </summary>
        public string ServiceBusNamespace {
            get;
            set;
        }

        /// <summary>
        /// ServiceBusIssuerName (required)
        /// </summary>
        public string ServiceBusIssuerName {
            get;
            set;
        }

        /// <summary>
        /// ServiceBusIssuerKey (required)
        /// </summary>
        public string ServiceBusIssuerKey {
            get;
            set;
        }

        /// <summary>
        /// ServicePath
        /// </summary>
        public string ServicePath {
            get;
            set;
        }

        /// <summary>
        /// TopicName
        /// </summary>
        public string TopicName {
            get;
            set;
        }
    }
}
