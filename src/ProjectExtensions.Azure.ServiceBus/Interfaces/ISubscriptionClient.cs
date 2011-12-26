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

        IAsyncResult BeginReceive(TimeSpan serverWaitTime, AsyncCallback callback, object state);
        IBrokeredMessage EndReceive(IAsyncResult result);

        void Close();

        ReceiveMode Mode {
            get;
        }

    }
}
