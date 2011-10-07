using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;
using Microsoft.AzureCAT.Samples.TransientFaultHandling;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Allows you to configure the subscription
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class MessageHandlerConfigurationAttribute : Attribute {

        //http://msdn.microsoft.com/en-us/library/hh181895.aspx

        int defaultMessageTimeToLive;
        bool defaultMessageTimeToLiveSet;

        int lockDuration;
        bool lockDurationSet;

        int maxDeliveryCount;
        bool maxDeliveryCountSet;

        int prefetchCount;
        bool prefetchCountSet;

        public MessageHandlerConfigurationAttribute() {
            EnableBatchedOperations = true;
        }

        /// <summary>
        /// Gets or sets the default message time to live for a subscription. (in seconds)
        /// </summary>
        public int DefaultMessageTimeToLive {
            get {
                return defaultMessageTimeToLive;
            }
            set {
                defaultMessageTimeToLive = value;
                defaultMessageTimeToLiveSet = true;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the batched operations are enabled.
        /// </summary>
        public bool EnableBatchedOperations {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value that indicates if a subscription has dead letter support when a message expires.
        /// </summary>
        public bool EnableDeadLetteringOnMessageExpiration {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the lock duration time span for the subscription. (in seconds)
        /// </summary>
        /// <remarks>30 seconds to 5 minutes??? Looking for more info.</remarks>
        public int LockDuration {
            get {
                return lockDuration;
            }
            set {
                lockDuration = value;
                lockDurationSet = true;
            }
        }

        /// <summary>
        /// Gets or sets the number of maximum deliveries.
        /// </summary>
        public int MaxDeliveryCount {
            get {
                return maxDeliveryCount;
            }
            set {
                Guard.ArgumentNotZeroOrNegativeValue(value, "value");
                maxDeliveryCount = value;
                maxDeliveryCountSet = true;
            }
        }

        //http://msdn.microsoft.com/en-us/library/hh144031.aspx

        /// <summary>
        /// Gets or sets the number of messages that the message receiver can simultaneously request.
        /// </summary>
        /// <remarks>This is a property of the SubscriptionClient</remarks>
        public int PrefetchCount {
            get {
                return prefetchCount;
            }
            set {
                Guard.ArgumentNotZeroOrNegativeValue(value, "value");
                prefetchCount = value;
                prefetchCountSet = true;
            }
        }

        /// <summary>
        /// Enumerates the values for the receive mode.
        /// </summary>
        /// <remarks>
        /// PeekLock
        /// Specifies the PeekLock receive mode.
        /// This mode receives the message but keeps it peek-locked until the receiver abandons the message.
        /// 
        /// ReceiveAndDelete
        /// Specifies the ReceiveAndDelete receive mode.
        ///This mode deletes the message after it is received.
        /// </remarks>
        public ReceiveMode ReceiveMode {
            get;
            set;
        }

        public bool DefaultMessageTimeToLiveSet() {
            return defaultMessageTimeToLiveSet;
        }

        public bool LockDurationSet() {
            return lockDurationSet;
        }

        public bool MaxDeliveryCountSet() {
            return maxDeliveryCountSet;
        }

        public bool PrefetchCountSet() {
            return prefetchCountSet;
        }

        public override string ToString() {
            var retVal = new StringBuilder();
            retVal.Append("DefaultMessageTimeToLive: ").Append(this.DefaultMessageTimeToLive);
            retVal.Append(" EnableBatchedOperations: ").Append(this.EnableBatchedOperations);
            retVal.Append(" EnableDeadLetteringOnMessageExpiration: ").Append(this.EnableDeadLetteringOnMessageExpiration);
            retVal.Append(" LockDuration: ").Append(this.LockDuration);
            retVal.Append(" MaxDeliveryCount: ").Append(this.MaxDeliveryCount);
            retVal.Append(" PrefetchCount: ").Append(this.PrefetchCount);
            retVal.Append(" ReceiveMode: ").Append(this.ReceiveMode);
            return retVal.ToString();
        }

        //todo look at RequiresSession and if we can support a session client in the future.

        //todo look at TopicPath for the future as an override for the subscription.
        
        //TODO determine if we need a flag to blow away the subscription since we can't set the values otherwise.
    }
}
