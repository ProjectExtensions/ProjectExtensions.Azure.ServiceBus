using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus;
using NLog;
using Microsoft.ServiceBus.Messaging;

namespace PubSubUsingConfiguration {

    [MessageHandlerConfiguration(
        Singleton = true)]
    public class TestMessageSubscriber : IHandleMessages<TestMessage> {

        static Logger logger = LogManager.GetCurrentClassLogger();

        public void Handle(IReceivedMessage<TestMessage> message, IDictionary<string, object> metadata) {
            logger.Info("TestMessageSubscriber Message received: {0} {1}", message.Message.Value, message.Message.MessageId);

            var newMessage = new AnotherTestMessage() {
                MessageId = message.Message.MessageId,
                Value = message.Message.Value
            };
            BusConfiguration.Instance.Bus.PublishAsync(newMessage, (result) => {
                Console.WriteLine(result.TimeSpent);
            }, null);
            //BusConfiguration.Instance.Bus.Publish(newMessage, null);
        }
    }
}
