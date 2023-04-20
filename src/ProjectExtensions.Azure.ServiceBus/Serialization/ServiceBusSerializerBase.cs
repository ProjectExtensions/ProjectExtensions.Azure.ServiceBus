using System;
using System.IO;

namespace ProjectExtensions.Azure.ServiceBus.Serialization {

    /// <summary>
    /// Abstract base class that may be used as a base class for serializers
    /// </summary>
    public abstract class ServiceBusSerializerBase : IServiceBusSerializer {

        /// <summary>
        /// Create an instance of the serializer
        /// </summary>
        /// <returns></returns>
        public abstract IServiceBusSerializer Create();

        /// <summary>
        /// Serialize the message
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public abstract Stream Serialize(object obj);

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract object Deserialize(Stream stream, Type type);

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public T Deserialize<T>(Stream stream) {
            return (T)Deserialize(stream, typeof(T));
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public abstract void Dispose();
    }
}
