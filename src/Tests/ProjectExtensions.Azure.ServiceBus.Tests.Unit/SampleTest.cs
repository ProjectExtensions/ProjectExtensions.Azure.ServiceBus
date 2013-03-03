using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Threading;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit {
    
    [TestFixture]
    public class SampleTest {

        [Test]
        public void Test() {
            Thread.Sleep(2000);
            Assert.IsTrue(true);
        }

    }
}
