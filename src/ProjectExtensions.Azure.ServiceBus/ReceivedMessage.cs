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
        IDictionary<string, object> metadata;

        public ReceivedMessage(BrokeredMessage brokeredMessage, T message, IDictionary<string, object> metadata) {
            Guard.ArgumentNotNull(brokeredMessage, "brokeredMessage");
            Guard.ArgumentNotNull(message, "message");
            Guard.ArgumentNotNull(metadata, "metadata");
            this.brokeredMessage = new BrokeredMessageWrapper(brokeredMessage);
            this.message = message;
            this.metadata = metadata;
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

        public IDictionary<string, object> Metadata {
            get {
                return metadata;
            }
        }
    }
}
