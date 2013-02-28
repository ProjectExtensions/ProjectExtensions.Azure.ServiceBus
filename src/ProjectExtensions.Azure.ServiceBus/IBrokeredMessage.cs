using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//DO NOT Change the namespace.
namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Interface for the BrokeredMessage
    /// </summary>
    public interface IBrokeredMessage {

        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        string ContentType {
            get;
        }

        /// <summary>
        /// Gets or sets the identifier of the correlation.
        /// </summary>
        string CorrelationId {
            get;
        }

        /// <summary>
        /// Gets the number of deliveries.
        /// </summary>
        int DeliveryCount {
            get;
        }

        /// <summary>
        /// Gets or sets the date and time of the sent time in UTC.
        /// </summary>
        DateTime EnqueuedTimeUtc {
            get;
        }

        /// <summary>
        /// Gets the date and time in UTC at which the message is set to expire.
        /// </summary>
        DateTime ExpiresAtUtc {
            get;
        }

        /// <summary>
        /// Gets or sets the application specific label.
        /// </summary>
        string Label {
            get;
        }

        /// <summary>
        /// Gets the date and time in UTC until which the message will be locked in the queue/subscription.
        /// </summary>
        DateTime LockedUntilUtc {
            get;
        }

        /// <summary>
        /// Gets the lock token assigned by Service Bus to this message.
        /// </summary>
        Guid LockToken {
            get;
        }

        /// <summary>
        /// Gets or sets the identifier of the message.
        /// </summary>
        string MessageId {
            get;
            set;
        }

        /// <summary>
        /// Gets the application specific message properties.
        /// </summary>
        IDictionary<string, object> Properties {
            get;
        }

        /// <summary>
        /// Gets or sets the address of the queue to reply to.
        /// </summary>
        string ReplyTo {
            get;
        }

        /// <summary>
        /// Gets or sets the session identifier to reply to.
        /// </summary>
        string ReplyToSessionId {
            get;
        }

        /// <summary>
        /// Gets or sets the date and time in UTC at which the message will be enqueued.
        /// </summary>
        DateTime ScheduledEnqueueTimeUtc {
            get;
        }

        /// <summary>
        /// Gets the unique number assigned to a message by the Service Bus.
        /// </summary>
        long SequenceNumber {
            get;
        }

        /// <summary>
        /// Gets or sets the identifier of the session.
        /// </summary>
        string SessionId {
            get;
        }

        /// <summary>
        /// Gets the size of the message in bytes.
        /// </summary>
        long Size {
            get;
        }

        /// <summary>
        /// Gets or sets the message’s time to live value. This is the duration after which the message expires, starting from when the message is sent to the Service Bus. Messages older than their TimeToLive value will expire and no longer be retained in the message store. Subscribers will be unable to receive expired messages.
        /// </summary>
        TimeSpan TimeToLive {
            get;
        }

        /// <summary>
        /// Gets or sets the send to address.
        /// </summary>
        string To {
            get;
        }

        /// <summary>
        /// Abandons the lock on a peek-locked message.
        /// </summary>
        void Abandon();

        /// <summary>
        /// Completes the receive operation of a message and indicates that the message should be marked as processed and deleted or archived.
        /// </summary>
        void Complete();

        /// <summary>
        /// Moves the message to the dead letter queue.
        /// </summary>
        /// <param name="deadLetterReason">The reason for deadlettering the message.</param>
        /// <param name="deadLetterErrorDescription">The description information for deadlettering the message.</param>
        void DeadLetter(string deadLetterReason, string deadLetterErrorDescription);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        void Dispose();

        /// <summary>
        /// Deserializes the brokered message body into an object of the specified type by using the supplied XmlObjectSerializer with a binary XmlDictionaryReader.
        /// </summary>
        /// <typeparam name="T">The type to which the message body will be deserialized.</typeparam>
        /// <returns></returns>
        T GetBody<T>();

    }
}
