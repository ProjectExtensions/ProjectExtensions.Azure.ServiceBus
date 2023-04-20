using Autofac;
using ProjectExtensions.Azure.ServiceBus.Container;
using System;
using System.Collections.Generic;

namespace ProjectExtensions.Azure.ServiceBus.Autofac.Container {
    /// <summary>
    /// Implementation of <see cref="IAzureBusContainer"/> for Autofac
    /// </summary>
    public class AutofacAzureBusContainer : AzureBusContainerBase {
        IContainer container;
        ContainerBuilder builder = new ContainerBuilder();
        List<Type> registeredTypes = new List<Type>();

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="container">Optional autofac container owned by the calling application.
        /// If one is not provided, a new one will be created.</param>
        public AutofacAzureBusContainer(IContainer container = null) {
            this.container = container;
        }

        /// <summary>
        /// Resolve component type of T with optional arguments.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T Resolve<T>() {
            return container.Resolve<T>();
        }

        /// <summary>
        /// Resolve component with optional arguments.
        /// </summary>
        /// <param name="t">The type to resolve.</param>
        /// <returns></returns>
        public override object Resolve(Type t) {
            return container.Resolve(t);
        }

        /// <summary>
        /// Register an implementation for a service type.
        /// </summary>
        /// <param name="serviceType">The service type.</param>
        /// <param name="implementationType">The implementation type.</param>
        /// <param name="perInstance">True creates an instance each time resolved.  False uses a singleton instance for the entire lifetime of the process.</param>
        public override void Register(Type serviceType, Type implementationType, bool perInstance = false) {
            var reg = builder.RegisterType(implementationType).As(serviceType);
            if (perInstance) {
                reg.InstancePerDependency();
            }
            else {
                reg.SingleInstance();
            }
            registeredTypes.Add(serviceType);
        }

        /// <summary>
        /// Registers the configuration instance with the bus if it is not already registered
        /// </summary>
        public override void RegisterConfiguration() {
            if (!IsRegistered(typeof(IBusConfiguration))) {
                builder.Register(item => BusConfiguration.Instance).As<IBusConfiguration>().SingleInstance();
            }
        }

        /// <summary>
        /// Build the container if needed.
        /// </summary>
        public override void Build() {
            if (container == null) {
                container = builder.Build();
            }
            else {
                builder.Update(container);
            }
            builder = new ContainerBuilder();
            registeredTypes.Clear();
        }

        /// <summary>
        /// Return true if the given type is registered with the container.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public override bool IsRegistered(Type type) {
            if (container != null) {
                return container.IsRegistered(type) || registeredTypes.Contains(type);
            }
            else {
                return registeredTypes.Contains(type);
            }
        }
    }
}
