using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;

namespace ProjectExtensions.Azure.ServiceBus.Interfaces {

    interface IMessagingFactory {
        ISubscriptionClient CreateSubscriptionClient(string topicPath, string name, ReceiveMode receiveMode);
        ITopicClient CreateTopicClient(string path);
        void Close();
    }
}
