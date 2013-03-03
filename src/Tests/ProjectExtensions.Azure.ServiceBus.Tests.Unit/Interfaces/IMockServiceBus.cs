using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using ProjectExtensions.Azure.ServiceBus.Interfaces;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Interfaces {

    interface IMockServiceBus : IBus {

        SubscriptionDescription CreateSubscription(SubscriptionDescription description, Filter filter);

        ISubscriptionClient CreateSubscriptionClient(string topicPath, string name, ReceiveMode receiveMode);

        ITopicClient CreateTopicClient(string path);

        TopicDescription CreateTopic(TopicDescription description);

        void DeleteSubscription(string topicPath, string name);

        SubscriptionDescription GetSubscription(string topicPath, string name);

        TopicDescription GetTopic(string path);

        void MessageAbandon(IBrokeredMessage message);

        void MessageComplete(IBrokeredMessage message);

        void MessageDeadLetter(IBrokeredMessage message, string deadLetterReason, string deadLetterErrorDescription);

        bool SubscriptionExists(string topicPath, string name);

        /// <summary>
        /// send a message and let the receivers get a copy of the message.
        /// </summary>
        /// <param name="message"></param>
        void SendMessage(IBrokeredMessage message);

        IAsyncResult BeginReceive(ISubscriptionClient client, TimeSpan serverWaitTime, AsyncCallback callback, object state);

        IBrokeredMessage EndReceive(IAsyncResult result);
    }
}
