using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using ProjectExtensions.Azure.ServiceBus.Helpers;
using ProjectExtensions.Azure.ServiceBus.Receiver;

namespace ProjectExtensions.Azure.ServiceBus {

    class ServiceBusEnpointData {

        /// <summary>
        /// Custom attribute found on the class.
        /// </summary>
        public MessageHandlerConfigurationAttribute AttributeData {
            get;
            set;
        }

        /// <summary>
        /// The type containing the interface
        /// </summary>
        public Type DeclaredType {
            get;
            set;
        }

        public Type ServiceType {
            get;
            set;
        }

        public string SubscriptionName {
            get;
            set;
        }

        /// <summary>
        /// If true, it is a singleton
        /// </summary>
        public bool IsReusable {
            get {
                return AttributeData != null && AttributeData.Singleton;
            }
        }

        /// <summary>
        /// The message type
        /// </summary>
        public Type MessageType {
            get;
            set;
        }

        ObjectActivator activator = null;

        public object GetReceivedMessage(object[] obj) {
            if (activator == null) {
                var genericType = typeof(ReceivedMessage<>).MakeGenericType(MessageType);
                //object receivedMessage = Activator.CreateInstance(gt, new object[] { state.Message, msg, values });
                ConstructorInfo ctor = genericType.GetConstructors().First();
                activator = ReflectionHelper.GetActivator(ctor);
            }
            //create an instance:
            return activator(obj);
        }
    }
}
