using System;
using System.Threading;

namespace ProjectExtensions.Azure.ServiceBus.Tests.Unit.Mocks {

    class MockIAsyncResult : IAsyncResult {

        WaitHandle handle = null;

        public object AsyncState {
            get;
            set;
        }

        public WaitHandle AsyncWaitHandle {
            get {
                if (handle == null) {
                    handle = new ManualResetEvent(false);
                }
                return handle;
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
