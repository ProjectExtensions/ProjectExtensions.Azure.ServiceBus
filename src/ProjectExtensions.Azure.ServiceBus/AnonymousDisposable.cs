using NLog;
using System;
using System.Threading;

namespace ProjectExtensions.Azure.ServiceBus {

    internal sealed class AnonymousDisposable : IDisposable {

        static Logger logger = LogManager.GetCurrentClassLogger();

        readonly Action dispose;
        int isDisposed;

        public AnonymousDisposable(Action dispose) {
            this.dispose = dispose;
        }

        public void Dispose() {
            logger.Debug("Dispose start");
            if (Interlocked.Exchange(ref this.isDisposed, 1) == 0) {
                logger.Debug("Dispose this.isDisposed start");
                this.dispose();
                logger.Debug("Dispose this.isDisposed end");
            }
            logger.Debug("Dispose end");
        }
    }
}
