using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Net;

namespace ProjectExtensions.Azure.ServiceBus.TransientFaultHandling.ServiceBus {

    /// <summary>
    /// When we verify if an item exits, we consider a 404 to NOT be Transient.
    /// </summary>
    class ServiceBusTransientErrorToDetermineExistanceDetectionStrategy : ServiceBusTransientErrorDetectionStrategy {

        protected override bool CheckIsTransientInternal(Exception ex) {

            if (ex is MessagingEntityNotFoundException) {
                return false;
            }
            else if (ex is WebException) {
                if (ex.Message.IndexOf("404") > -1) {
                    return false;
                }
            }

            return base.CheckIsTransientInternal(ex);
        }

    }
}
