using Autofac;

namespace ProjectExtensions.Azure.ServiceBus.Autofac.Container {
    /// <summary>
    /// Extensions to allow configuration using Autofac
    /// </summary>
    public static class AutofacBusConfigurationBuilderExtensions {
        /// <summary>
        /// Initializes Autofac
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="container">Autofac container used in your application.  This is optional.  A new container will be created if one is not provided</param>
        /// <returns></returns>
        public static BusConfigurationBuilder UseAutofacContainer(this BusConfigurationBuilder builder, IContainer container = null) {
            builder.Configuration.Container = new AutofacAzureBusContainer(container);
            return builder;
        }
        
    }
}