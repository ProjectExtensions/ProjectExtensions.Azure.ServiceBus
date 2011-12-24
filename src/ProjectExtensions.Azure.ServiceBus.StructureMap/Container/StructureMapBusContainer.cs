using System;
using ProjectExtensions.Azure.ServiceBus.Container;
using StructureMap;
using StructureMap.Pipeline;

namespace ProjectExtensions.Azure.ServiceBus.StructureMap.Container {
    /// <summary>
    /// Implementation of <see cref="IAzureBusContainer"/> for Structure Map.
    /// </summary>
    public class StructureMapBusContainer : IAzureBusContainer {
        IContainer container;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="container">Optional StructueMap container.  If one is not provided, a new one will be created.</param>
        public StructureMapBusContainer(IContainer container = null) {
            this.container = container ?? new global::StructureMap.Container();
        }
        /// <summary>
        /// Resolve component type of T with optional arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>() where T : class {
            return container.GetInstance<T>();
        }

        /// <summary>
        /// Resolve component with optional arguments.
        /// </summary>
        /// <param name="t">The type to resolve</param>
        /// <returns></returns>
        public object Resolve(Type t) {
            return container.GetInstance(t);
        }

        /// <summary>
        /// Register an implementation for a service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="perInstance">True creates an instance each time resolved.  False uses a singleton instance for the entire lifetime of the process.</param>
        public void Register(Type serviceType, Type implementationType, bool perInstance = false) {
            ILifecycle lifecycle = perInstance ? (ILifecycle) new UniquePerRequestLifecycle() : (ILifecycle) new SingletonLifecycle();
            container.Configure(c => c.For(serviceType).LifecycleIs(lifecycle).Use(implementationType));
        }

        /// <summary>
        /// Registers the configuration instance with the bus if it is not already registered
        /// </summary>
        public void RegisterConfiguration() {
            if (!IsRegistered(typeof(IBusConfiguration))) {
                container.Configure(c => c.For<IBusConfiguration>().LifecycleIs(new SingletonLifecycle()).Use(() => BusConfiguration.Instance));
            }
        }

        /// <summary>
        /// Build the container if needed.
        /// </summary>
        public void Build() {
            //do nothing
        }

        /// <summary>
        /// Return true if the given type is registered with the container.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public bool IsRegistered(Type type) {
            return container.Model.HasDefaultImplementationFor(type);
        }
    }
}
