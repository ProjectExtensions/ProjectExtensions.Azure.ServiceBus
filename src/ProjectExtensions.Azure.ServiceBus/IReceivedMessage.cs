using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectExtensions.Azure.ServiceBus {
    
    public interface IReceivedMessage<T> {

        IBrokeredMessage BrokeredMessage {
            get;
        }

        T Message {
            get;
        }

        IDictionary<string, object> Metadata {
            get;
        }
    }
}
