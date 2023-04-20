using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus.Messaging;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Interfaces;
using System.IO;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Mocks {

    class MockMessagingFactory : IMessagingFactory {

        IMockServiceBus serviceBus;

        public MockMessagingFactory(IBus serviceBus) {
            Guard.ArgumentNotNull(serviceBus, "serviceBus");
            this.serviceBus = serviceBus as IMockServiceBus;
            Guard.ArgumentNotNull(this.serviceBus, "serviceBus");
        }

        public ISubscriptionClient CreateSubscriptionClient(string topicPath, string name, ReceiveMode receiveMode) {
            return serviceBus.CreateSubscriptionClient(topicPath, name, receiveMode);
        }

        public ITopicClient CreateTopicClient(string path) {
            return serviceBus.CreateTopicClient(path);
        }

        public void Close() {

        }

        public IBrokeredMessage CreateBrokeredMessage(Stream messageBodyStream) {
            var retVal = new MockBrokeredMessage(serviceBus);
            retVal.SetBody(messageBodyStream);
            return retVal;
        }
    }
}
