using System;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using System.Collections.Generic;

namespace ProjectExtensions.Azure.ServiceBus {

    interface IAzureBusSender {
        void Close();
        void Dispose(bool disposing);
        void Send<T>(T obj);
        void Send<T>(T obj, IDictionary<string, object> metadata = null);
        void Send<T>(T obj, IServiceBusSerializer serializer = null, IDictionary<string, object> metadata = null);
        void SendAsync<T>(T obj, object state, Action<IMessageSentResult<T>> resultCallBack);
        void SendAsync<T>(T obj, object state, Action<IMessageSentResult<T>> resultCallBack, IDictionary<string, object> metadata = null);
        void SendAsync<T>(T obj, object state, Action<IMessageSentResult<T>> resultCallBack, IServiceBusSerializer serializer = null, IDictionary<string, object> metadata = null);
    }
}
