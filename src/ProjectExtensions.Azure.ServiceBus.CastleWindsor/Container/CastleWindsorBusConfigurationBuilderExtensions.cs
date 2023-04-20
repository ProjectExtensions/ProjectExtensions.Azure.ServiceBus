using Castle.Windsor;

namespace ProjectExtensions.Azure.ServiceBus.CastleWindsor.Container {
    /// <summary>
    /// Extensions to allow configuration using Castle Windsor.
    /// </summary>
    public static class CastleWindsorBusConfigurationBuilderExtensions {
        /// <summary>
        /// Initializes Castle Windsor
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="container">Castle Windsor container used in your application.  This is optional.  A new container will be created if one is not provided</param>
        /// <returns></returns>
        public static BusConfigurationBuilder UseCastleWindsorContainer(this BusConfigurationBuilder builder, IWindsorContainer container = null) {
            builder.Configuration.Container = new CastleWindsorBusContainer(container);
            return builder;
        }
    }
}
