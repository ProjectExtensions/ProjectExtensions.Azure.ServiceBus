using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Activation;
using Ninject.Parameters;
using ProjectExtensions.Azure.ServiceBus.Container;

namespace ProjectExtensions.Azure.ServiceBus.Ninject.Container {
    /// <summary>
    /// Ninject support for the Azure service bus.
    /// </summary>
    public class NinjectAzureBusContainer : IAzureBusContainer {
        IKernel container;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="container">Optional Ninject kernel owned by the calling application.
        /// If one is not provided, a new one will be created.</param>
        public NinjectAzureBusContainer(IKernel container = null) {
            this.container = container ?? new StandardKernel();
        }


        /// <summary>
        /// Resolve component type of T with optional arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T Resolve<T>() where T : class {
            return container.Get<T>();
        }

        /// <summary>
        /// Resolve component with optional arguments.
        /// </summary>
        /// <param name="t">The type to resolve</param>
        /// <returns></returns>
        public object Resolve(Type t) {
            return container.Get(t);
        }

        /// <summary>
        /// Register an implementation for a service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="perInstance">True creates an instance each time resolved.  False uses a singleton instance for the entire lifetime of the process.</param>
        public void Register(Type serviceType, Type implementationType, bool perInstance = false) {
            if (perInstance) {
                container.Bind(serviceType).To(implementationType).InTransientScope();
            } else {
                container.Bind(serviceType).To(implementationType).InSingletonScope();
            }
        }

        /// <summary>
        /// Registers the configuration instance with the bus if it is not already registered
        /// </summary>
        public void RegisterConfiguration() {
            if (!IsRegistered(typeof(IBusConfiguration))) {
                container.Bind<IBusConfiguration>().ToConstant(BusConfiguration.Instance).InSingletonScope();
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
            return container.CanResolve(container.CreateRequest(type, null, new List<IParameter>(), false, false));
        }
    }
}
