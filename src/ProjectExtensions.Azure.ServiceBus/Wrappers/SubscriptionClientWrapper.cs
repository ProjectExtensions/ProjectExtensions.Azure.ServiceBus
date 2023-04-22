using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus.Messaging;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using System;

namespace ProjectExtensions.Azure.ServiceBus.Wrappers {

    class SubscriptionClientWrapper : ISubscriptionClient {

        SubscriptionClient client;

        public SubscriptionClientWrapper(SubscriptionClient client) {
            Guard.ArgumentNotNull(client, "client");
            this.client = client;
        }

        public bool IsClosed {
            get {
                return client.IsClosed;
            }
        }

        public ReceiveMode Mode {
            get {
                return client.Mode;
            }
        }

        public int PrefetchCount {
            get {
                return client.PrefetchCount;
            }
            set {
                client.PrefetchCount = value;
            }
        }

        public IBrokeredMessage BeginReceive(TimeSpan serverWaitTime) {
            return new BrokeredMessageWrapper(client.Receive(serverWaitTime));
        }

        public void OnMessage(Action<IBrokeredMessage> callback, OnMessageOptions onMessageOptions) {
            client.OnMessage((msg) => {
                callback(new BrokeredMessageWrapper(msg));
            }, onMessageOptions);
        }

        public void Close() {
            client.Close();
        }

    }
}
