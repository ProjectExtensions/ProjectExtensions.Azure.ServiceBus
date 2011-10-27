using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Basic message Handler
    /// </summary>
    /// <typeparam name="T">The type of message to be handled.</typeparam>
    /// <remarks>A new handler will be instantiated for each message unless you decorate your handler class with [SingletonMessageHandler]</remarks>
    public interface IMessageHandler<T> {
        /// <summary>
        /// Process a Message with the given signature.
        /// </summary>
        /// <param name="message">The message to handle.</param>
        /// <remarks>
        /// Every message received by the bus with this message type will call this method.
        /// </remarks>
        void Handle(IReceivedMessage<T> message);

    }

    /// <summary>
    /// Interface that needs to be implemented if you wish to register a subscription.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>A new handler will be instantiated for each message unless you decorate your handler class with [SingletonMessageHandler]</remarks>
    public interface IHandleMessages<T> : IMessageHandler<T> {

    }

    /// <summary>
    /// Interface that needs to be implemented if you wish to register a competing subscription.
    /// </summary>
    /// <remarks>This implementation will allow the Topic to scale out, allowing multiple subscibers to handle the messages sent.</remarks>
    /// <typeparam name="T"></typeparam>
    /// <remarks>A new handler will be instantiated for each message unless you decorate your handler class with [SingletonMessageHandler]</remarks>
    public interface IHandleCompetingMessages<T> : IMessageHandler<T> {

    }
}
