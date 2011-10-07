using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Autofac;
using NLog;
using System.Linq.Expressions;
using Microsoft.AzureCAT.Samples.TransientFaultHandling;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Implementation of IBus
    /// </summary>
    public class AzureBus : IBus {

        static Logger logger = LogManager.GetCurrentClassLogger();

        BusConfiguration config;
        IAzureBusSender sender;
        IAzureBusReceiver receiver;

        List<Type> subscribedTypes = new List<Type>();

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="config"></param>
        public AzureBus(BusConfiguration config) {
            Guard.ArgumentNotNull(config, "config");
            this.config = config;
            Configure();
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
        /// <param name="metadata">Metadata to sent with the message.</param>
        public void Publish<T>(T message) {
            sender.Send<T>(message, default(IDictionary<string, object>));
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
        /// <typeparam name="T">The type of message to subscribe to.</typeparam>
        /// <param name="type">The type to subscribe</param>
        public void Subscribe(Type type) {
            Guard.ArgumentNotNull(type, "type");
            logger.Info("Subscribe={0}", type.FullName);
            SubscribeOrUnsubscribeType(type, receiver.CreateSubscription);
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
            SubscribeOrUnsubscribeType(type, receiver.CancelSubscription);
        }

        void Configure() {
            //set up the server first.
            sender = BusConfiguration.Container.Resolve<IAzureBusSender>(new TypedParameter(typeof(BusConfiguration), config));
            receiver = BusConfiguration.Container.Resolve<IAzureBusReceiver>(new TypedParameter(typeof(BusConfiguration), config));

            foreach (var item in config.RegisteredAssemblies) {
                RegisterAssembly(item);
            }
            foreach (var item in config.RegisteredSubscribers) {
                Subscribe(item);
            }
        }

        bool IsCompetingHandler(Type type) {
            return type.GetGenericTypeDefinition() == typeof(IHandleCompetingMessages<>);
        }

        void RegisterAssembly(IEnumerable<Assembly> assemblies) {
            Guard.ArgumentNotNull(assemblies, "assemblies");
            foreach (var item in assemblies) {
                RegisterAssembly(item);
            }
        }

        void SubscribeOrUnsubscribeType(Type type, Action<ServiceBusEnpointData> callback) {
            Guard.ArgumentNotNull(type, "type");
            Guard.ArgumentNotNull(callback, "callback");

            logger.Info("SubscribeOrUnsubscribeType={0}", type.FullName);
            var interfaces = type.GetInterfaces()
                            .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IHandleMessages<>) || i.GetGenericTypeDefinition() == typeof(IHandleCompetingMessages<>)))
                            .ToList();

            if (interfaces.Count == 0) {
                throw new ApplicationException(string.Format("Type {0} does not implement IHandleMessages or IHandleCompetingMessages", type.FullName));
            }

            subscribedTypes.Add(type);

            var builder = new ContainerBuilder();

            //for each interface we find, we need to register it with the bus.
            foreach (var foundInterface in interfaces) {

                var implementedMessageType = foundInterface.GetGenericArguments()[0];
                //due to the limits of 50 chars we will take the name and a MD5 for the name.
                var hashName = implementedMessageType.FullName + "|" + type.FullName;

                var hash = Helpers.CalculateMD5(hashName);
                var fullName = (IsCompetingHandler(foundInterface) ? "C_" : config.ServiceBusApplicationId + "_") + hash;

                var info = new ServiceBusEnpointData() {
                    DeclaredType = type,
                    MessageType = implementedMessageType,
                    SubscriptionName = fullName,
                    ServiceType = foundInterface,
                    IsReusable = (type.GetCustomAttributes(typeof(SingletonMessageHandlerAttribute), false).Count() > 0)
                };


                if (!BusConfiguration.Container.IsRegistered(type)) {
                    if (info.IsReusable) {
                        builder.RegisterType(type).SingleInstance();
                    }
                    else {
                        builder.RegisterType(type).InstancePerDependency();
                    }
                }


                callback(info);
            }

            builder.Update(BusConfiguration.Container);
        }
    }
}
