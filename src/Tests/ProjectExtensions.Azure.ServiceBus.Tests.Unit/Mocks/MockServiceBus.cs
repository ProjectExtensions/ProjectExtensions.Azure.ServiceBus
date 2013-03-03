using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Interfaces;
using Microsoft.ServiceBus.Messaging;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using Microsoft.Practices.TransientFaultHandling;
using System.Threading;
using System.Threading.Tasks;
using ProjectExtensions.Azure.ServiceBus.Helpers;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Mocks {

    class MockServiceBus : IMockServiceBus {

        IBusConfiguration config;
        IAzureBusSender sender;
        IDictionary<string, TopicDescription> _topics = new Dictionary<string, TopicDescription>(StringComparer.OrdinalIgnoreCase);
        IDictionary<string, ITopicClient> _topicClients = new Dictionary<string, ITopicClient>(StringComparer.OrdinalIgnoreCase);
        List<SubscriptionDescriptionState> _subscriptions = new List<SubscriptionDescriptionState>();
        IDictionary<IAsyncResult, SubscriptionDescriptionState> _messages = new Dictionary<IAsyncResult, SubscriptionDescriptionState>();

        public MockServiceBus(IBusConfiguration config) {
            Guard.ArgumentNotNull(config, "config");
            this.config = config;
            Configure();
        }

        public SubscriptionDescription CreateSubscription(SubscriptionDescription description, Filter filter) {
            SqlFilter sf = filter as SqlFilter;
            var value = sf.SqlExpression;
            //we must parse on the first ' and then go to the end of the string  -1 char
            var index = value.IndexOf('\'');
            var typeName = value.Substring(index + 1);
            typeName = typeName.Substring(0, typeName.Length - 1).Replace("_", ".");

            //NOTE Limit is test class must exist in this assembly for now.
            var theType = this.GetType().Assembly.GetType(typeName);

            _subscriptions.Add(new SubscriptionDescriptionState() {
                Description = description,
                Type = theType
            });

            return description;
        }

        public ISubscriptionClient CreateSubscriptionClient(string topicPath, string name, ReceiveMode receiveMode) {
            var retVal = _subscriptions.FirstOrDefault(item => item.Description.TopicPath.Equals(topicPath) && item.Description.Name.Equals(name));
            if (retVal != null) {
                retVal.Client = new MockSubscriptionClient(this, topicPath, name, receiveMode);
                return retVal.Client;
            }
            throw new MessagingEntityNotFoundException(name);
        }

        public ITopicClient CreateTopicClient(string path) {
            ITopicClient retVal = null;
            if (!_topicClients.TryGetValue(path, out retVal)) {
                retVal = new MockTopicClient(this, path);
                _topicClients[path] = retVal;
            }
            return retVal;
        }

        public TopicDescription CreateTopic(TopicDescription description) {
            TopicDescription tempValue;
            if (!_topics.TryGetValue(description.Path, out tempValue)) {
                _topics[description.Path] = description;
            }
            else {
                throw new ApplicationException("Duplicate topic not allowed.");
            }
            return description;
        }

        public void DeleteSubscription(string topicPath, string name) {
            throw new NotImplementedException();
        }

        public SubscriptionDescription GetSubscription(string topicPath, string name) {
            var retVal = _subscriptions.FirstOrDefault(item => item.Description.TopicPath.Equals(topicPath) && item.Description.Name.Equals(name));
            if (retVal != null) {
                return retVal.Description;
            }
            throw new MessagingEntityNotFoundException(name);
        }

        public TopicDescription GetTopic(string path) {
            TopicDescription retVal = null;
            if (!_topics.TryGetValue(path, out retVal)) {
                throw new MessagingEntityNotFoundException(path);
            }
            return retVal;
        }

        public void MessageAbandon(IBrokeredMessage message) {
            //do nothing for now
        }

        public void MessageComplete(IBrokeredMessage message) {
            foreach (var subscription in _subscriptions) {
                if (subscription.Messages.Contains(message)) {
                    subscription.Messages.Remove(message);
                    break;
                }
            }
        }

        public void MessageDeadLetter(IBrokeredMessage message, string deadLetterReason, string deadLetterErrorDescription) {
            foreach (var subscription in _subscriptions) {
                if (subscription.Messages.Contains(message)) {
                    subscription.Messages.Remove(message);
                    break;
                }
            }
        }

        /// <summary>
        /// Send a message onto the service bus to any item that is registered for the type
        /// </summary>
        /// <param name="message"></param>
        public void SendMessage(IBrokeredMessage message) {
            var typeName = message.Properties[AzureSenderReceiverBase.TYPE_HEADER_NAME] as string;
            if (typeName == null) {
                throw new ApplicationException(AzureSenderReceiverBase.TYPE_HEADER_NAME + " Key does not exist.");
            }
            typeName = typeName.Replace("_", ".");

            //NOTE Limit is test class must exist in this assembly for now.
            var theType = this.GetType().Assembly.GetType(typeName);

            var handlers = _subscriptions.Where(item => item.Type == theType).ToList();

            foreach (var handler in handlers) {
                handler.AddMessage(new MockBrokeredMessage(this, message));
            }
        }

        public bool SubscriptionExists(string topicPath, string name) {
            return _subscriptions.Any(item => item.Description.TopicPath.Equals(topicPath) && item.Description.Name.Equals(name));
        }

        public IAsyncResult BeginReceive(ISubscriptionClient client, TimeSpan serverWaitTime, AsyncCallback callback, object state) {
            var retVal = new MockIAsyncResult() {
                AsyncState = new KeyValuePair<TimeSpan, object>(serverWaitTime, state)
            };

            var subClient = _subscriptions.FirstOrDefault(item => item.Client == client);

            _messages[retVal] = subClient;
            callback(retVal);
            return retVal;
        }

        public IBrokeredMessage EndReceive(IAsyncResult result) {

            IBrokeredMessage retVal = null;

            SubscriptionDescriptionState message = null;
            if (!_messages.TryGetValue(result, out message)) {
                throw new ApplicationException("You must call EndSend with a valid IAsyncResult. Duplicate Calls are not allowed.");
            }

            KeyValuePair<TimeSpan, object> state = (KeyValuePair<TimeSpan, object>)result.AsyncState;

            var t = new Task(() => {
                while (true) {
                    //TODO this could get caught in a loop, we need to check for delivered.
                    if (message.Messages.Count > 0) {
                        retVal = message.Messages.First();
                        break;
                    }
                    Thread.Sleep(1000);
                }

                (result.AsyncWaitHandle as ManualResetEvent).Set();
            });

            t.Start();

            bool completed = result.AsyncWaitHandle.WaitOne(state.Key);
            result.AsyncWaitHandle.Dispose();

            return retVal;
        }

        public void RegisterAssembly(System.Reflection.Assembly assembly) {
            Guard.ArgumentNotNull(assembly, "assembly");
            //logger.Info("RegisterAssembly={0}", assembly.FullName);

            foreach (var type in assembly.GetTypes()) {
                var interfaces = type.GetInterfaces()
                                .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IHandleMessages<>) || i.GetGenericTypeDefinition() == typeof(IHandleCompetingMessages<>)))
                                .ToList();
                if (interfaces.Count > 0) {
                    Subscribe(type);
                }
            }
        }

        public void Publish<T>(T message) {
            if (sender == null) {
                sender = config.Container.Resolve<IAzureBusSender>();
            }
            sender.Send<T>(message);
        }

        public void Publish<T>(T message, IDictionary<string, object> metadata = null) {
            if (sender == null) {
                sender = config.Container.Resolve<IAzureBusSender>();
            }
            sender.Send<T>(message, metadata);
        }

        public void PublishAsync<T>(T message, Action<IMessageSentResult<T>> resultCallBack, IDictionary<string, object> metadata = null) {
            if (sender == null) {
                sender = config.Container.Resolve<IAzureBusSender>();
            }
            sender.SendAsync<T>(message, null, resultCallBack, metadata);
        }

        public void PublishAsync<T>(T message, object state, Action<IMessageSentResult<T>> resultCallBack, IDictionary<string, object> metadata = null) {
            if (sender == null) {
                sender = config.Container.Resolve<IAzureBusSender>();
            }
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

            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && (i.GetGenericTypeDefinition() == typeof(IHandleMessages<>) || i.GetGenericTypeDefinition() == typeof(IHandleCompetingMessages<>)))
                .ToList();

            //for each interface we find, we need to register it with the bus.
            foreach (var foundInterface in interfaces) {

                var implementedMessageType = foundInterface.GetGenericArguments()[0];
                //due to the limits of 50 chars we will take the name and a MD5 for the name.
                var hashName = implementedMessageType.FullName + "|" + type.FullName;

                var hash = MD5Helper.CalculateMD5(hashName);
                var fullName = (IsCompetingHandler(foundInterface) ? "C_" : config.ServiceBusApplicationId + "_") + hash;

                var info = new ServiceBusEnpointData() {
                    AttributeData = type.GetCustomAttributes(typeof(MessageHandlerConfigurationAttribute), false).FirstOrDefault() as MessageHandlerConfigurationAttribute,
                    DeclaredType = type,
                    MessageType = implementedMessageType,
                    SubscriptionName = fullName,
                    ServiceType = foundInterface
                };

                if (!config.Container.IsRegistered(type)) {
                    if (info.IsReusable) {
                        config.Container.Register(type, type);
                    }
                    else {
                        config.Container.Register(type, type, true);
                    }
                }

                //TODO verify if this is the correct subscription type
                var filter = new SqlFilter(string.Format(AzureSenderReceiverBase.TYPE_HEADER_NAME + " = '{0}'", implementedMessageType.FullName.Replace('.', '_')));

                CreateSubscription(new SubscriptionDescription(config.TopicName, fullName), filter);
            }

            config.Container.Build();
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
            //logger.Info("Unsubscribe={0}", type.FullName);

            var subscription = _subscriptions.FirstOrDefault(item => item.Type == type);

            if (subscription != null) {
                _subscriptions.Remove(subscription);
            }
        }

        internal static bool IsCompetingHandler(Type type) {
            return type.GetGenericTypeDefinition() == typeof(IHandleCompetingMessages<>);
        }

        private void Configure() {
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

    }

    class SubscriptionDescriptionState {

        public SubscriptionDescriptionState() {
            Messages = new List<IBrokeredMessage>();
        }

        public List<IBrokeredMessage> Messages {
            get;
            private set;
        }

        public SubscriptionDescription Description {
            get;
            set;
        }

        public Type Type {
            get;
            set;
        }

        public ISubscriptionClient Client {
            get;
            set;
        }

        public void AddMessage(IBrokeredMessage message) {
            Messages.Add(message);
        }

        public void RemoveMessage(IBrokeredMessage message) {
            Messages.Remove(message);
        }
    }
}
