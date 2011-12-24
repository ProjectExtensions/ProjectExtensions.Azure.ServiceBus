using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Interfaces;
using Microsoft.ServiceBus.Messaging;
using Microsoft.Practices.TransientFaultHandling;

namespace ProjectExtensions.Azure.ServiceBus.Wrappers {
    
    class TopicClientWrapper : ITopicClient {

        TopicClient topicClient;

        public TopicClientWrapper(TopicClient topicClient) {
            Guard.ArgumentNotNull(topicClient, "topicClient");
            this.topicClient = topicClient;
        }

        public IAsyncResult BeginSend(BrokeredMessage message, AsyncCallback callback, object state) {
            return topicClient.BeginSend(message, callback, state);
        }

        public void EndSend(IAsyncResult result) {
            topicClient.EndSend(result);
        }

        public void Close() {
            topicClient.Close();
        }
    }
}
