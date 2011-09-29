using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Basic message Handler
    /// </summary>
    /// <typeparam name="T">The type of message to be handled.</typeparam>
    public interface IMessageHandler<T> {

        /// <summary>
        /// Is your handler thread safe.
        /// </summary>
        bool IsReusable {
            get;
        }

        /// <summary>
        /// Process a Message with the given signature.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        /// <param name="metadata">Metadata that was sent with the message.</param>
        /// <remarks>
        /// Every message received by the bus with this message type will call this method.
        /// </remarks>
        void Handle(IReceivedMessage<T> message, IDictionary<string, object> metadata);

    }

    /// <summary>
    /// Interface that needs to be implemented if you wish to register a subscription.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IHandleMessages<T> : IMessageHandler<T> {

    }

    /// <summary>
    /// Interface that needs to be implemented if you wish to register a competing subscription.
    /// </summary>
    /// <remarks>This implementation will allow the Topic to scale out, allowing multiple subscibers to handle the messages sent.</remarks>
    /// <typeparam name="T"></typeparam>
    public interface IHandleCompetingMessages<T> : IMessageHandler<T> {

    }
}
