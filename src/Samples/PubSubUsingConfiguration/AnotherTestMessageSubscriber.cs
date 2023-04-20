
/* Unmerged change from project 'PubSubUsingCastleWindsor'
Before:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
After:
using Microsoft.ServiceBus.Messaging;
using NLog;
using ProjectExtensions.Azure.ServiceBus;
using System;
*/

/* Unmerged change from project 'PubSubUsingStructureMap'
Before:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
After:
using Microsoft.ServiceBus.Messaging;
using NLog;
using ProjectExtensions.Azure.ServiceBus;
using System;
*/

/* Unmerged change from project 'PubSubUsingUnity'
Before:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
After:
using Microsoft.ServiceBus.Messaging;
using NLog;
using ProjectExtensions.Azure.ServiceBus;
using System;
*/

/* Unmerged change from project 'PubSubUsingNinject'
Before:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
After:
using Microsoft.ServiceBus.Messaging;
using NLog;
using ProjectExtensions.Azure.ServiceBus;
using System;
*/
using Microsoft.ServiceBus.Messaging;
using NLog;
using ProjectExtensions.Azure.ServiceBus;
/* Unmerged change from project 'PubSubUsingCastleWindsor'
Before:
using System.Collections.Generic;

/* Unmerged change from project 'PubSubUsingCastleWindsor'
After:
/* Unmerged change from project 'PubSubUsingCastleWindsor'
*/

/* Unmerged change from project 'PubSubUsingStructureMap'
Before:
using System.Collections.Generic;

/* Unmerged change from project 'PubSubUsingCastleWindsor'
After:
/* Unmerged change from project 'PubSubUsingCastleWindsor'
*/

/* Unmerged change from project 'PubSubUsingUnity'
Before:
using System.Collections.Generic;

/* Unmerged change from project 'PubSubUsingCastleWindsor'
After:
/* Unmerged change from project 'PubSubUsingCastleWindsor'
*/

/* Unmerged change from project 'PubSubUsingNinject'
Before:
using System.Collections.Generic;

/* Unmerged change from project 'PubSubUsingCastleWindsor'
After:
/* Unmerged change from project 'PubSubUsingCastleWindsor'
*/


/* Unmerged change from project 'PubSubUsingCastleWindsor'
Before:
using NLog;
After:
using System.Diagnostics;
*/

/* Unmerged change from project 'PubSubUsingStructureMap'
Before:
using NLog;
After:
using System.Diagnostics;
*/

/* Unmerged change from project 'PubSubUsingUnity'
Before:
using NLog;
After:
using System.Diagnostics;
*/

/* Unmerged change from project 'PubSubUsingNinject'
Before:
using NLog;
After:
using System.Diagnostics;
*/
using System
/* Unmerged change from project 'PubSubUsingCastleWindsor'
Before:
using System.Linq;
After:
using System.Collections.Generic;
using System.Linq;
*/

/* Unmerged change from project 'PubSubUsingStructureMap'
Before:
using System.Linq;
After:
using System.Collections.Generic;
using System.Linq;
*/

/* Unmerged change from project 'PubSubUsingUnity'
Before:
using System.Linq;
After:
using System.Collections.Generic;
using System.Linq;
*/

/* Unmerged change from project 'PubSubUsingNinject'
Before:
using System.Linq;
After:
using System.Collections.Generic;
using System.Linq;
*/
;
using System.Diagnostics;
/* Unmerged change from project 'PubSubUsingCastleWindsor'
Before:
using Microsoft.ServiceBus.Messaging;
After:
using System.Text;
*/

/* Unmerged change from project 'PubSubUsingStructureMap'
Before:
using Microsoft.ServiceBus.Messaging;
After:
using System.Text;
*/

/* Unmerged change from project 'PubSubUsingUnity'
Before:
using Microsoft.ServiceBus.Messaging;
After:
using System.Text;
*/

/* Unmerged change from project 'PubSubUsingNinject'
Before:
using Microsoft.ServiceBus.Messaging;
After:
using System.Text;
*/


namespace PubSubUsingConfiguration {

    [MessageHandlerConfiguration(
        DefaultMessageTimeToLive = 240,
        LockDuration = 120,
        MaxConcurrentCalls = 4,
        MaxRetries = 2,
        PrefetchCount = 20,
        PauseTimeIfErrorWasThrown = 20000,
        ReceiveMode = ReceiveMode.PeekLock,
        Singleton = true)]
    public class AnotherTestMessageSubscriber : IHandleCompetingMessages<AnotherTestMessage> {

        static Logger logger = LogManager.GetCurrentClassLogger();

        static bool timerSet = false;

        static Stopwatch sw = new Stopwatch();

        public void Handle(IReceivedMessage<AnotherTestMessage> message) {
            if (!timerSet) {
                sw.Start();
                timerSet = true;
            }

            logger.Info("AnotherTestMessageSubscriber Message received: {0} {1} {2}", message.Message.Value, message.Message.MessageId, sw.ElapsedMilliseconds);
            throw new Exception("Bad Day");
        }
    }
}
