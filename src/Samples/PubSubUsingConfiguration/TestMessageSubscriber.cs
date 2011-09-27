using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus;
using NLog;

namespace PubSubUsingConfiguration {

    public class TestMessageSubscriber : IHandleMessages<TestMessage> {

        static Logger logger = LogManager.GetCurrentClassLogger();

        public void Handle(TestMessage message, IDictionary<string, object> metadata) {
            logger.Log(LogLevel.Info, "Message received: {0} {1}", message.Value, message.MessageId);
        }

        public bool IsReusable {
            get {
                return false;
            }
        }

    }
}
