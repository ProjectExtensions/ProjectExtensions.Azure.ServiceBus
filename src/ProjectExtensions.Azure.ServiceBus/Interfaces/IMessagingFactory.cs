using Microsoft.ServiceBus.Messaging;
using System.IO;

namespace ProjectExtensions.Azure.ServiceBus.Interfaces {

    interface IMessagingFactory {
        ISubscriptionClient CreateSubscriptionClient(string topicPath, string name, ReceiveMode receiveMode);
        ITopicClient CreateTopicClient(string path);
        void Close();
        IBrokeredMessage CreateBrokeredMessage(Stream messageBodyStream);
    }
}
