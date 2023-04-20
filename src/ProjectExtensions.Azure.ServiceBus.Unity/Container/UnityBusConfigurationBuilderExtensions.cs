using Microsoft.Practices.Unity;

namespace ProjectExtensions.Azure.ServiceBus.Unity.Container {
    /// <summary>
    /// Extensions to allow configuration to use the Unity IoC container.
    /// </summary>
    public static class UnityBusConfigurationBuilderExtensions {
        /// <summary>
        /// Initializes Unity
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="container">Unity container used in your application.  This is optional.  A new container will be created if one is not provided</param>
        /// <returns></returns>
        public static BusConfigurationBuilder UseUnityContainer(this BusConfigurationBuilder builder, IUnityContainer container = null) {
            builder.Configuration.Container = new UnityAzureBusContainer(container);
            return builder;
        }
    }
}
