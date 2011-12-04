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

namespace Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling
{
    using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.AzureStorage;
    using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.ServiceBus;
    using Microsoft.Practices.TransientFaultHandling;

    /// <summary>
    /// Non-static entry point to the retry functionality.
    /// </summary>
    public abstract class RetryManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryManager"/> class.
        /// </summary>
        /// <param name="defaultRetryStrategyName">The default retry strategy.</param>
        /// <param name="defaultSqlConnectionStrategyName">The default retry strategy for SQL connections.</param>
        /// <param name="defaultSqlCommandStrategyName">The default retry strategy for SQL commands.</param>
        /// <param name="defaultAzureServiceBusStrategyName">The default retry strategy for Windows Azure Service Bus.</param>
        /// <param name="defaultAzureCachingStrategyName">The default retry strategy for Windows Azure Caching.</param>
        /// <param name="defaultAzureStorageStrategyName">The default retry strategy for Windows Azure Storage.</param>
        protected RetryManager(string defaultRetryStrategyName, string defaultSqlConnectionStrategyName, string defaultSqlCommandStrategyName, string defaultAzureServiceBusStrategyName, string defaultAzureCachingStrategyName, string defaultAzureStorageStrategyName)
        {
            this.DefaultRetryStrategyName = defaultRetryStrategyName;
            this.DefaultSqlConnectionStrategyName = string.IsNullOrEmpty(defaultSqlConnectionStrategyName) ? this.DefaultRetryStrategyName : defaultSqlConnectionStrategyName;
            this.DefaultSqlCommandStrategyName = string.IsNullOrEmpty(defaultSqlCommandStrategyName) ? this.DefaultRetryStrategyName : defaultSqlCommandStrategyName;
            this.DefaultAzureServiceBusStrategyName = string.IsNullOrEmpty(defaultAzureServiceBusStrategyName) ? this.DefaultRetryStrategyName : defaultAzureServiceBusStrategyName;
            this.DefaultAzureCachingStrategyName = string.IsNullOrEmpty(defaultAzureCachingStrategyName) ? this.DefaultRetryStrategyName : defaultAzureCachingStrategyName;
            this.DefaultAzureStorageStrategyName = string.IsNullOrEmpty(defaultAzureStorageStrategyName) ? this.DefaultRetryStrategyName : defaultAzureStorageStrategyName;
        }

        /// <summary>
        /// Gets or sets the default retry strategy name.
        /// </summary>
        public string DefaultRetryStrategyName { get; protected set; }

        /// <summary>
        /// Gets or sets the default SQL connection retry strategy name.
        /// </summary>
        public string DefaultSqlConnectionStrategyName { get; protected set; }

        /// <summary>
        /// Gets or sets the default SQL command retry strategy name.
        /// </summary>
        public string DefaultSqlCommandStrategyName { get; protected set; }

        /// <summary>
        /// Gets or sets the default Windows Azure Service Bus retry strategy name.
        /// </summary>
        public string DefaultAzureServiceBusStrategyName { get; protected set; }

        /// <summary>
        /// Gets or sets the default Windows Azure Caching retry strategy name.
        /// </summary>
        public string DefaultAzureCachingStrategyName { get; protected set; }

        /// <summary>
        /// Gets or sets the default Windows Azure Storage retry strategy name.
        /// </summary>
        public string DefaultAzureStorageStrategyName { get; protected set; }

        /// <summary>
        /// Return a retry policy with the specified error detection strategy and the default retry strategy defined in the config. 
        /// </summary>
        /// <typeparam name="T">The type implementing the <see cref="ITransientErrorDetectionStrategy"/> interface that is responsible for detecting transient conditions.</typeparam>
        /// <returns>A new retry policy with the specified error detection strategy and the default retry strategy defined in the config</returns>
        public virtual RetryPolicy<T> GetRetryPolicy<T>()
            where T : ITransientErrorDetectionStrategy, new()
        {
            return new RetryPolicy<T>(this.GetRetryStrategy());
        }

        /// <summary>
        /// Return a retry policy with the specified error detection strategy and retry strategy matching the specified name. 
        /// </summary>
        /// <typeparam name="T">The type implementing the <see cref="ITransientErrorDetectionStrategy"/> interface that is responsible for detecting transient conditions.</typeparam>
        /// <param name="retryStrategyName">The retry strategy name as defined in the config.</param>
        /// <returns>A new retry policy with the specified error detection strategy and the default retry strategy defined in the config</returns>
        public virtual RetryPolicy<T> GetRetryPolicy<T>(string retryStrategyName)
            where T : ITransientErrorDetectionStrategy, new()
        {
            return new RetryPolicy<T>(this.GetRetryStrategy(retryStrategyName));
        }

        /// <summary>
        /// Return the default retry strategy defined in the config.
        /// </summary>
        /// <returns>The retry strategy matching the default strategy.</returns>
        public RetryStrategy GetRetryStrategy()
        {
            return this.GetRetryStrategy(this.DefaultRetryStrategyName);
        }

        /// <summary>
        /// Return the retry matching the specified name.
        /// </summary>
        /// <param name="retryStrategyName">The retry Strategy Name.</param>
        /// <returns>The retry matching the specified name.</returns>
        public abstract RetryStrategy GetRetryStrategy(string retryStrategyName);

        /// <summary>
        /// Return the default retry strategy for SqlConnection.
        /// </summary>
        /// <returns>The default SqlConnection retry strategy (or the default strategy if no default could be found).</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "As designed")]
        public RetryStrategy GetDefaultSqlConnectionRetryStrategy()
        {
            return GetRetryStrategy(this.DefaultSqlConnectionStrategyName);
        }

        /// <summary>
        /// Return the default retry strategy for SqlCommand.
        /// </summary>
        /// <returns>The default SqlConnection retry strategy (or the default strategy if no default could be found).</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "As designed")]
        public virtual RetryStrategy GetDefaultSqlCommandRetryStrategy()
        {
            return GetRetryStrategy(this.DefaultSqlCommandStrategyName);
        }

        /// <summary>
        /// Return the default retry strategy for Windows Azure Service.
        /// </summary>
        /// <returns>The default Windows Azure Service retry strategy (or the default strategy if no default could be found).</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "As designed")]
        public virtual RetryStrategy GetDefaultAzureServiceBusRetryStrategy()
        {
            return GetRetryStrategy(this.DefaultAzureServiceBusStrategyName);
        }

        /// <summary>
        /// Return the default retry strategy for Windows Azure Caching.
        /// </summary>
        /// <returns>The default Windows Azure Caching retry strategy (or the default strategy if no default could be found).</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "As designed")]
        public virtual RetryStrategy GetDefaultAzureCachingRetryStrategy()
        {
            return GetRetryStrategy(this.DefaultAzureCachingStrategyName);
        }

        /// <summary>
        /// Return the default retry strategy for Windows Azure Storage.
        /// </summary>
        /// <returns>The default Windows Azure Storage retry strategy (or the default strategy if no default could be found).</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "As designed")]
        public virtual RetryStrategy GetDefaultAzureStorageRetryStrategy()
        {
            return GetRetryStrategy(this.DefaultAzureStorageStrategyName);
        }

        /// <summary>
        /// Returns the default retry policy dedicated to handling transient conditions with Windows Azure Service Bus.
        /// </summary>
        /// <returns>The retry policy for Windows Azure Service Bus with the corresponding default strategy (or the default strategy if no retry strategy definition assigned to Windows Azure Service Bus was found).</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "As designed")]
        public virtual RetryPolicy GetDefaultAzureServiceBusRetryPolicy()
        {
            return new RetryPolicy<ServiceBusTransientErrorDetectionStrategy>(this.GetDefaultAzureServiceBusRetryStrategy());
        }

        /// <summary>
        /// Returns the default retry policy dedicated to handling transient conditions with Windows Azure Storage.
        /// </summary>
        /// <returns>The retry policy for Windows Azure Storage with the corresponding default strategy (or the default strategy if no retry strategy definition assigned to Windows Azure Storage was found).</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "As designed")]
        public virtual RetryPolicy GetDefaultAzureStorageRetryPolicy()
        {
            return new RetryPolicy<StorageTransientErrorDetectionStrategy>(this.GetDefaultAzureStorageRetryStrategy());
        }
    }
}
