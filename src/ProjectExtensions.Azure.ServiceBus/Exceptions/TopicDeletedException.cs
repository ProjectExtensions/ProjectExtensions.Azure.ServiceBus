using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectExtensions.Azure.ServiceBus.Exceptions {


    public class TopicDeletedException : Exception {

        public TopicDeletedException()
            : base("Topic was deleted during execution but was recreated. Message could not be sent.") {
        }

    }
}
