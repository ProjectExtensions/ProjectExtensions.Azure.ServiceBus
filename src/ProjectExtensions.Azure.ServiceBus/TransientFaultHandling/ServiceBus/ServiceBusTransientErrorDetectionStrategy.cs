//===============================================================================
// Microsoft patterns & practices Enterprise Library
// Transient Fault Handling Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

namespace Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.ServiceBus
{
    using System;
    using System.IdentityModel.Tokens;
    using System.Net;
    using System.Net.Sockets;
    using System.Security;
    using System.ServiceModel;
    using System.Text.RegularExpressions;
    using Microsoft.Practices.TransientFaultHandling;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    /// <summary> 
    /// Provides the transient error detection logic that can recognize transient faults when dealing with Windows Azure Service Bus. 
    /// </summary> 
    public class ServiceBusTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        /// <summary> 
        /// Provides a compiled regular expression used for extracting the error code from the message. 
        /// </summary> 
        private static readonly Regex acsErrorCodeRegEx = new Regex(@"Error:Code:(\d+):SubCode:(\w\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary> 
        /// Determines whether the specified exception represents a transient failure that can be compensated by a retry. 
        /// </summary> 
        /// <param name="ex">The exception object to be verified.</param> 
        /// <returns>True if the specified exception is considered as transient, otherwise false.</returns> 
        public bool IsTransient(Exception ex)
        {
            return ex != null && (CheckIsTransientInternal(ex) || (ex.InnerException != null && CheckIsTransientInternal(ex.InnerException)));
        }

        /// <summary>
        /// Since we call a static method, we allow you to override the call to change the behavior.
        /// </summary>
        /// <param name="ex">The error to check.</param>
        /// <returns></returns>
        protected virtual bool CheckIsTransientInternal(Exception ex) {
           return CheckIsTransient(ex);
        }

        // SecuritySafeCritical because it references MessageLockLostException, MessagingCommunicationException, 
        // MessagingEntityAlreadyExistsException, MessagingEntityNotFoundException, ServerBusyException, ServerErrorException
        [SecuritySafeCritical]
        private static bool CheckIsTransient(Exception ex)
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
            else if (ex is ServerBusyException)
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
            else if (ex is CommunicationException)
            {
                return true;
            }
            else if (ex is SocketException)
            {
                var socketFault = ex as SocketException;

                return socketFault.SocketErrorCode == SocketError.TimedOut;
            }
            else if (ex is UnauthorizedAccessException)
            {
                // Need to provide some resilience against the following fault that was seen a few times: 
                // System.UnauthorizedAccessException: The token provider was unable to provide a security token while accessing 'https://mysbns-sb.accesscontrol.windows.net/WRAPv0.9/'.  
                // Token provider returned message: 'Error:Code:500:SubCode:T9002:Detail:An internal network error occured. Please try again.'.  
                // System.IdentityModel.Tokens.SecurityTokenException: The token provider was unable to provide a security token while accessing 'https://mysbns-sb.accesscontrol.windows.net/WRAPv0.9/'.  
                // Token provider returned message: 'Error:Code:500:SubCode:T9002:Detail:An internal network error occured. Please try again.'.  
                // System.Net.WebException: The remote server returned an error: (500) Internal Server Error. 
                var match = acsErrorCodeRegEx.Match(ex.Message);
                var errorCode = 0;

                if (match.Success && match.Groups.Count > 1 && int.TryParse(match.Groups[1].Value, out errorCode))
                {
                    return errorCode == (int)HttpStatusCode.InternalServerError;
                }
            }

            return false;
        }
    }
}
