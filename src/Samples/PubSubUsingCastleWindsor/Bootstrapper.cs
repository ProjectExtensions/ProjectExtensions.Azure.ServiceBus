using ProjectExtensions.Azure.ServiceBus;
using ProjectExtensions.Azure.ServiceBus.CastleWindsor.Container;
using PubSubUsingConfiguration;

namespace PubSubUsingConfiguration {
    public static class Bootstrapper {
        public static void Initialize() {
            BusConfiguration.WithSettings()
                .UseCastleWindsorContainer()
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
