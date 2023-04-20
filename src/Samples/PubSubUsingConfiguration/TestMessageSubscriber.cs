
/* Unmerged change from project 'PubSubUsingCastleWindsor'
Before:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus;
using NLog;
After:
using NLog;
using ProjectExtensions.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Azure.ServiceBus;
using System.Text;
*/

/* Unmerged change from project 'PubSubUsingStructureMap'
Before:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus;
using NLog;
After:
using NLog;
using ProjectExtensions.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Azure.ServiceBus;
using System.Text;
*/

/* Unmerged change from project 'PubSubUsingUnity'
Before:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus;
using NLog;
After:
using NLog;
using ProjectExtensions.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Azure.ServiceBus;
using System.Text;
*/

/* Unmerged change from project 'PubSubUsingNinject'
Before:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus;
using NLog;
After:
using NLog;
using ProjectExtensions.Azure.ServiceBus;
using System;
using System.Collections.Generic;
using System.Azure.ServiceBus;
using System.Text;
*/
using NLog;
using ProjectExtensions.Azure.ServiceBus;

namespace PubSubUsingConfiguration {

    [MessageHandlerConfiguration(
        DeadLetterAfterMaxRetries = true,
        MaxRetries = 2,
        PauseTimeIfErrorWasThrown = 15000,
        Singleton = true)]
    public class TestMessageSubscriber : IHandleMessages<TestMessage> {

        static Logger logger = LogManager.GetCurrentClassLogger();

        public void Handle(IReceivedMessage<TestMessage> message) {
            logger.Info("TestMessageSubscriber Message received: {0} {1}", message.Message.Value, message.Message.MessageId);
            //throw new Exception("I hate this message");
        }
    }

    [MessageHandlerConfiguration(
        Singleton = true)]
    public class TestMessageSubscriberNumber2 : IHandleMessages<TestMessage> {

        static Logger logger = LogManager.GetCurrentClassLogger();

        public void Handle(IReceivedMessage<TestMessage> message) {
            logger.Info("TestMessageSubscriberNumber2 Message received: {0} {1}", message.Message.Value, message.Message.MessageId);
        }
    }
}
