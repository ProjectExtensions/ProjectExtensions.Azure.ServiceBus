using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using Microsoft.ServiceBus.Messaging;

namespace ProjectExtensions.Azure.ServiceBus.Wrappers {
    
    public class SubscriptionClientWrapper : ISubscriptionClient {

        SubscriptionClient client;

        public SubscriptionClientWrapper(SubscriptionClient client) {
            this.client = client;
        }

        public IAsyncResult BeginReceive(TimeSpan serverWaitTime, AsyncCallback callback, object state) {
            return client.BeginReceive(serverWaitTime, callback, state);
        }

        public BrokeredMessage EndReceive(IAsyncResult result) {
            return client.EndReceive(result);
        }

        public void Close() {
            client.Close();
        }

        public ReceiveMode Mode {
            get {
                return client.Mode;
            }
        }
    }
}
