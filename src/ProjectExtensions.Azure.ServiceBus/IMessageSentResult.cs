using System;

//DO NOT Change the namespace.
namespace ProjectExtensions.Azure.ServiceBus {

    /// <summary>
    /// Result of a call to SendAsyc on the message bus
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMessageSentResult<T> {
        /// <summary>
        /// Did the message get sent successfully?
        /// </summary>
        bool IsSuccess {
            get;
            set;
        }

        /// <summary>
        /// The Exception thrown during the processing of sending the message.
        /// </summary>
        Exception ThrownException {
            get;
            set;
        }

        /// <summary>
        /// The actual time spent in milliseconds to send the message.
        /// </summary>
        TimeSpan TimeSpent {
            get;
            set;
        }

        /// <summary>
        /// A custom state object passed by the user.
        /// </summary>
        object State {
            get;
            set;
        }
    }
}
