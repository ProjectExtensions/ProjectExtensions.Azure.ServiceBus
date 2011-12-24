using StructureMap;

namespace ProjectExtensions.Azure.ServiceBus.StructureMap.Container {
    /// <summary>
    /// Extensions to allow configuration using Castle Windsor.
    /// </summary>
    public static class StructureMapBusConfigurationBuilderExtensions {
        /// <summary>
        /// Initializes Structure Map
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="container">Structure Map container used in your application.  This is optional.  A new container will be created if one is not provided</param>
        /// <returns></returns>
        public static  BusConfigurationBuilder UseStructureMapContainer(this BusConfigurationBuilder builder, IContainer container = null) {
            builder.Configuration.Container = new StructureMapBusContainer(container);
            return builder;
        }
    }
}
