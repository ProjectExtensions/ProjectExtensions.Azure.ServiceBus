using ProjectExtensions.Azure.ServiceBus;
using ProjectExtensions.Azure.ServiceBus.Unity.Container;
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
