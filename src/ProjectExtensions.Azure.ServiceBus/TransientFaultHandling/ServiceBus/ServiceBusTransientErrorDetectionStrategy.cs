//=======================================================================================
// Transient Fault Handling Framework for SQL Azure, Storage, Service Bus & Cache
//
// This sample is supplemental to the technical guidance published on the Windows Azure
// Customer Advisory Team blog at http://windowsazurecat.com/. 
//
//=======================================================================================
// Copyright © 2011 Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER 
// EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF 
// MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. YOU BEAR THE RISK OF USING IT.
//=======================================================================================
namespace Microsoft.AzureCAT.Samples.TransientFaultHandling.ServiceBus
{
    #region Using references
    using System;
    using System.Net;
    using System.ServiceModel;
    using System.Net.Sockets;
    using System.IdentityModel.Tokens;

    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    #endregion

    /// <summary>
    /// Provides the transient error detection logic that can recognize transient faults when dealing with Windows Azure Service Bus.
    /// </summary>
    public class ServiceBusTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        /// <summary>
        /// Determines whether the specified exception represents a transient failure that can be compensated by a retry.
        /// </summary>
        /// <param name="ex">The exception object to be verified.</param>
        /// <returns>True if the specified exception is considered as transient, otherwise false.</returns>
        public bool IsTransient(Exception ex)
        {
            if (ex is FaultException)
            {
                return false;
            }
            else if (ex is MessagingEntityNotFoundException)
            {
                return false;
            }
            else if (ex is MessagingEntityAlreadyExistsException)
            {
                return false;
            }
            else if (ex is MessageLockLostException)
            {
                return false;
            }
            else if (ex is CommunicationObjectFaultedException)
            {
                return false;
            }
            else if (ex is MessagingCommunicationException)
            {
                return true;
            }
            else if (ex is CommunicationException)
            {
                return true;
            }
            else if (ex is TimeoutException)
            {
                return true;
            }
            else if (ex is WebException)
            {
                return true;
            }
            else if (ex is SecurityTokenException)
            {
                return true;
            }
            else if (ex is ServerTooBusyException)
            {
                return true;
            }
            else if (ex is ServerErrorException)
            {
                return true;
            }
            else if (ex is ProtocolException)
            {
                return true;
            }
            else if (ex is EndpointNotFoundException)
            {
                // This exception may occur when a listener and a consumer are being
                // initialized out of sync (e.g. consumer is reaching to a listener that
                // is still in the process of setting up the Service Host).
                return true;
            }
            else if (ex is SocketException)
            {
                var socketFault = ex as SocketException;

                return socketFault.SocketErrorCode == SocketError.TimedOut;
            }
            else if (ex is UnauthorizedAccessException)
            {
                return ex.GetInnerException<TimeoutException>() != null;
            }

            return false;
        }
    }
}
