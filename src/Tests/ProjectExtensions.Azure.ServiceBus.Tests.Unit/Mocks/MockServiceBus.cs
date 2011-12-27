using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Interfaces;
using Microsoft.ServiceBus.Messaging;
using ProjectExtensions.Azure.ServiceBus.Interfaces;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Mocks {

    class MockServiceBus : IMockServiceBus {

        IDictionary<string, TopicDescription> _topics = new Dictionary<string, TopicDescription>(StringComparer.OrdinalIgnoreCase);
        IDictionary<string, ITopicClient> _topicClients = new Dictionary<string, ITopicClient>(StringComparer.OrdinalIgnoreCase);

        public SubscriptionDescription CreateSubscription(SubscriptionDescription description, Filter filter) {
            throw new NotImplementedException();
        }

        public ISubscriptionClient CreateSubscriptionClient(string topicPath, string name, ReceiveMode receiveMode) {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public void DeleteSubscription(string topicPath, string name) {
            throw new NotImplementedException();
        }

        public SubscriptionDescription GetSubscription(string topicPath, string name) {
            throw new NotImplementedException();
        }

        public TopicDescription GetTopic(string path) {
            TopicDescription retVal = null;
            if (!_topics.TryGetValue(path, out retVal)) {
                retVal = new TopicDescription(path);
                _topics[path] = retVal;
            }
            return retVal;
        }

        public void MessageAbandon() {
            throw new NotImplementedException();
        }

        public void MessageComplete() {
            throw new NotImplementedException();
        }

        public void MessageDeadLetter(string deadLetterReason, string deadLetterErrorDescription) {
            throw new NotImplementedException();
        }

        public bool SubscriptionExists(string topicPath, string name) {
            throw new NotImplementedException();
        }
    }
}
