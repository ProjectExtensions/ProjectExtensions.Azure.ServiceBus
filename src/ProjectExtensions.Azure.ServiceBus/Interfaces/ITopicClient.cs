using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;

namespace ProjectExtensions.Azure.ServiceBus.Interfaces {
    
    interface ITopicClient {
        IAsyncResult BeginSend(IBrokeredMessage message, AsyncCallback callback, object state);
        void EndSend(IAsyncResult result);
        void Close();
    }
}
