using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Service Bus interface
    /// </summary>
    public interface IBus {

        /// <summary>
        /// Auto discover all of the Subscribers in the assembly.
        /// </summary>
        /// <param name="assembly">The assembly to register</param>
        void RegisterAssembly(Assembly assembly);

        /// <summary>
        /// Publish a Message with the given signature.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to publish.</param>
        /// <param name="metadata">Metadata to sent with the message.</param>
        void Publish<T>(T message, IDictionary<string, object> metadata);

        /// <summary>
        /// Subscribes to recieve published messages of type T.
        /// This method is only necessary if you turned off auto-subscribe.
        /// </summary>
        /// <typeparam name="T">The type of message to subscribe to.</typeparam>
        void Subscribe<T>();

        /// <summary>
        /// Unsubscribes from receiving published messages of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of message to unsubscribe from.</typeparam>
        void Unsubscribe<T>();

    }

}
