using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;

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
