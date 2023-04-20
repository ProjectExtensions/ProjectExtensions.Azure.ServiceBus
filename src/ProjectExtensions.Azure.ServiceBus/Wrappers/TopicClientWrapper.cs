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

        public IAsyncResult BeginSend(IBrokeredMessage message, AsyncCallback callback, object state) {
            if (!(message is BrokeredMessageWrapper)) {
                throw new ArgumentOutOfRangeException("message", "message must be BrokeredMessage for Azure use");
            }
            return topicClient.BeginSend((message as BrokeredMessageWrapper).GetMessage(), callback, state);
        }

        public void EndSend(IAsyncResult result) {
            topicClient.EndSend(result);
        }

        public void Close() {
            topicClient.Close();
        }
    }
}
