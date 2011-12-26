using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace ProjectExtensions.Azure.ServiceBus.Serialization {

    /// <summary>
    /// Default XmlSerializer that is used when serializing messages.
    /// </summary>
    public class XmlServiceBusSerializer : ServiceBusSerializerBase {

        static Logger logger = LogManager.GetCurrentClassLogger();
        MemoryStream serializedStream;

        /// <summary>
        /// Create an instance of the serializer
        /// </summary>
        /// <returns></returns>
        public override IServiceBusSerializer Create() {
            return new XmlServiceBusSerializer();
        }

        /// <summary>
        /// Serialize the message
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override Stream Serialize(object obj) {
            var serial = new XmlSerializer(obj.GetType());
            serializedStream = new MemoryStream();

            using (var writer = XmlDictionaryWriter.CreateBinaryWriter(serializedStream, null, null, false)) {
                serial.Serialize(writer, obj);
            }

            serializedStream.Position = 0; //make sure you always set the stream position to where you want to serialize.
            logger.Debug("Serialize {0} at Bytes={1}", obj.GetType(), serializedStream.Length);
            return serializedStream;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override object Deserialize(Stream stream, Type type) {
            var serial = new XmlSerializer(type);
            logger.Debug("Deserialize {0} at {1} bytes", type, stream.Length);
            using (var reader = XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max)) {
                return serial.Deserialize(reader);
            }
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public override void Dispose() {
            if (serializedStream != null) {
                serializedStream.Dispose();
                serializedStream = null;
            }
        }
    }
}
