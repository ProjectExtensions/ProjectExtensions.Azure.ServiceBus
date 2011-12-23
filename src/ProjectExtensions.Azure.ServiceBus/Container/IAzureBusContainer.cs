using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectExtensions.Azure.ServiceBus.Container
{
    /// <summary>
    /// Generic IOC container interface
    /// </summary>
    public interface IAzureBusContainer {
        /// <summary>
        /// Resolve component type of T with optional arguments.
        /// </summary>
        /// <param name="parms">Additional parameters to be passed to the constructor.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T Resolve<T>(params KeyValuePair<string, object>[] parms) where T : class;
        /// <summary>
        /// Resolve component with optional arguments.
        /// </summary>
        /// <param name="parms">Additional parameters to be passed to the constructor.</param>
        /// <param name="t">The type to resolve</param>
        /// <returns></returns>
        object Resolve(Type t, params KeyValuePair<string, object>[] parms);
        /// <summary>
        /// Register an implementation for a service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="perInstance">True creates an instance each time resolved.  False uses a singleton instance for the entire lifetime of the process.</param>
        void Register(Type serviceType, Type implementationType, bool perInstance = false);
        /// <summary>
        /// Register the bus
        /// </summary>
        /// <param name="busConfiguration">The configuration instance to use on the bus.</param>
        void RegisterBus(BusConfiguration busConfiguration);
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

    }
}
