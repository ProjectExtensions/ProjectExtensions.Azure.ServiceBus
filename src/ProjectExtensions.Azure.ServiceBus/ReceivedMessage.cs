using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;

namespace ProjectExtensions.Azure.ServiceBus {

    public class ReceivedMessage<T> : IReceivedMessage<T> {

        BrokeredMessageWrapper brokeredMessage;
        T message;

        public ReceivedMessage(BrokeredMessage brokeredMessage, T message) {
            if (brokeredMessage == null) {
                throw new ArgumentNullException("brokeredMessage");
            }
            if (message == null) {
                throw new ArgumentNullException("message");
            }
            this.brokeredMessage = new BrokeredMessageWrapper(brokeredMessage);
            this.message = message;
        }

        public IBrokeredMessage BrokeredMessage {
            get {
                return brokeredMessage;
            }
        }

        public T Message {
            get {
                return this.message;
            }
        }
    }
}
