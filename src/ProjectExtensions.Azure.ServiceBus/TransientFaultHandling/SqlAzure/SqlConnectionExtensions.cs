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
namespace Microsoft.AzureCAT.Samples.TransientFaultHandling.SqlAzure
{
    #region Using references
    using System;
    using System.Data;
    using System.Data.SqlClient;
    #endregion

    /// <summary>
    /// Provides a set of extension methods adding retry capabilities into the standard <see cref="System.Data.SqlClient.SqlConnection"/> implementation.
    /// </summary>
    public static class SqlConnectionExtensions
    {
        /// <summary>
        /// Opens a database connection with the connection settings specified in the ConnectionString property of the connection object.
        /// Uses the default retry policy when opening the connection.
        /// </summary>
        /// <param name="connection">The connection object which is required as per extension method declaration.</param>
        public static void OpenWithRetry(this SqlConnection connection)
        {
            OpenWithRetry(connection, RetryPolicyFactory.GetDefaultSqlConnectionRetryPolicy());
        }

        /// <summary>
        /// Opens a database connection with the connection settings specified in the ConnectionString property of the connection object.
        /// Uses the specified retry policy when opening the connection.
        /// </summary>
        /// <param name="connection">The connection object which is required as per extension method declaration.</param>
        /// <param name="retryPolicy">The retry policy defining whether to retry a request if the connection fails to be opened.</param>
        public static void OpenWithRetry(this SqlConnection connection, RetryPolicy retryPolicy)
        {
            // Check if retry policy was specified, if not, use the default retry policy.
            (retryPolicy != null ? retryPolicy : RetryPolicy.NoRetry).ExecuteAction(() =>
            {
                connection.Open();
            });
        }
    }
}
