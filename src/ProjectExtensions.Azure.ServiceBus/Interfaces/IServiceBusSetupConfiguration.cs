using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Serialization;

namespace ProjectExtensions.Azure.ServiceBus.Interfaces {

    /// <summary>
    /// Interface used to define the settings we can pull from the user instead of them being hard coded in the web/app.config file or passed in as parameters.
    /// </summary>
    public interface IServiceBusSetupConfiguration {

        /// <summary>
        /// Set the DefaultSerializer
        /// </summary>
        IServiceBusSerializer DefaultSerializer { get; }

        /// <summary>
        /// Auto discover all of the Subscribers in the assembly.
        /// </summary>
        List<Assembly> AssembliesToRegister { get; }

        /// <summary>
        /// Register subscriber by type
        /// </summary>
        List<Type> TypesToRegister { get; }

        /// <summary>
        /// ServiceBusApplicationId
        /// </summary>
        string ServiceBusApplicationId { get; }

        /// <summary>
        /// ServiceBusNamespace (required)
        /// </summary>
        string ServiceBusNamespace { get; }

        /// <summary>
        /// ServiceBusIssuerName (required)
        /// </summary>
        string ServiceBusIssuerName { get; }

        /// <summary>
        /// ServiceBusIssuerKey (required)
        /// </summary>
        string ServiceBusIssuerKey { get; }

        /// <summary>
        /// ServicePath
        /// </summary>
        string ServicePath { get; }

        /// <summary>
        /// TopicName
        /// </summary>
        string TopicName { get; }
    }
}
