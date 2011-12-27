using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Mocks {

    class MockIAsyncResult : IAsyncResult {
        
        public object AsyncState {
            get;
            set;
        }

        public WaitHandle AsyncWaitHandle {
            get {
                return new ManualResetEvent(false);
            }
        }

        public bool CompletedSynchronously {
            get {
                return false;
            }
        }

        public bool IsCompleted {
            get {
                return false;
            }
        }
    }
}
