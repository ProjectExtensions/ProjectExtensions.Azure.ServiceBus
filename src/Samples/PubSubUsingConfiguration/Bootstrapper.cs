using System.Configuration;
using Amazon.ServiceBus.DistributedMessages.Serializers;
using ProjectExtensions.Azure.ServiceBus;
using ProjectExtensions.Azure.ServiceBus.Autofac.Container;

namespace PubSubUsingConfiguration {
    static internal class Bootstrapper {
        public static void Initialize() {

            var setup = new ServiceBusSetupConfiguration() {
                DefaultSerializer = new GZipXmlSerializer(),
                ServiceBusIssuerKey = ConfigurationManager.AppSettings["ServiceBusIssuerKey"],
                ServiceBusIssuerName = ConfigurationManager.AppSettings["ServiceBusIssuerName"],
                ServiceBusNamespace = ConfigurationManager.AppSettings["ServiceBusNamespace"],
                ServiceBusApplicationId = "AppName"
            };

            setup.AssembliesToRegister.Add(typeof(TestMessageSubscriber).Assembly);

            BusConfiguration.WithSettings()
                .UseAutofacContainer()
                .ReadFromConfigurationSettings(setup)
                .EnablePartitioning(true)
                .DefaultSerializer(new GZipXmlSerializer())
                .Configure();

            /*
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
            */
        }
    }
}