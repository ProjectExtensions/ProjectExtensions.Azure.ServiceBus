using Microsoft.Practices.TransientFaultHandling;
using NLog;
using ProjectExtensions.Azure.ServiceBus.Helpers;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Implementation of IBus
    /// </summary>
    class AzureBus : IBus {

        static Logger logger = LogManager.GetCurrentClassLogger();

        IBusConfiguration config;
        IAzureBusSender sender;
        IAzureBusReceiver receiver;

        List<Type> subscribedTypes = new List<Type>();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="config"></param>
        /// <param name="sender"></param>
        /// <param name="receiver"></param>
        public AzureBus(IBusConfiguration config, IAzureBusSender sender, IAzureBusReceiver receiver) {
            Guard.ArgumentNotNull(config, "config");
            Guard.ArgumentNotNull(sender, "sender");
            Guard.ArgumentNotNull(receiver, "receiver");
            this.config = config;
            this.sender = sender;
            this.receiver = receiver;
        }

        public void Initialize() {
            Configure();
        }

        /// <summary>
        /// Get the Number of messages for the topic for a given type
        /// </summary>
        /// <param name="type">The end point message</param>
        /// <returns></returns>
        public long MessageCountForType(Type type) {
            return receiver.MessageCountForType(type);
        }

        /// <summary>
        /// Auto discover all of the Subscribers in the assembly.
        /// </summary>
        /// <param name="assembly">The assembly to register</param>
        public void RegisterAssembly(Assembly assembly) {
            Guard.ArgumentNotNull(assembly, "assembly");
            logger.Info("RegisterAssembly={0}", assembly.FullName);

            foreach (var type in assembly.GetTypes()) {
                var interfaces = type.GetInterfaces()
                                .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IHandleMessages<>) || i.GetGenericTypeDefinition() == typeof(IHandleCompetingMessages<>)))
                                .ToList();
                if (interfaces.Count > 0) {
                    Subscribe(type);
                }
            }
        }

        /// <summary>
        /// Publish a Message with the given signature.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to publish.</param>
        public void Publish<T>(T message) {
            sender.Send<T>(message, default);
        }

        /// <summary>
        /// Publish a Message with the given signature.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to publish.</param>
        /// <param name="metadata">Metadata to sent with the message.</param>
        public void Publish<T>(T message, IDictionary<string, object> metadata = null) {
            Guard.ArgumentNotNull(message, "message");
            logger.Info("Publish={0}", message.GetType().FullName);
            sender.Send<T>(message, metadata);
        }

        /// <summary>
        /// Publish a Message with the given signature.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to publish.</param>
        /// <param name="resultCallBack">The callback when the operation completes</param>
        /// <param name="metadata">Metadata to sent with the message.</param>
        public void PublishAsync<T>(T message, Action<IMessageSentResult<T>> resultCallBack, IDictionary<string, object> metadata) {
            sender.SendAsync<T>(message, null, resultCallBack, metadata);
        }

        /// <summary>
        /// Publish a Message with the given signature.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message">The message to publish.</param>
        /// <param name="state">State object that is returned to the user</param>
        /// <param name="resultCallBack">The callback when the operation completes</param>
        /// <param name="metadata">Metadata to sent with the message.</param>
        public void PublishAsync<T>(T message, object state, Action<IMessageSentResult<T>> resultCallBack, IDictionary<string, object> metadata) {
            Guard.ArgumentNotNull(message, "message");
            Guard.ArgumentNotNull(resultCallBack, "resultCallBack");
            logger.Info("PublishAsync={0}", message.GetType().FullName);
            sender.SendAsync<T>(message, state, resultCallBack, metadata);
        }

        /// <summary>
        /// Subscribes to recieve published messages of type T.
        /// This method is only necessary if you turned off auto-subscribe
        /// </summary>
        /// <typeparam name="T">The type of message to subscribe to.</typeparam>
        public void Subscribe<T>() {
            Subscribe(typeof(T));
        }

        /// <summary>
        /// Subscribes to recieve published messages of type T.
        /// This method is only necessary if you turned off auto-subscribe
        /// </summary>
        /// <param name="type">The type to subscribe</param>
        public void Subscribe(Type type) {
            Guard.ArgumentNotNull(type, "type");
            logger.Info("Subscribe={0}", type.FullName);
            subscribedTypes.Add(type);
            BusHelper.SubscribeOrUnsubscribeType((s) => { logger.Info(s); }, type, config, receiver.CreateSubscription);
        }

        /// <summary>
        /// Unsubscribes from receiving published messages of the specified type
        /// </summary>
        /// <typeparam name="T">The type of message to unsubscribe from</typeparam>
        public void Unsubscribe<T>() {
            Unsubscribe(typeof(T));
        }

        /// <summary>
        /// Unsubscribes from receiving published messages of the specified type.
        /// </summary>
        /// <param name="type">The type of message to unsubscribe from</param>
        public void Unsubscribe(Type type) {
            Guard.ArgumentNotNull(type, "type");
            logger.Info("Unsubscribe={0}", type.FullName);

            if (subscribedTypes.Contains(type)) {
                subscribedTypes.Remove(type);
            }
            BusHelper.SubscribeOrUnsubscribeType((s) => { logger.Info(s); }, type, config, receiver.CancelSubscription);
        }

        void Configure() {
            //this fixes a bug in .net 4 that will be fixed in sp1
            using (CloudEnvironment.EnsureSafeHttpContext()) {

                foreach (var item in config.RegisteredAssemblies) {
                    RegisterAssembly(item);
                }
                foreach (var item in config.RegisteredSubscribers) {
                    Subscribe(item);
                }
            }
        }

        void RegisterAssembly(IEnumerable<Assembly> assemblies) {
            Guard.ArgumentNotNull(assemblies, "assemblies");
            foreach (var item in assemblies) {
                RegisterAssembly(item);
            }
        }
    }
}
