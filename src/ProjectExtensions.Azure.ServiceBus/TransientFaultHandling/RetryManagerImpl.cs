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
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Practices.EnterpriseLibrary.WindowsAzure.TransientFaultHandling.Properties;
    using Microsoft.Practices.TransientFaultHandling;

    /// <summary>
    /// Non-static entry point to the retry functionality.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "As designed. Naming convention.")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Impl", Justification = "As designed. Naming convention.")]
    public class RetryManagerImpl : RetryManager
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification = "Used as read only")]
        private readonly IDictionary<string, RetryStrategy> retryStrategies;

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryManagerImpl"/> class.
        /// </summary>
        /// <param name="retryStrategies">The complete set of retry strategy.</param>
        /// <param name="defaultRetryStrategyName">The default retry strategy.</param>
        /// <param name="defaultSqlConnectionStrategyName">The default retry strategy for SQL connections.</param>
        /// <param name="defaultSqlCommandStrategyName">The default retry strategy for SQL commands.</param>
        /// <param name="defaultAzureServiceBusStrategyName">The default retry strategy for Windows Azure Service Bus.</param>
        /// <param name="defaultAzureCachingStrategyName">The default retry strategy for Windows Azure Caching.</param>
        /// <param name="defaultAzureStorageStrategyName">The default retry strategy for Windows Azure Storage.</param>
        public RetryManagerImpl(IEnumerable<RetryStrategy> retryStrategies, string defaultRetryStrategyName, string defaultSqlConnectionStrategyName, string defaultSqlCommandStrategyName, string defaultAzureServiceBusStrategyName, string defaultAzureCachingStrategyName, string defaultAzureStorageStrategyName)
            : this(retryStrategies.ToDictionary(p => p.Name), defaultRetryStrategyName, defaultSqlConnectionStrategyName, defaultSqlCommandStrategyName, defaultAzureServiceBusStrategyName, defaultAzureCachingStrategyName, defaultAzureStorageStrategyName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryManagerImpl"/> class.
        /// </summary>
        /// <param name="retryStrategies">The complete set of retry strategies.</param>
        /// <param name="defaultRetryStrategyName">The default retry strategy.</param>
        /// <param name="defaultSqlConnectionStrategyName">The default retry strategy for SQL connections.</param>
        /// <param name="defaultSqlCommandStrategyName">The default retry strategy for SQL commands.</param>
        /// <param name="defaultAzureServiceBusStrategyName">The default retry strategy for Windows Azure Service Bus.</param>
        /// <param name="defaultAzureCachingStrategyName">The default retry strategy for Windows Azure Caching.</param>
        /// <param name="defaultAzureStorageStrategyName">The default retry strategy for Windows Azure Storage.</param>
        public RetryManagerImpl(IDictionary<string, RetryStrategy> retryStrategies, string defaultRetryStrategyName, string defaultSqlConnectionStrategyName, string defaultSqlCommandStrategyName, string defaultAzureServiceBusStrategyName, string defaultAzureCachingStrategyName, string defaultAzureStorageStrategyName)
            : base(defaultRetryStrategyName, defaultSqlConnectionStrategyName, defaultSqlCommandStrategyName, defaultAzureServiceBusStrategyName, defaultAzureCachingStrategyName, defaultAzureStorageStrategyName)
        {
            this.retryStrategies = retryStrategies;
        }

        /// <summary>
        /// Return the retry matching the specified name.
        /// </summary>
        /// <param name="retryStrategyName">The retry Strategy Name.</param>
        /// <returns>The retry matching the specified name.</returns>
        public override RetryStrategy GetRetryStrategy(string retryStrategyName)
        {
            Guard.ArgumentNotNullOrEmptyString(retryStrategyName, "retryStrategyName");

            RetryStrategy retryStrategy;

            if (!this.retryStrategies.TryGetValue(retryStrategyName, out retryStrategy))
            {
                throw new ArgumentOutOfRangeException(string.Format(CultureInfo.CurrentCulture, Resources.RetryStrategyNotFound, retryStrategyName));
            }

            return retryStrategy;
        }
    }
}
