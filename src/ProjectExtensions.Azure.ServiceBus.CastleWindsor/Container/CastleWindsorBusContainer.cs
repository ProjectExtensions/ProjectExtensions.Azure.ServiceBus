using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using ProjectExtensions.Azure.ServiceBus.Container;

namespace ProjectExtensions.Azure.ServiceBus.CastleWindsor.Container {
    public class CastleWindsorBusContainer : IAzureBusContainer {
        IWindsorContainer container;
  
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="container">Optional Castle Windsor container.  If one is not provided,
        /// a new one will be created.</param>
        public CastleWindsorBusContainer(IWindsorContainer container = null) {
            this.container = container ?? new WindsorContainer();
        }
        /// <summary>
        /// Resolve component type of T with optional arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>() where T : class {
            return container.Resolve<T>();
        }

        /// <summary>
        /// Resolve component with optional arguments.
        /// </summary>
        /// <param name="t">The type to resolve</param>
        /// <returns></returns>
        public object Resolve(Type t) {
            return container.Resolve(t);
        }

        /// <summary>
        /// Register an implementation for a service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="perInstance">True creates an instance each time resolved.  False uses a singleton instance for the entire lifetime of the process.</param>
        public void Register(Type serviceType, Type implementationType, bool perInstance = false) {
            if (perInstance) {
                container.Register(Component.For(serviceType).ImplementedBy(implementationType).LifestyleTransient());
            } else {
                container.Register(Component.For(serviceType).ImplementedBy(implementationType).LifestyleSingleton());
            }
        }

        /// <summary>
        /// Registers the configuration instance with the bus if it is not already registered
        /// </summary>
        public void RegisterConfiguration() {
            if (!IsRegistered(typeof(IBusConfiguration))) {
                container.Register(Component.For<IBusConfiguration>().Instance(BusConfiguration.Instance).LifestyleSingleton());
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
            return container.Kernel.HasComponent(type.FullName);
        }
    }
}
