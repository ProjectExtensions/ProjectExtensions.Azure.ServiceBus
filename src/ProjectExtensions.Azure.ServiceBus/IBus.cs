using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

//DO NOT Change the namespace.
namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Service Bus interface
    /// </summary>
    public interface IBus {

        /// <summary>
        /// Called After we prime the bus so that any config needed can be initialized.
        /// </summary>
        void Initialize();

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
        void Publish<T>(T message);

        /// <summary>
        /// Publish a Message with the given signature.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to publish.</param>
        /// <param name="metadata">Metadata to sent with the message.</param>
        void Publish<T>(T message, IDictionary<string, object> metadata = null);

        /// <summary>
        /// Publish a Message with the given signature.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to publish.</param>
        /// <param name="resultCallBack">The callback when the message is complete</param>
        /// <param name="metadata">Metadata to sent with the message.</param>
        void PublishAsync<T>(T message, Action<IMessageSentResult<T>> resultCallBack, IDictionary<string, object> metadata = null);

        /// <summary>
        /// Publish a Message with the given signature.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to publish.</param>
        /// <param name="state">State object that is returned to the user</param>
        /// <param name="resultCallBack">The callback when the message is complete</param>
        /// <param name="metadata">Metadata to sent with the message.</param>
        void PublishAsync<T>(T message, object state, Action<IMessageSentResult<T>> resultCallBack, IDictionary<string, object> metadata = null);

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
