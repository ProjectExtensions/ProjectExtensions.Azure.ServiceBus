using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectExtensions.Azure.ServiceBus {
    
    /// <summary>
    /// Main interface that is passed to the Handlers.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReceivedMessage<T> {

        /// <summary>
        /// A wrapped instance of the BrokeredMessage received from the Service Bus
        /// </summary>
        IBrokeredMessage BrokeredMessage {
            get;
        }

        /// <summary>
        /// The message that was sent.
        /// </summary>
        T Message {
            get;
        }

        /// <summary>
        /// The metadata passed with the message from the user.
        /// </summary>
        IDictionary<string, object> Metadata {
            get;
        }
    }
}
