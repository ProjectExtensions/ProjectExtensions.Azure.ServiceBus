using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Interfaces;
using Microsoft.Practices.TransientFaultHandling;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Mocks {

    class MockTopicClient : ITopicClient {

        IDictionary<IAsyncResult, IBrokeredMessage> _messages = new Dictionary<IAsyncResult, IBrokeredMessage>();

        IMockServiceBus serviceBus;

        public MockTopicClient(IMockServiceBus serviceBus) {
            Guard.ArgumentNotNull(serviceBus, "serviceBus");
            this.serviceBus = serviceBus;
        }

        public IAsyncResult BeginSend(IBrokeredMessage message, AsyncCallback callback, object state) {
            var retVal = new MockIAsyncResult() {
                AsyncState = state
            };
            _messages[retVal] = message;
            callback(retVal);
            return retVal;
        }

        public void EndSend(IAsyncResult result) {
            IBrokeredMessage message = null;
            if (!_messages.TryGetValue(result, out message)) {
                throw new ApplicationException("You must call EndSend with a valid IAsyncResult. Duplicate Calls are not allowed.");
            }
            serviceBus.SendMessage(message);
        }

        public void Close() {
            //do nothing
        }
    }
}
