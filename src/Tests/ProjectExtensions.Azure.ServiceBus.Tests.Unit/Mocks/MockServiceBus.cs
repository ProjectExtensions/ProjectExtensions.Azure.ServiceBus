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

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Mocks {

    class MockServiceBus : IMockServiceBus {

        IDictionary<string, TopicDescription> _topics = new Dictionary<string, TopicDescription>(StringComparer.OrdinalIgnoreCase);
        IDictionary<string, ITopicClient> _topicClients = new Dictionary<string, ITopicClient>(StringComparer.OrdinalIgnoreCase);
        List<SubscriptionDescriptionState> _subscriptions = new List<SubscriptionDescriptionState>();
        IDictionary<IAsyncResult, SubscriptionDescriptionState> _messages = new Dictionary<IAsyncResult, SubscriptionDescriptionState>();

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
                retVal = new MockTopicClient(this);
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
