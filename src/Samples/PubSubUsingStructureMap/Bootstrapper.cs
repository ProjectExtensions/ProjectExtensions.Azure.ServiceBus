using ProjectExtensions.Azure.ServiceBus;
using ProjectExtensions.Azure.ServiceBus.StructureMap.Container;

namespace PubSubUsingConfiguration {
    public static class Bootstrapper {
        public static void Initialize() {
            BusConfiguration.WithSettings()
                .UseStructureMapContainer()
                .ReadFromConfigFile()
                .ServiceBusApplicationId("AppName")
                //.ServiceBusIssuerKey("[sb password]")
                //.ServiceBusIssuerName("owner")
                //.ServiceBusNamespace("[addresshere]")
                .RegisterAssembly(typeof(TestMessageSubscriber).Assembly)
                .Configure();
        }
    }
}
