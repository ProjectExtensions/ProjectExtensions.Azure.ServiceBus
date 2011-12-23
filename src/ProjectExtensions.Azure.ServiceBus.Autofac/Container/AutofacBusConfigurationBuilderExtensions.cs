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
        /// <param name="container"></param>
        /// <returns></returns>
        public static BusConfigurationBuilder UseAutofacContainer(this BusConfigurationBuilder builder, IContainer container = null) {
            builder.Configuration.Container = new AutofacAzureBusContainer(container);
            return builder;
        }
        
    }
}