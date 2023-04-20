using Microsoft.ServiceBus.Messaging;
using System;

namespace ProjectExtensions.Azure.ServiceBus.Interfaces {

    interface ISubscriptionClient {

        int PrefetchCount {
            get;
            set;
        }

        void OnMessage(Action<IBrokeredMessage> callback, OnMessageOptions onMessageOptions);

        bool IsClosed { get; }

        void Close();

        ReceiveMode Mode {
            get;
        }

    }
}
