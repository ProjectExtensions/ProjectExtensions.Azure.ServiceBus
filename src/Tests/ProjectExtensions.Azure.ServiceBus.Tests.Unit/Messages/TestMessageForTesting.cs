using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Messages {

    class TestMessageForTesting {

        public TestMessageForTesting() {
            Id = Guid.NewGuid();
        }

        public Guid Id {
            get;
            set;
        }

    }
}
