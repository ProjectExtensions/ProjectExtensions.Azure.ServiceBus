using System;

namespace ProjectExtensions.Azure.ServiceBus.Interfaces {

    interface IAzureBusReceiver {
        void CancelSubscription(ServiceBusEnpointData value);
        void CreateSubscription(ServiceBusEnpointData value);
        void Dispose(bool disposing);
    }

}
