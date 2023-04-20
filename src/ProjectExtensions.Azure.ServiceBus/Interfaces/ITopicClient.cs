using System;

namespace ProjectExtensions.Azure.ServiceBus.Interfaces {

    interface ITopicClient {
        IAsyncResult BeginSend(IBrokeredMessage message, AsyncCallback callback, object state);
        void EndSend(IAsyncResult result);
        void Close();
    }
}
