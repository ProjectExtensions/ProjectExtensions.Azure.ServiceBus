using Microsoft.ServiceBus;
using System;

namespace ProjectExtensions.Azure.ServiceBus.Interfaces {

    interface IServiceBusTokenProvider {

        TokenProvider TokenProvider {
            get;
        }

        Uri ServiceUri {
            get;
        }
    }
}
