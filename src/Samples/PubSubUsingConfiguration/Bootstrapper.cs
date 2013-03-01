using Amazon.ServiceBus.DistributedMessages.Serializers;
using ProjectExtensions.Azure.ServiceBus;
using ProjectExtensions.Azure.ServiceBus.Autofac.Container;

namespace PubSubUsingConfiguration {
    static internal class Bootstrapper {
        public static void Initialize() {
            BusConfiguration.WithSettings()
                .UseAutofacContainer()
                .ReadFromConfigFile()
                .ServiceBusApplicationId("AppName")
                .DefaultSerializer(new GZipXmlSerializer())
                //.ServiceBusIssuerKey("[sb password]")
                //.ServiceBusIssuerName("owner")
                //.ServiceBusNamespace("[addresshere]")
                .RegisterAssembly(typeof(TestMessageSubscriber).Assembly)
                .Configure();
        }
    }
}