using System;
using System.Collections.Generic;
using ProjectExtensions.Azure.ServiceBus;
using NLog.Config;
using NLog.Targets;
using NLog;
using System.Diagnostics;
using System.Transactions;

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

            //This sets up the bus configuration for the application.
            Bootstrapper.Initialize();

            //put 4 messages in a transaction
            using (var scope = new TransactionScope()) {
                for (int i = 0; i < 4; i++) {
                    var sw = new Stopwatch();
                    sw.Start();
                    var message1 = new TestMessage() {
                        Value = 1000 + i,
                        MessageId = DateTime.Now.ToString()
                    };
                    var values = new Dictionary<string, object>();
                    values.Add("hello", i);

                    BusConfiguration.Instance.Bus.Publish(message1, values);
                    sw.Stop();
                    Debug.WriteLine("sync:" + sw.Elapsed);
                    Console.WriteLine("sync:" + sw.Elapsed);
                }
                scope.Complete();
            }

            for (int i = 1; i <= 20; i++) {
                var sw = new Stopwatch();
                sw.Start();
                var message1 = new TestMessage() {
                    Value = i,
                    MessageId = DateTime.Now.ToString()
                };
                //BusConfiguration.Instance.Bus.PublishAsync(message1, (result) => {
                //    Console.WriteLine(result.TimeSpent);
                //}, null);
                BusConfiguration.Instance.Bus.Publish(message1, null); //Optional Dictionary of name value pairs to pass with the massage. Can be used for filtering
                sw.Stop();
                Debug.WriteLine("sync:" + sw.Elapsed);
                Console.WriteLine("sync:" + sw.Elapsed);
            }

            for (int i = 1; i <= 20; i++) {
                var message2 = new AnotherTestMessage() {
                    Value = i,
                    MessageId = DateTime.Now.ToString()
                };
                BusConfiguration.Instance.Bus.PublishAsync(message2, (result) => {
                    if (!result.IsSuccess) { 
                        //message failed.
                    }
                    Debug.WriteLine("async:" + result.TimeSpent);
                    Console.WriteLine("async:" + result.TimeSpent);
                }, null); //Optional Dictionary of name value pairs to pass with the massage. Can be used for filtering
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
            consoleTarget.Layout = "${date:format=HH\\:mm\\:ss} ${logger} ${message}";
            fileTarget.Layout = "${date:format=HH\\:mm\\:ss} ${message}";

            // Step 4. Define rules
            LoggingRule rule = new LoggingRule("*", LogLevel.Info, consoleTarget);
            config.LoggingRules.Add(rule);

            rule = new LoggingRule("*", LogLevel.Debug, fileTarget);
            config.LoggingRules.Add(rule);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;
        }
    }
}
