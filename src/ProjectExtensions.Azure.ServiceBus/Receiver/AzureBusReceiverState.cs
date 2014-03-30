using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus.Messaging;
using ProjectExtensions.Azure.ServiceBus.Interfaces;

namespace ProjectExtensions.Azure.ServiceBus.Receiver {
    /// <summary>
    /// Class used to store everything needed in the state and also used so we can cancel.
    /// </summary>
    class AzureBusReceiverState {

        CancellationTokenSource cancelToken = new CancellationTokenSource();

        public CancellationTokenSource CancelToken {
            get {
                return cancelToken;
            }
        }

        /// <summary>
        /// Once the item has stopped running, it marks the state as cancelled.
        /// </summary>
        public bool Cancelled {
            get {
                return CancelToken.IsCancellationRequested && MessageLoopCompleted;
            }
        }

        public ISubscriptionClient Client {
            get;
            set;
        }

        public ServiceBusEnpointData EndPointData {
            get;
            set;
        }

        /// <summary>
        /// when a message loop is complete, this is called so things can be cleaned up.
        /// </summary>
        public bool MessageLoopCompleted {
            get;
            private set;
        }

        public void Cancel() {
            // Stop the message receive loop gracefully.
            cancelToken.Cancel();
        }

        /// <summary>
        /// Called when receive returnes from completing a message loop.
        /// </summary>
        public void SetMessageLoopCompleted() {
            MessageLoopCompleted = true;
        }

    }
}
