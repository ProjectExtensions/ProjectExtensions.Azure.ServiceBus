using Newtonsoft.Json;
using NLog;
using System;
using System.IO;

namespace ProjectExtensions.Azure.ServiceBus.Serialization {

    /// <summary>
    /// JSon Serializer that may be used to serialize messages
    /// </summary>
    public class JsonServiceBusSerializer : ServiceBusSerializerBase {

        static Logger logger = LogManager.GetCurrentClassLogger();
        Stream serializedStream;

        /// <summary>
        /// Create an instance of the serializer
        /// </summary>
        /// <returns></returns>
        public override IServiceBusSerializer Create() {
            return new JsonServiceBusSerializer();
        }

        /// <summary>
        /// Serialize the message
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override Stream Serialize(object obj) {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;

            serializedStream = new MemoryStream();

            StreamWriter sw = new StreamWriter(serializedStream);
            //do not wrap in using, we don't want to close the stream
            JsonWriter jw = new JsonTextWriter(sw);
            serializer.Serialize(jw, obj);
            jw.Flush();
            serializedStream.Position = 0; //make sure you always set the stream position to where you want to serialize.

            logger.Debug("Serialize {0} at {1} bytes", obj.GetType(), serializedStream.Length);

            return serializedStream;
        }

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public override object Deserialize(Stream stream, Type type) {
            logger.Debug("Deserialize {0} at {1} bytes", type, stream.Length);

            JsonSerializer serializer = new JsonSerializer();
            //serializer.Converters.Add(new JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Ignore;

            StreamReader sr = new StreamReader(stream);
            //do not wrap in using, we don't want to close the stream
            var jr = new JsonTextReader(sr);
            return serializer.Deserialize(jr, type);
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