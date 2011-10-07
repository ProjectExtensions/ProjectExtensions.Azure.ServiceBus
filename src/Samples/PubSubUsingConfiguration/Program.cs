using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using ProjectExtensions.Azure.ServiceBus;
using System.Threading.Tasks;
using NLog.Config;
using NLog.Targets;
using NLog;
using System.Diagnostics;

namespace PubSubUsingConfiguration {

    class Program {

        static void Main(string[] args) {

            SetupLogging();

            //you can either call .ReadFromConfigFile() or set the ServiceSub Methods in code.
            //
            //<add key="ServiceBusIssuerKey" value="base64hash" />
            //<add key="ServiceBusIssuerName" value="owner" />
            //https://addresshere.servicebus.windows.net/
            //<add key="ServiceBusNamespace" value="namespace set up in service bus (addresshere) portion" />

            ProjectExtensions.Azure.ServiceBus.BusConfiguration.WithSettings()
                .ReadFromConfigFile()
                .ServiceBusApplicationId("AppName")
                //.ServiceBusIssuerKey("[sb password]")
                //.ServiceBusIssuerName("owner")
                //.ServiceBusNamespace("[addresshere]")
                .RegisterAssembly(typeof(TestMessageSubscriber).Assembly)
                .Configure();

            for (int i = 0; i < 20; i++) {
                var sw = new Stopwatch();
                sw.Start();
                var message1 = new TestMessage() {
                    Value = i,
                    MessageId = DateTime.Now.ToString()
                };
                //BusConfiguration.Instance.Bus.PublishAsync(message1, (result) => {
                //    Console.WriteLine(result.TimeSpent);
                //}, null);
                BusConfiguration.Instance.Bus.Publish(message1, null);
                sw.Stop();
                Debug.WriteLine("sync:" + sw.Elapsed);
                Console.WriteLine("sync:" + sw.Elapsed);
            }

            for (int i = 0; i < 20; i++) {
                var message2 = new AnotherTestMessage() {
                    Value = 2 + i,
                    MessageId = DateTime.Now.ToString()
                };
                BusConfiguration.Instance.Bus.PublishAsync(message2, (result) => {
                    Debug.WriteLine("async:" + result.TimeSpent);
                    Console.WriteLine("async:" + result.TimeSpent);
                }, null);
            }

            Console.WriteLine("You must wait for the messages to be processed");

            Console.WriteLine("Press any key to continue");

            Console.ReadLine();
        }

        private static void SetupLogging() {
            // Step 1. Create configuration object 
            LoggingConfiguration config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            ColoredConsoleTarget consoleTarget = new ColoredConsoleTarget();
            config.AddTarget("console", consoleTarget);

            FileTarget fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);
            fileTarget.FileName = "${basedir}/file.txt";

            // Step 3. Set target properties 
            consoleTarget.Layout = "${date:format=HH\\:MM\\:ss} ${logger} ${message}";
            fileTarget.Layout = "${date:format=HH\\:MM\\:ss} ${message}";

            // Step 4. Define rules
            LoggingRule rule = new LoggingRule("*", LogLevel.Debug, consoleTarget);
            config.LoggingRules.Add(rule);

            rule = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }
    }
}
