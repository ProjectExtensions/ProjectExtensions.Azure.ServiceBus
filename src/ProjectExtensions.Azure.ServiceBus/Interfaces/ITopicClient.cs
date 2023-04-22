using System;

namespace ProjectExtensions.Azure.ServiceBus.Interfaces {

    interface ITopicClient {
        void Send(IBrokeredMessage message);
        void Close();
    }
}
