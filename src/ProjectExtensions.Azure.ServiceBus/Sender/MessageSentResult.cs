using System;

namespace ProjectExtensions.Azure.ServiceBus.Sender {

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
