using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Interfaces;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Practices.TransientFaultHandling;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Mocks {

    class MockSubscriptionClient : ISubscriptionClient {

        IMockServiceBus serviceBus;
        string topicPath;
        string name;
        ReceiveMode receiveMode;

        public MockSubscriptionClient(IBus serviceBus, string topicPath, string name, ReceiveMode receiveMode) {
            Guard.ArgumentNotNull(serviceBus, "serviceBus");
            Guard.ArgumentNotNull(topicPath, "topicPath");
            Guard.ArgumentNotNull(name, "name");
            this.serviceBus = serviceBus as IMockServiceBus;
            Guard.ArgumentNotNull(this.serviceBus, "serviceBus");
            this.topicPath = topicPath;
            this.name = name;
            this.receiveMode = receiveMode;
        }

        public int PrefetchCount {
            get;
            set;
        }

        public IAsyncResult BeginReceive(TimeSpan serverWaitTime, AsyncCallback callback, object state) {
            return serviceBus.BeginReceive(this, serverWaitTime, callback, state);
        }

        public IBrokeredMessage EndReceive(IAsyncResult result) {
            return serviceBus.EndReceive(result);
        }

        public void OnMessage(Action<IBrokeredMessage> callback, OnMessageOptions onMessageOptions) {
            throw new NotImplementedException();
        }

        public void Close() {

        }

        public ReceiveMode Mode {
            get {
                return receiveMode;
            }
        }
    }
}
