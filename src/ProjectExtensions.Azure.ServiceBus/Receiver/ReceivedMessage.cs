using Microsoft.Practices.TransientFaultHandling;
using System.Collections.Generic;

namespace ProjectExtensions.Azure.ServiceBus.Receiver {

    class ReceivedMessage<T> : IReceivedMessage<T> {

        IBrokeredMessage brokeredMessage;
        T message;
        IDictionary<string, object> metadata;

        public ReceivedMessage(IBrokeredMessage brokeredMessage, T message, IDictionary<string, object> metadata) {
            Guard.ArgumentNotNull(brokeredMessage, "brokeredMessage");
            Guard.ArgumentNotNull(message, "message");
            Guard.ArgumentNotNull(metadata, "metadata");
            this.brokeredMessage = brokeredMessage;
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
