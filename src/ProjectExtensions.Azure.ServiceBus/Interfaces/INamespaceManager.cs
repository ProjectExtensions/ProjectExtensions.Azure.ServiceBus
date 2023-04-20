using Microsoft.ServiceBus.Messaging;

namespace ProjectExtensions.Azure.ServiceBus.Interfaces {

    /// <summary>
    /// Interface used to abstract the NamespaceManager
    /// </summary>
    interface INamespaceManager {

        SubscriptionDescription CreateSubscription(SubscriptionDescription description, Filter filter);
        TopicDescription CreateTopic(TopicDescription description);

        void DeleteSubscription(string topicPath, string name);

        SubscriptionDescription GetSubscription(string topicPath, string name);
        TopicDescription GetTopic(string path);

        bool SubscriptionExists(string topicPath, string name);

    }
}
