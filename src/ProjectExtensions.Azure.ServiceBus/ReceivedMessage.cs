using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Microsoft.AzureCAT.Samples.TransientFaultHandling;

namespace ProjectExtensions.Azure.ServiceBus {

    public class ReceivedMessage<T> : IReceivedMessage<T> {

        BrokeredMessageWrapper brokeredMessage;
        T message;

        public ReceivedMessage(BrokeredMessage brokeredMessage, T message) {
            Guard.ArgumentNotNull(brokeredMessage, "brokeredMessage");
            Guard.ArgumentNotNull(message, "message");
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
