using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectExtensions.Azure.ServiceBus {

    class MessageSentResult<T> : IMessageSentResult<T> {

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

        public object State {
            get;
            set;
        }

    }
}
