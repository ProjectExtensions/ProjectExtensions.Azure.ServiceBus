using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Interface for the BrokeredMessage
    /// </summary>
    public interface IBrokeredMessage {

        string ContentType {
            get;
        }
        string CorrelationId {
            get;
        }
        int DeliveryCount {
            get;
        }
        DateTime EnqueuedTimeUtc {
            get;
        }
        DateTime ExpiresAtUtc {
            get;
        }
        string Label {
            get;
        }
        DateTime LockedUntilUtc {
            get;
        }
        Guid LockToken {
            get;
        }
        string MessageId {
            get;
        }
        IDictionary<string, object> Properties {
            get;
        }
        string ReplyTo {
            get;
        }
        string ReplyToSessionId {
            get;
        }
        DateTime ScheduledEnqueueTimeUtc {
            get;
        }
        long SequenceNumber {
            get;
        }
        string SessionId {
            get;
        }
        long Size {
            get;
        }
        TimeSpan TimeToLive {
            get;
        }
        string To {
            get;
        }

        void Abandon();
        void Complete();
        void DeadLetter(string deadLetterReason, string deadLetterErrorDescription);
        void Dispose();

        T GetBody<T>();

    }
}
