using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Microsoft.ServiceBus;

namespace ProjectExtensions.Azure.ServiceBus.Interfaces {
    
    /// <summary>
    /// Interface used to abstract the NamespaceManager
    /// </summary>
    interface INamespaceManager {

        void Initialize(Uri serviceUri, TokenProvider tokenProvider);

        SubscriptionDescription CreateSubscription(SubscriptionDescription description, Filter filter);
        TopicDescription CreateTopic(TopicDescription description);

        void DeleteSubscription(string topicPath, string name);

        SubscriptionDescription GetSubscription(string topicPath, string name);
        TopicDescription GetTopic(string path);

        bool SubscriptionExists(string topicPath, string name);

    }
}
