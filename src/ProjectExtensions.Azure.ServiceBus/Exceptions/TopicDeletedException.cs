using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectExtensions.Azure.ServiceBus.Exceptions {

    /// <summary>
    /// Exception that is returned when a topic is deleted
    /// </summary>
    public class TopicDeletedException : Exception {

        /// <summary>
        /// Create a new exception.
        /// </summary>
        public TopicDeletedException()
            : base("Topic was deleted during execution but was recreated. Message could not be sent.") {
        }

    }
}
