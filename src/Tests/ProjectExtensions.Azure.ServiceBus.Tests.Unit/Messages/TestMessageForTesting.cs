using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Messages {

    public class TestMessageForTesting {

        static int counter;

        public TestMessageForTesting() {
            Id = Guid.NewGuid();
            counter++;
            this.Counter = counter;
        }

        public int Counter {
            get;
            set;
        }

        public Guid Id {
            get;
            set;
        }

    }
}
