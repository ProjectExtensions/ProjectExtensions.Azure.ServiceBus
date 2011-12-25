using ProjectExtensions.Azure.ServiceBus;
using ProjectExtensions.Azure.ServiceBus.Ninject.Container;

namespace PubSubUsingConfiguration {
    static internal class Bootstrapper {
        public static void Initialize() {
            BusConfiguration.WithSettings()
                .UseNinjectContainer()
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