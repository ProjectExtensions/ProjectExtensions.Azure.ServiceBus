using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.ServiceBus;

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
