using Microsoft.Practices.TransientFaultHandling;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Text;

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

        int maxConcurrentCalls;

        int maxDeliveryCount;
        bool maxDeliveryCountSet;

        int prefetchCount;
        bool prefetchCountSet;

        /// <summary>
        /// Create a new instance of the Attribute with the default settings.
        /// </summary>
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
        /// Send the letter to the default dead letter queue after the max retries.
        /// </summary>
        public bool DeadLetterAfterMaxRetries {
            get;
            set;
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
        /// Gets or sets the maximum number of concurrent calls to the callback the message pump should initiate.
        /// </summary>
        public int MaxConcurrentCalls {
            get {
                return maxConcurrentCalls;
            }
            set {
                Guard.ArgumentNotZeroOrNegativeValue(value, "value");
                maxConcurrentCalls = value;
            }
        }

        /// <summary>
        /// Gets or sets the number of maximum calls to your handler.
        /// </summary>
        public int MaxRetries {
            get {
                return maxDeliveryCount;
            }
            set {
                Guard.ArgumentNotZeroOrNegativeValue(value, "value");
                maxDeliveryCount = value;
                maxDeliveryCountSet = true;
            }
        }

        /// <summary>
        /// If we threw an error, pause the provided milliseconds before calling the handler again
        /// </summary>
        /// <remarks>
        /// The goal is to allow the application to have basic retry throttling logic.
        /// </remarks>
        public int PauseTimeIfErrorWasThrown {
            get;
            set;
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

        /// <summary>
        /// Is the message handler a singleton or instance member?
        /// </summary>
        public bool Singleton {
            get;
            set;
        }

        internal bool DefaultMessageTimeToLiveSet() {
            return defaultMessageTimeToLiveSet;
        }

        internal bool LockDurationSet() {
            return lockDurationSet;
        }

        internal bool MaxDeliveryCountSet() {
            return maxDeliveryCountSet;
        }

        internal bool PrefetchCountSet() {
            return prefetchCountSet;
        }

        /// <summary>
        /// Override of ToString();
        /// </summary>
        /// <returns>Data about the attribute normally used for debugging.</returns>
        public override string ToString() {
            var retVal = new StringBuilder();
            retVal.Append("DefaultMessageTimeToLive: ").Append(this.DefaultMessageTimeToLive);
            retVal.Append(" EnableBatchedOperations: ").Append(this.EnableBatchedOperations);
            retVal.Append(" EnableDeadLetteringOnMessageExpiration: ").Append(this.EnableDeadLetteringOnMessageExpiration);
            retVal.Append(" LockDuration: ").Append(this.LockDuration);
            retVal.Append(" MaxConcurrentCalls: ").Append(this.MaxConcurrentCalls);
            retVal.Append(" MaxDeliveryCount: ").Append(this.MaxRetries);
            retVal.Append(" PrefetchCount: ").Append(this.PrefetchCount);
            retVal.Append(" ReceiveMode: ").Append(this.ReceiveMode);
            return retVal.ToString();
        }

        //todo look at RequiresSession and if we can support a session client in the future.

        //todo look at TopicPath for the future as an override for the subscription.

        //TODO determine if we need a flag to blow away the subscription since we can't set the values otherwise.
    }
}
