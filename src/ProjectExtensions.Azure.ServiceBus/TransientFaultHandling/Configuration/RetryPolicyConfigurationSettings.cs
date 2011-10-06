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
namespace Microsoft.AzureCAT.Samples.TransientFaultHandling.Configuration
{
    #region Using references
    using System;
    using System.Configuration;
    #endregion

    /// <summary>
    /// Implements a configuration section containing retry policy settings.
    /// </summary>
    [Serializable]
    public sealed class RetryPolicyConfigurationSettings : ConfigurationSection
    {
        #region Private members
        private const string DefaultPolicyProperty = "defaultPolicy";
        private const string DefaultSqlConnectionPolicyProperty = "defaultSqlConnectionPolicy";
        private const string DefaultSqlCommandPolicyProperty = "defaultSqlCommandPolicy";
        private const string DefaultStoragePolicyProperty = "defaultStoragePolicy";
        private const string DefaultCommunicationPolicyProperty = "defaultCommunicationPolicy";
        #endregion

        #region Public members
        /// <summary>
        /// The name of the configuration section represented by this type.
        /// </summary>
        public const string SectionName = "RetryPolicyConfiguration";
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of a <see cref="RetryPolicyConfigurationSettings"/> object using default settings.
        /// </summary>
        public RetryPolicyConfigurationSettings()
        {
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets or sets the name of the default general-purpose retry policy.
        /// </summary>
        [ConfigurationProperty(DefaultPolicyProperty, IsRequired = true)]
        public string DefaultPolicy
        {
            get { return (string)base[DefaultPolicyProperty]; }
            set { base[DefaultPolicyProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the name of a retry policy dedicated to handling transient conditions with SQL connections.
        /// </summary>
        [ConfigurationProperty(DefaultSqlConnectionPolicyProperty, IsRequired = false)]
        public string DefaultSqlConnectionPolicy
        {
            get { return (string)base[DefaultSqlConnectionPolicyProperty]; }
            set { base[DefaultSqlConnectionPolicyProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the name of a retry policy dedicated to handling transient conditions with SQL commands.
        /// </summary>
        [ConfigurationProperty(DefaultSqlCommandPolicyProperty, IsRequired = false)]
        public string DefaultSqlCommandPolicy
        {
            get { return (string)base[DefaultSqlCommandPolicyProperty]; }
            set { base[DefaultSqlCommandPolicyProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the name of a retry policy dedicated to handling transient conditions in Windows Azure storage services.
        /// </summary>
        [ConfigurationProperty(DefaultStoragePolicyProperty, IsRequired = false)]
        public string DefaultStoragePolicy
        {
            get { return (string)base[DefaultStoragePolicyProperty]; }
            set { base[DefaultStoragePolicyProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the name of a retry policy dedicated to handling transient conditions in the WCF communication infrastructure.
        /// </summary>
        [ConfigurationProperty(DefaultCommunicationPolicyProperty, IsRequired = false)]
        public string DefaultCommunicationPolicy
        {
            get { return (string)base[DefaultCommunicationPolicyProperty]; }
            set { base[DefaultCommunicationPolicyProperty] = value; }
        }

        /// <summary>
        /// Returns a collection of retry policy definitions represented by the <see cref="RetryPolicyInfo"/> object instances.
        /// </summary>
        [ConfigurationProperty("", Options = ConfigurationPropertyOptions.IsDefaultCollection)]
        [ConfigurationCollection(typeof(RetryPolicyInfo))]
        public RetryPolicyCollection Policies
        {
            get { return (RetryPolicyCollection)base[String.Empty]; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns an instance of the <see cref="RetryPolicy"/> object initialized for a given policy name.
        /// </summary>
        /// <typeparam name="T">The type implementing the <see cref="ITransientErrorDetectionStrategy"/> interface which is responsible for detecting transient conditions.</typeparam>
        /// <param name="policyName">The name under which a retry policy definition is registered in the application configuration.</param>
        /// <returns>The retry policy initialized from the specified policy definition, or a null reference if no such policy definition was found.</returns>
        public RetryPolicy<T> GetRetryPolicy<T>(string policyName) where T : ITransientErrorDetectionStrategy, new()
        {
            RetryPolicy<T> retryPolicy = null;

            if (!String.IsNullOrEmpty(policyName) && Policies.Contains(policyName))
            {
                RetryPolicyInfo retryPolicyInfo = Policies.Get(policyName);
                return retryPolicyInfo != null ? retryPolicyInfo.CreatePolicy<T>() : null;
            }

            return retryPolicy;
        }

        /// <summary>
        /// Adds a new definition describing a retry policy based on fixed interval retry mode.
        /// </summary>
        /// <param name="policyName">The name under which a retry policy definition will be registered in the application configuration.</param>
        /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
        /// <param name="retryInterval">The interval in milliseconds between retry attempts.</param>
        public void AddFixedIntervalPolicy(string policyName, int maxRetryCount, int retryInterval)
        {
            Policies.Add(new RetryPolicyInfo() { Name = policyName, MaxRetryCount = maxRetryCount, RetryInterval = retryInterval });
        }

        /// <summary>
        /// Adds a new definition describing a retry policy based on incremental interval retry mode.
        /// </summary>
        /// <param name="policyName">The name under which a retry policy definition will be registered in the application configuration.</param>
        /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
        /// <param name="retryInterval">The initial interval in milliseconds which will apply for the first retry.</param>
        /// <param name="retryIncrement">The incremental time value in milliseconds for calculating progressive delay between retry attempts.</param>
        public void AddIncrementalIntervalPolicy(string policyName, int maxRetryCount, int retryInterval, int retryIncrement)
        {
            Policies.Add(new RetryPolicyInfo() { Name = policyName, MaxRetryCount = maxRetryCount, RetryInterval = retryInterval, RetryIncrement = retryIncrement });
        }

        /// <summary>
        /// Adds a new definition describing a retry policy based on random exponential back-off retry mode.
        /// </summary>
        /// <param name="policyName">The name under which a retry policy definition will be registered in the application configuration.</param>
        /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
        /// <param name="minBackoff">The minimum back-off time in milliseconds.</param>
        /// <param name="maxBackoff">The maximum back-off time in milliseconds.</param>
        /// <param name="deltaBackof">The delta value in milliseconds to calculate random exponential delay between retry attempts.</param>
        public void AddExponentialIntervalPolicy(string policyName, int maxRetryCount, int minBackoff, int maxBackoff, int deltaBackof)
        {
            Policies.Add(new RetryPolicyInfo() { Name = policyName, MaxRetryCount = maxRetryCount, MinBackoff = minBackoff, MaxBackoff = maxBackoff, DeltaBackoff = deltaBackof });
        }
        #endregion
    }
}
