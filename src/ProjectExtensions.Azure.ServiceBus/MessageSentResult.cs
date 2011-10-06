using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectExtensions.Azure.ServiceBus {
    
    /// <summary>
    /// Result of a call to SendAsyc on the message bus
    /// </summary>
    public class MessageSentResult<T> : IMessageSentResult<T> {

        public bool IsSuccess {
            get;
            set;
        }

        public Exception ThrownException {
            get;
            set;
        }

        public TimeSpan TimeSpent {
            get;
            set;
        }

        public T Message {
            get;
            set;
        }

    }
}
