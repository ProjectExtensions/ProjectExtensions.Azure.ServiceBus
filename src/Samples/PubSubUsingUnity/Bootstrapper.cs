using ProjectExtensions.Azure.ServiceBus;
using ProjectExtensions.Azure.ServiceBus.StructureMap;
using PubSubUsingConfiguration;

namespace PubSubUsingConfiguration {
    public static class Bootstrapper {
        public static void Initialize() {
            BusConfiguration.WithSettings()
                .UseUnityContainer()
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
