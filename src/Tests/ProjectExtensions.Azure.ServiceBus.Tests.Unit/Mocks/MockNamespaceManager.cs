using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus.Messaging;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Interfaces;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Mocks {

    class MockNamespaceManager : INamespaceManager {

        IMockServiceBus serviceBus;

        public MockNamespaceManager(IBus serviceBus) {
            Guard.ArgumentNotNull(serviceBus, "serviceBus");
            this.serviceBus = serviceBus as IMockServiceBus;
            Guard.ArgumentNotNull(this.serviceBus, "serviceBus");
        }

        public SubscriptionDescription CreateSubscription(SubscriptionDescription description, Filter filter) {
            return serviceBus.CreateSubscription(description, filter);
        }

        public TopicDescription CreateTopic(TopicDescription description) {
            return serviceBus.CreateTopic(description);
        }

        public void DeleteSubscription(string topicPath, string name) {
            serviceBus.DeleteSubscription(topicPath, name);
        }

        public SubscriptionDescription GetSubscription(string topicPath, string name) {
            return serviceBus.GetSubscription(topicPath, name);
        }

        public TopicDescription GetTopic(string path) {
            return serviceBus.GetTopic(path);
        }

        public bool SubscriptionExists(string topicPath, string name) {
            return serviceBus.SubscriptionExists(topicPath, name);
        }
    }
}
