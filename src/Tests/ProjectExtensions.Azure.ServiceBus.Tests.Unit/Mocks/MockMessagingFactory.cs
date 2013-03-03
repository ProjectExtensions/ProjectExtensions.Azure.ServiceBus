using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using Microsoft.ServiceBus.Messaging;
using System.IO;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Interfaces;
using Microsoft.Practices.TransientFaultHandling;

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
