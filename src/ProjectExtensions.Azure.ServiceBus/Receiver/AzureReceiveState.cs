using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Microsoft.ServiceBus.Messaging;
using ProjectExtensions.Azure.ServiceBus.Serialization;
using Microsoft.Practices.TransientFaultHandling;

namespace ProjectExtensions.Azure.ServiceBus.Receiver {
    
    class AzureReceiveState {

        public AzureReceiveState(AzureBusReceiverState data, MethodInfo methodInfo,
            IServiceBusSerializer serializer, BrokeredMessage message) {
            Guard.ArgumentNotNull(data, "data");
            Guard.ArgumentNotNull(methodInfo, "methodInfo");
            Guard.ArgumentNotNull(serializer, "serializer");
            Guard.ArgumentNotNull(message, "message");
            this.Data = data;
            this.MethodInfo = methodInfo;
            this.Serializer = serializer;
            this.Message = message;
        }

        public AzureBusReceiverState Data {
            get;
            set;
        }
        public MethodInfo MethodInfo {
            get;
            set;
        }
        private IServiceBusSerializer Serializer {
            get;
            set;
        }
        public BrokeredMessage Message {
            get;
            set;
        }

        public IServiceBusSerializer CreateSerializer() {
            return Serializer.Create();
        }
        /*

           //TODO create a cache for object creation.
        var gt = typeof(IReceivedMessage<>).MakeGenericType(data.EndPointData.MessageType);

        //set up the methodinfo
        var methodInfo = data.EndPointData.DeclaredType.GetMethod("Handle",
            new Type[] { gt, typeof(IDictionary<string, object>) });
             
         */
    }
}
