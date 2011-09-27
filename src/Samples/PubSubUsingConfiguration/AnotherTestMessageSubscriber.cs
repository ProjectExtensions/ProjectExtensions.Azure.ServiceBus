using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus;

namespace PubSubUsingConfiguration {

    public class AnotherTestMessageSubscriber : IHandleCompetingMessages<AnotherTestMessage> {
        public bool IsReusable {
            get {
                return true;
            }
        }

        public void Handle(AnotherTestMessage message, IDictionary<string, object> metadata) {
            //throw new NotImplementedException();
        }
    }
}
