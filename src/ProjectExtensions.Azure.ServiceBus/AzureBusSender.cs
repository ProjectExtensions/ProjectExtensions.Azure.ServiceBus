using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using NLog;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Sender class that publishes messages to the bus
    /// </summary>
    class AzureBusSender : AzureSenderReceiverBase, IAzureBusSender {

        static Logger logger = LogManager.GetCurrentClassLogger();
        TopicClient client;

        public AzureBusSender(BusConfiguration configuration)
            : base(configuration) {
            client = factory.CreateTopicClient(topic.Path);
        }

        public void Close() {
            if (client != null) {
                client.Close();
                client = null;
            }
        }

        public void Send<T>(T obj, IDictionary<string, object> metadata) {
            using (var serial = configuration.DefaultSerializer.Create()) {
                Send<T>(obj, metadata, serial);
            }
        }

        public void Send<T>(T obj, IDictionary<string, object> metadata, IServiceBusSerializer serializer) {

            using (BrokeredMessage message = new BrokeredMessage(serializer.Serialize(obj), false)) {
                message.MessageId = Guid.NewGuid().ToString();
                message.Properties.Add(TYPE_HEADER_NAME, obj.GetType().FullName.Replace('.', '_'));

                if (metadata != null) {
                    foreach (var item in metadata) {
                        message.Properties.Add(item.Key, item.Value);
                    }
                }

                logger.Log(LogLevel.Info, "Send Type={0} Serializer={1} MessageId={2}", obj.GetType().FullName, serializer.GetType().FullName, message.MessageId);
                Helpers.Execute(() => client.Send(message));
            }
        }

        public override void Dispose(bool disposing) {
            Close();
        }
    }
}
