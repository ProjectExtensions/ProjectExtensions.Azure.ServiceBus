using ProjectExtensions.Azure.ServiceBus.Serialization;
using System;

namespace ProjectExtensions.Azure.ServiceBus.Container {
    /// <summary>
    /// Generic IOC container interface
    /// </summary>
    public interface IAzureBusContainer {
        /// <summary>
        /// Resolve component type of T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>() where T : class;
        /// <summary>
        /// Resolve component.
        /// </summary>
        /// <param name="t">The type to resolve</param>
        /// <returns></returns>
        object Resolve(Type t);
        /// <summary>
        /// Register an implementation for a service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="perInstance">True creates an instance each time resolved.  False uses a singleton instance for the entire lifetime of the process.</param>
        void Register(Type serviceType, Type implementationType, bool perInstance = false);

        /// <summary>
        /// Registers the configuration instance with the bus if it is not already registered
        /// </summary>
        void RegisterConfiguration();
        /// <summary>
        /// Build the container if needed.
        /// </summary>
        void Build();
        /// <summary>
        /// Return true if the given type is registered with the container.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        bool IsRegistered(Type type);

        /// <summary>
        /// Return the Service Bus
        /// </summary>
        IBus Bus {
            get;
        }

        /// <summary>
        /// Resolve the Default Serializer
        /// </summary>
        IServiceBusSerializer DefaultSerializer {
            get;
        }

    }
}
