using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus;
using NLog;

namespace PubSubUsingConfiguration {

    public class AnotherTestMessageSubscriber : IHandleCompetingMessages<AnotherTestMessage> {

        static Logger logger = LogManager.GetCurrentClassLogger();

        public bool IsReusable {
            get {
                return true;
            }
        }

        public void Handle(IReceivedMessage<AnotherTestMessage> message, IDictionary<string, object> metadata) {
            logger.Log(LogLevel.Info, "Message received: {0} {1}", message.Message.Value, message.Message.MessageId);
        }
    }
}
