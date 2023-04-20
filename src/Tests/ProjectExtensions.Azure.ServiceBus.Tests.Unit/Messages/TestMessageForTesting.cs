using System;

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
