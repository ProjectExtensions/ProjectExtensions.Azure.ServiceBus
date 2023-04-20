using NLog;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using System;
using System.IO;
using System.IO.Compression;
using System.Xml;
using System.Xml.Serialization;

namespace Amazon.ServiceBus.DistributedMessages.Serializers {

    public class GZipXmlSerializer : ServiceBusSerializerBase {

        static Logger logger = LogManager.GetCurrentClassLogger();
        MemoryStream serializedStream;

        public override IServiceBusSerializer Create() {
            return new GZipXmlSerializer();
        }

        public override Stream Serialize(object obj) {
            var serial = new XmlSerializer(obj.GetType());
            serializedStream = new MemoryStream();

            using (var zipStream = new GZipStream(serializedStream, CompressionMode.Compress, true)) {
                using (var writer = XmlDictionaryWriter.CreateBinaryWriter(zipStream, null, null, false)) {
                    serial.Serialize(writer, obj);
                }
            }

            serializedStream.Position = 0; //make sure you always set the stream position to where you want to serialize.
            logger.Debug("Serialize {0} at Bytes={1}", obj.GetType(), serializedStream.Length);
            return serializedStream;
        }

        public override object Deserialize(Stream stream, Type type) {
            var serial = new XmlSerializer(type);
            logger.Debug("Deserialize {0} at {1} bytes", type, stream.Length);
            using (var zipStream = new GZipStream(stream, CompressionMode.Decompress, true)) {
                using (var reader = XmlDictionaryReader.CreateBinaryReader(zipStream, XmlDictionaryReaderQuotas.Max)) {
                    return serial.Deserialize(reader);
                }
            }
        }

        public override void Dispose() {
            if (serializedStream != null) {
                serializedStream.Dispose();
                serializedStream = null;
            }
        }
    }
}
