using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus.Messaging;

namespace ProjectExtensions.Azure.ServiceBus {

    public class BrokeredMessageWrapper : IBrokeredMessage {

        BrokeredMessage message;

        public BrokeredMessageWrapper(BrokeredMessage message) {
            this.message = message;
        }

        public string ContentType {
            get {
                return message.ContentType;
            }
        }

        public string CorrelationId {
            get {
                return message.CorrelationId;
            }
        }

        public int DeliveryCount {
            get {
                return message.DeliveryCount;
            }
        }

        public DateTime EnqueuedTimeUtc {
            get {
                return message.EnqueuedTimeUtc;
            }
        }

        public DateTime ExpiresAtUtc {
            get {
                return message.ExpiresAtUtc;
            }
        }

        public string Label {
            get {
                return message.Label;
            }
        }

        public DateTime LockedUntilUtc {
            get {
                return message.LockedUntilUtc;
            }
        }

        public Guid LockToken {
            get {
                return message.LockToken;
            }
        }

        public string MessageId {
            get {
                return message.MessageId;
            }
        }

        public string ReplyTo {
            get {
                return message.ReplyTo;
            }
        }

        public string ReplyToSessionId {
            get {
                return message.ReplyToSessionId;
            }
        }

        public DateTime ScheduledEnqueueTimeUtc {
            get {
                return message.ScheduledEnqueueTimeUtc;
            }
        }

        public long SequenceNumber {
            get {
                return message.SequenceNumber;
            }
        }

        public string SessionId {
            get {
                return message.SessionId;
            }
        }

        public long Size {
            get {
                return message.Size;
            }
        }

        public TimeSpan TimeToLive {
            get {
                return message.TimeToLive;
            }
        }

        public string To {
            get {
                return message.To;
            }
        }
    }
}
