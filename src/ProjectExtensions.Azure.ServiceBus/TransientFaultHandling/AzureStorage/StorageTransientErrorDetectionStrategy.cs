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

namespace Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.AzureStorage
{
    #region Using references

    using System;
    using System.Data.Services.Client;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;

    using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Properties;
    using Microsoft.Practices.TransientFaultHandling;
    using Microsoft.WindowsAzure.StorageClient;

    #endregion

    /// <summary>
    /// Provides the transient error detection logic that can recognize transient faults when dealing with Windows Azure storage services.
    /// </summary>
    public class StorageTransientErrorDetectionStrategy : ITransientErrorDetectionStrategy
    {
        /// <summary>
        /// Determines whether the specified exception represents a transient failure that can be compensated by a retry.
        /// </summary>
        /// <param name="ex">The exception object to be verified.</param>
        /// <returns>True if the specified exception is considered as transient, otherwise false.</returns>
        public bool IsTransient(Exception ex)
        {
            return ex != null && (CheckIsTransient(ex) || (ex.InnerException != null && CheckIsTransient(ex.InnerException)));
        }

        private static bool CheckIsTransient(Exception ex)
        {
            var webException = ex as WebException;

            if (webException != null && (webException.Status == WebExceptionStatus.ProtocolError || webException.Status == WebExceptionStatus.ConnectionClosed))
            {
                return true;
            }

            var dataServiceException = ex as DataServiceRequestException;

            if (dataServiceException != null)
            {
                if (IsErrorStringMatch(GetErrorCode(dataServiceException), StorageErrorCodeStrings.InternalError, StorageErrorCodeStrings.ServerBusy, StorageErrorCodeStrings.OperationTimedOut, TableErrorCodeStrings.TableServerOutOfMemory))
                {
                    return true;
                }
            }

            var serverException = ex as StorageServerException;

            if (serverException != null)
            {
                if (IsErrorCodeMatch(serverException, StorageErrorCode.ServiceInternalError, StorageErrorCode.ServiceTimeout))
                {
                    return true;
                }

                if (IsErrorStringMatch(serverException, StorageErrorCodeStrings.InternalError, StorageErrorCodeStrings.ServerBusy, StorageErrorCodeStrings.OperationTimedOut))
                {
                    return true;
                }
            }

            var storageException = ex as StorageClientException;

            if (storageException != null)
            {
                if (IsErrorStringMatch(storageException, StorageErrorCodeStrings.InternalError, StorageErrorCodeStrings.ServerBusy, TableErrorCodeStrings.TableServerOutOfMemory))
                {
                    return true;
                }
            }

            if (ex is TimeoutException)
            {
                return true;
            }

            return false;
        }

        #region Private members
        private static string GetErrorCode(DataServiceRequestException ex)
        {
            if (ex != null && ex.InnerException != null)
            {
                var regEx = new Regex(Resources.GetErrorCodeRegEx, RegexOptions.IgnoreCase);
                var match = regEx.Match(ex.InnerException.Message);

                return match.Groups[1].Value;
            }
            else
            {
                return null;
            }
        }

        private static bool IsErrorCodeMatch(StorageException ex, params StorageErrorCode[] codes)
        {
            return ex != null && codes.Contains(ex.ErrorCode);
        }

        private static bool IsErrorStringMatch(StorageException ex, params string[] errorStrings)
        {
            return ex != null && ex.ExtendedErrorInformation != null && errorStrings.Contains(ex.ExtendedErrorInformation.ErrorCode);
        }

        private static bool IsErrorStringMatch(string exceptionErrorString, params string[] errorStrings)
        {
            return errorStrings.Contains(exceptionErrorString);
        }
        #endregion
    }
}
