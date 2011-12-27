using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using ProjectExtensions.Azure.ServiceBus.Interfaces;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Interfaces {
    
    interface IMockServiceBus {

        SubscriptionDescription CreateSubscription(SubscriptionDescription description, Filter filter);

        ISubscriptionClient CreateSubscriptionClient(string topicPath, string name, ReceiveMode receiveMode);

        ITopicClient CreateTopicClient(string path);

        TopicDescription CreateTopic(TopicDescription description);

        void DeleteSubscription(string topicPath, string name);

        SubscriptionDescription GetSubscription(string topicPath, string name);

        TopicDescription GetTopic(string path);

        void MessageAbandon();

        void MessageComplete();

        void MessageDeadLetter(string deadLetterReason, string deadLetterErrorDescription);

        bool SubscriptionExists(string topicPath, string name);
    }
}
