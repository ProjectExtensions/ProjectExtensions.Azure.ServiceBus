using Ninject;

namespace ProjectExtensions.Azure.ServiceBus.Ninject.Container {
    /// <summary>
    /// Extensions to allow use of the Ninject IoC container.
    /// </summary>
    public static class NinjectBusConfigurationBuilderExtensions {
        /// <summary>
        /// Initializes Autofac
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="container">Ninject kernel used in your application.  This is optional.  A new kernel will be created if one is not provided</param>
        /// <returns></returns>
        public static BusConfigurationBuilder UseNinjectContainer(this BusConfigurationBuilder builder, IKernel container = null) {
            builder.Configuration.Container = new NinjectAzureBusContainer(container);
            return builder;
        }
    }
}
