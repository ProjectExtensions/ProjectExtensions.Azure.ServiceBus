using Microsoft.Practices.TransientFaultHandling;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Interfaces;
using System;
using System.Collections.Generic;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Mocks {

    class MockTopicClient : ITopicClient {

        IDictionary<IAsyncResult, IBrokeredMessage> _messages = new Dictionary<IAsyncResult, IBrokeredMessage>();

        IMockServiceBus serviceBus;

        public string Path {
            get;
            private set;
        }

        public MockTopicClient(IBus serviceBus, string path) {
            Guard.ArgumentNotNull(serviceBus, "serviceBus");
            this.serviceBus = serviceBus as IMockServiceBus;
            Guard.ArgumentNotNull(this.serviceBus, "serviceBus");
            this.Path = path;
        }

        public void Send(IBrokeredMessage message) {
            var retVal = new MockIAsyncResult() {
                AsyncState = null
            };
            _messages[retVal] = message;
        }

        public void Close() {
            //do nothing
        }
    }
}
