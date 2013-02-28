using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectExtensions.Azure.ServiceBus.Tests.Unit.Messages;
using System.Diagnostics;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Handlers {
   
    public class TestMessageForTestingHandler : IHandleMessages<TestMessageForTesting> {
       
       public void Handle(IReceivedMessage<TestMessageForTesting> message) {
           Debug.WriteLine("Received:" + message.BrokeredMessage.MessageId + " Value:" + message.Message.Id + " counter:" + message.Message.Counter);
       }
   }
}
