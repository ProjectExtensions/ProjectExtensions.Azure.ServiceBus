using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectExtensions.Azure.ServiceBus {
    
    class ServiceBusEnpointData {

        /// <summary>
        /// The type containing the interface
        /// </summary>
        public Type DeclaredType {
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

        /// <summary>
        /// If the Item is IsReusable, this value will be set.
        /// </summary>
        public object StaticInstance {
            get;
            set;
        }
       
    }
}
