using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;

namespace ProjectExtensions.Azure.ServiceBus.Interfaces {
    
    public interface ISubscriptionClient {

        IAsyncResult BeginReceive(TimeSpan serverWaitTime, AsyncCallback callback, object state);
        BrokeredMessage EndReceive(IAsyncResult result);

        void Close();

        ReceiveMode Mode {
            get;
        }

    }
}
