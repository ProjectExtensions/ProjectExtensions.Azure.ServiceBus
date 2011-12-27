using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Interfaces;
using Microsoft.Practices.TransientFaultHandling;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Mocks {

    class MockTopicClient : ITopicClient {

        IMockServiceBus serviceBus;

        public MockTopicClient(IMockServiceBus serviceBus) {
            Guard.ArgumentNotNull(serviceBus, "serviceBus");
            this.serviceBus = serviceBus;
        }

        public IAsyncResult BeginSend(IBrokeredMessage message, AsyncCallback callback, object state) {
            var retVal = new MockIAsyncResult() {
                AsyncState = state
            };
            callback(retVal);
            return retVal;
        }

        public void EndSend(IAsyncResult result) {
            //TODO call the mock service bus
        }

        public void Close() {
            //do nothing
        }
    }
}
