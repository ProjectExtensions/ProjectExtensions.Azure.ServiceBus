using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            get;
            set;
        }

        /// <summary>
        /// The message type
        /// </summary>
        public Type MessageType {
            get;
            set;
        }       
    }
}
