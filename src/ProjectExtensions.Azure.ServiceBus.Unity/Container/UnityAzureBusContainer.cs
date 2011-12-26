using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Practices.Unity;
using ProjectExtensions.Azure.ServiceBus.Container;

namespace ProjectExtensions.Azure.ServiceBus.Unity.Container {
    public class UnityAzureBusContainer : IAzureBusContainer {
        IUnityContainer container;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="container">Unity container used in your application.  This is optional.  A new container will be created if one is not provided.</param>
        public UnityAzureBusContainer(IUnityContainer container = null) {
            this.container = container ?? new UnityContainer();
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
                container.RegisterType(serviceType, implementationType, new TransientLifetimeManager());
            } else {
                container.RegisterType(serviceType, implementationType, new ContainerControlledLifetimeManager());
            }
        }

        /// <summary>
        /// Registers the configuration instance with the bus if it is not already registered
        /// </summary>
        public void RegisterConfiguration() {
            if (!IsRegistered(typeof(IBusConfiguration))) {
                container.RegisterInstance<IBusConfiguration>(BusConfiguration.Instance, new ContainerControlledLifetimeManager());
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
            return container.IsRegistered(type);
        }
    }
}
