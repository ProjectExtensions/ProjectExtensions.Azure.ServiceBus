using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ProjectExtensions.Azure.ServiceBus.Autofac.Container;
using Autofac;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Mocks;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Messages;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Interfaces;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit {

    [SetUpFixture]
    public class Config {

        [SetUp]
        public void SetUp() {

            var builder = new ContainerBuilder();
            //IContainer container = Autofac.Container.

            builder.RegisterType(typeof(MockTopicClient)).As(typeof(ITopicClient)).SingleInstance();
            builder.RegisterType(typeof(MockNamespaceManager)).As(typeof(INamespaceManager)).SingleInstance();
            builder.RegisterType(typeof(MockMessagingFactory)).As(typeof(IMessagingFactory)).SingleInstance();
            builder.RegisterType(typeof(MockServiceBus)).As(typeof(IMockServiceBus)).SingleInstance();

            BusConfiguration.WithSettings()
                                        .UseAutofacContainer(builder.Build())
                                        .ServiceBusApplicationId("AppName")
                                        .TopicName("test")
                //.RegisterAssembly(typeof(TestMessageSubscriber).Assembly)
                                        .Configure();

            //test send a message

            BusConfiguration.Instance.Bus.PublishAsync(new TestMessageForTesting(), (callback) => {
                Console.WriteLine("Time Spent:" + callback.TimeSpent);
            });

            Console.ReadLine();
        }

        [TearDown]
        public void TearDown() {

        }
    }
}
