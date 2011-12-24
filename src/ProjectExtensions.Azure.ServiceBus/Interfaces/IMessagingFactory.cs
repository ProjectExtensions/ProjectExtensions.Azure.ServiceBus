using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;

namespace ProjectExtensions.Azure.ServiceBus.Interfaces {

    interface IMessagingFactory {
        SubscriptionClient CreateSubscriptionClient(string topicPath, string name, ReceiveMode receiveMode);
        TopicClient CreateTopicClient(string path);
        void Close();
        void Initialize(Uri serviceUri, TokenProvider tokenProvider);
    }
}
