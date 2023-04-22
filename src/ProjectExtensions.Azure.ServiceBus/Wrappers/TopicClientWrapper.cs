using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus.Messaging;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using System;

namespace ProjectExtensions.Azure.ServiceBus.Wrappers {

    class TopicClientWrapper : ITopicClient {

        TopicClient topicClient;

        public TopicClientWrapper(TopicClient topicClient) {
            Guard.ArgumentNotNull(topicClient, "topicClient");
            this.topicClient = topicClient;
        }

        public void Send(IBrokeredMessage message) {
            if (!(message is BrokeredMessageWrapper)) {
                throw new ArgumentOutOfRangeException("message", "message must be BrokeredMessage for Azure use");
            }
            topicClient.Send((message as BrokeredMessageWrapper).GetMessage());
        }

        public void Close() {
            topicClient.Close();
        }
    }
}
