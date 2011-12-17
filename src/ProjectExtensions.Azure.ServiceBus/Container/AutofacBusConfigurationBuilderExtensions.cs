using Autofac;

namespace ProjectExtensions.Azure.ServiceBus.Container {
    /// <summary>
    /// Extensions to allow configuration using Autofac
    /// </summary>
    public static class AutofacBusConfigurationBuilderExtensions {
        /// <summary>
        /// Initializes Autofac
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="container"></param>
        /// <returns></returns>
        public static BusConfigurationBuilder UseAutofacContainer(this BusConfigurationBuilder builder, IContainer container = null) {
            builder.configuration.container = new AutofacAzureBusContainer(container);
            return builder;
        }
        
    }
}