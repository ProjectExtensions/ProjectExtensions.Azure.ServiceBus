using ProjectExtensions.Azure.ServiceBus.Helpers;
using ProjectExtensions.Azure.ServiceBus.Receiver;
using System;
using System.Linq;
using System.Reflection;

namespace ProjectExtensions.Azure.ServiceBus {

    class ServiceBusEnpointData {

        string _subscriptionNameDebug;

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

        public string SubscriptionNameDebug {
            get {
                if (_subscriptionNameDebug == null) {
                    _subscriptionNameDebug = DeclaredType.FullName.Replace("`1", "") + "<" + MessageType.FullName + "> - " + SubscriptionName;
                }
                return _subscriptionNameDebug;
            }
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
