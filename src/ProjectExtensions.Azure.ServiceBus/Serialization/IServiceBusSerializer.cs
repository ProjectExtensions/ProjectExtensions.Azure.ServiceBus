using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ProjectExtensions.Azure.ServiceBus.Serialization {

    /// <summary>
    /// Interface that must be implemented to register a custom serializer
    /// </summary>
    public interface IServiceBusSerializer : IDisposable {

        /// <summary>
        /// Create an instance of the serializer
        /// </summary>
        /// <returns></returns>
        IServiceBusSerializer Create();

        /// <summary>
        /// Serialize the message
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Stream Serialize(object obj);

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        object Deserialize(Stream stream, Type type);

        /// <summary>
        /// Deserialize the message
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        T Deserialize<T>(Stream stream);

    }
}
