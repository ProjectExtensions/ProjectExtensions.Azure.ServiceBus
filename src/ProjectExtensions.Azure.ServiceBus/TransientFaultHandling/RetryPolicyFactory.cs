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
namespace Microsoft.AzureCAT.Samples.TransientFaultHandling
{
    #region Using statements
    using System;

    using Microsoft.AzureCAT.Samples.TransientFaultHandling.Configuration;
    using Microsoft.AzureCAT.Samples.TransientFaultHandling.SqlAzure;
    #endregion

    /// <summary>
    /// Provides a factory class for instantiating application-specific retry policies described in the application configuration.
    /// </summary>
    public static class RetryPolicyFactory
    {
        /// <summary>
        /// Returns the default retry policy dedicated to handling transient conditions with SQL connections.
        /// </summary>
        /// <returns>The retry policy for SQL connections, or the default <see cref="RetryPolicy.NoRetry"/> policy if no retry policy definition assigned to SQL connections was found.</returns>
        public static RetryPolicy GetDefaultSqlConnectionRetryPolicy()
        {
            RetryPolicyConfigurationSettings retryPolicySettings = ApplicationConfiguration.Current.GetConfigurationSection<RetryPolicyConfigurationSettings>(RetryPolicyConfigurationSettings.SectionName);

            if (retryPolicySettings != null)
            {
                RetryPolicy defaultPolicy = retryPolicySettings.GetRetryPolicy<SqlAzureTransientErrorDetectionStrategy>(retryPolicySettings.DefaultSqlConnectionPolicy);
                return defaultPolicy != null ? defaultPolicy : ((defaultPolicy = retryPolicySettings.GetRetryPolicy<SqlAzureTransientErrorDetectionStrategy>(retryPolicySettings.DefaultPolicy)) != null ? defaultPolicy : RetryPolicy.NoRetry);
            }
            else
            {
                return RetryPolicy.NoRetry;
            }
        }

        /// <summary>
        /// Returns the default retry policy dedicated to handling transient conditions with SQL commands.
        /// </summary>
        /// <returns>The retry policy for SQL commands, or the default <see cref="RetryPolicy.NoRetry"/> policy if no retry policy definition assigned to SQL commands was found.</returns>
        public static RetryPolicy GetDefaultSqlCommandRetryPolicy()
        {
            RetryPolicyConfigurationSettings retryPolicySettings = ApplicationConfiguration.Current.GetConfigurationSection<RetryPolicyConfigurationSettings>(RetryPolicyConfigurationSettings.SectionName);

            if (retryPolicySettings != null)
            {
                RetryPolicy defaultPolicy = retryPolicySettings.GetRetryPolicy<SqlAzureTransientErrorDetectionStrategy>(retryPolicySettings.DefaultSqlCommandPolicy);
                return defaultPolicy != null ? defaultPolicy : ((defaultPolicy = retryPolicySettings.GetRetryPolicy<SqlAzureTransientErrorDetectionStrategy>(retryPolicySettings.DefaultPolicy)) != null ? defaultPolicy : RetryPolicy.NoRetry);
            }
            else
            {
                return RetryPolicy.NoRetry;
            }
        }

        /// <summary>
        /// Returns an instance of the <see cref="RetryPolicy"/> object initialized for a given policy name.
        /// </summary>
        /// <typeparam name="T">The type implementing the <see cref="ITransientErrorDetectionStrategy"/> interface which is responsible for detecting transient conditions.</typeparam>
        /// <param name="policyName">The name under which a retry policy definition is registered in the application configuration.</param>
        /// <returns>The retry policy initialized from the specified policy definition, or the default <see cref="RetryPolicy.NoRetry"/> policy if the specified policy definition was found.</returns>
        public static RetryPolicy GetRetryPolicy<T>(string policyName) where T : ITransientErrorDetectionStrategy, new()
        {
            Guard.ArgumentNotNullOrEmptyString(policyName, "policyName");

            RetryPolicyConfigurationSettings retryPolicySettings = ApplicationConfiguration.Current.GetConfigurationSection<RetryPolicyConfigurationSettings>(RetryPolicyConfigurationSettings.SectionName);

            if (retryPolicySettings != null)
            {
                RetryPolicy defaultPolicy = retryPolicySettings.GetRetryPolicy<T>(policyName);
                return defaultPolicy != null ? defaultPolicy : ((defaultPolicy = retryPolicySettings.GetRetryPolicy<T>(retryPolicySettings.DefaultPolicy)) != null ? defaultPolicy : RetryPolicy.NoRetry);
            }
            else
            {
                return RetryPolicy.NoRetry;
            }
        }
    }
}
