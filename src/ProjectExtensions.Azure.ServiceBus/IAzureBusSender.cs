using System;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using System.Collections.Generic;

namespace ProjectExtensions.Azure.ServiceBus {
    
    interface IAzureBusSender {
        void Close();
        void Dispose(bool disposing);
        void Send<T>(T obj, IDictionary<string, object> metadata);
        void Send<T>(T obj, IDictionary<string, object> metadata, IServiceBusSerializer serializer);
    }

}
