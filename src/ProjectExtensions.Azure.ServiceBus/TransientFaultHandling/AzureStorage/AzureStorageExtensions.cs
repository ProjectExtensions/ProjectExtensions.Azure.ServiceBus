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
    using Microsoft.Practices.TransientFaultHandling;

    /// <summary>
    /// Extends the RetryStrategy to allow using the retry strategies from the Transient Fault Handling Application Block with Windows Azure Store.
    /// </summary>
    public static class AzureStorageExtensions
    {
        /// <summary>
        /// Wrap a Transient Fault Handling Application Block retry strategy into a Microsoft.WindowsAzure.StorageClient.RetryPolicy.
        /// </summary>
        /// <param name="retryStrategy">The Transient Fault Handling Application Block retry strategy to wrap.</param>
        /// <returns>Returns a wrapped Transient Fault Handling Application Block retry strategy into a Microsoft.WindowsAzure.StorageClient.RetryPolicy.</returns>
        public static Microsoft.WindowsAzure.StorageClient.RetryPolicy AsAzureStorageClientRetryPolicy(this RetryStrategy retryStrategy)
        {
            Guard.ArgumentNotNull(retryStrategy, "retryStrategy");

            return () => new ShouldRetryWrapper(retryStrategy.GetShouldRetry()).ShouldRetry;
        }

        private class ShouldRetryWrapper
        {
            private readonly ShouldRetry shouldRetry;

            public ShouldRetryWrapper(ShouldRetry shouldRetry)
            {
                this.shouldRetry = shouldRetry;
            }

            public bool ShouldRetry(int retryCount, System.Exception lastException, out System.TimeSpan delay)
            {
                return this.shouldRetry(retryCount, lastException, out delay);
            }
        }
    }
}
