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
    /// Implements a configuration element holding retry policy parameters.
    /// </summary>
    public sealed class RetryPolicyInfo : ConfigurationElement
    {
        #region Private members
        private const string NameProperty = "name";
        private const string MaxRetryCountProperty = "maxRetryCount";
        private const string RetryIntervalProperty = "retryInterval";
        private const string MinBackoffProperty = "minBackoff";
        private const string MaxBackoffProperty = "maxBackoff";
        private const string DeltaBackoffProperty = "deltaBackoff";
        private const string RetryIncrementProperty = "retryIncrement";
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of a <see cref="RetryPolicyInfo"/> object with default settings.
        /// </summary>
        public RetryPolicyInfo()
        {
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Gets or sets the unique name of the retry policy under which it is registered and referenced in the application configuration.
        /// </summary>
        [ConfigurationProperty(NameProperty, IsRequired = true)]
        public string Name
        {
            get { return Convert.ToString(base[NameProperty]); }
            set { base[NameProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the maximum number of retry attempts. This parameter is compulsory.
        /// </summary>
        [ConfigurationProperty(MaxRetryCountProperty, IsRequired = true)]
        [IntegerValidator(MinValue = 0)]
        public int MaxRetryCount
        {
            get { return Convert.ToInt32(base[MaxRetryCountProperty]); }
            set {  base[MaxRetryCountProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the interval in milliseconds between retry attempts.
        /// </summary>
        [ConfigurationProperty(RetryIntervalProperty, IsRequired = false)]
        [IntegerValidator(MinValue = 0)]
        public int RetryInterval
        {
            get { return Convert.ToInt32(base[RetryIntervalProperty]); }
            set { base[RetryIntervalProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the minimum back-off time in milliseconds.
        /// </summary>
        [ConfigurationProperty(MinBackoffProperty, IsRequired = false)]
        [IntegerValidator(MinValue = 0)]
        public int MinBackoff
        {
            get { return Convert.ToInt32(base[MinBackoffProperty]); }
            set { base[MinBackoffProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the maximum back-off time in milliseconds.
        /// </summary>
        [ConfigurationProperty(MaxBackoffProperty, IsRequired = false)]
        [IntegerValidator(MinValue = 0)]
        public int MaxBackoff
        {
            get { return Convert.ToInt32(base[MaxBackoffProperty]); }
            set { base[MaxBackoffProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the delta value in milliseconds to calculate random exponential delay between retry attempts.
        /// </summary>
        [ConfigurationProperty(DeltaBackoffProperty, IsRequired = false)]
        [IntegerValidator(MinValue = 0)]
        public int DeltaBackoff
        {
            get { return Convert.ToInt32(base[DeltaBackoffProperty]); }
            set { base[DeltaBackoffProperty] = value; }
        }

        /// <summary>
        /// Gets or sets the incremental time value in milliseconds for calculating progressive delay between retry attempts.
        /// </summary>
        [ConfigurationProperty(RetryIncrementProperty, IsRequired = false)]
        public int RetryIncrement
        {
            get { return Convert.ToInt32(base[RetryIncrementProperty]); }
            set { base[RetryIncrementProperty] = value; }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns an instance of the <see cref="RetryPolicy"/> object based on the parameters in the current policy definition object.
        /// </summary>
        /// <typeparam name="T">The type implementing the <see cref="ITransientErrorDetectionStrategy"/> interface which is responsible for detecting transient conditions.</typeparam>
        /// <returns>The retry policy initialized from the current policy definition, or a null reference if method was unable to detect the type of retry method.</returns>
        public RetryPolicy<T> CreatePolicy<T>() where T : ITransientErrorDetectionStrategy, new()
        {
            RetryPolicy<T> retryPolicy = null;

            // Now it's time to make a decision as to what retry policy is required. First, check for any back-off parameters.
            if (MinBackoff != 0 || MaxBackoff != 0 || DeltaBackoff != 0)
            {
                retryPolicy = new RetryPolicy<T>(MaxRetryCount, TimeSpan.FromMilliseconds(MinBackoff), TimeSpan.FromMilliseconds(MaxBackoff), TimeSpan.FromMilliseconds(DeltaBackoff));
            }
            // Check if RetryIncrement was specified - this indicates that progressive delay retry policy is requested.
            else if (RetryIncrement != 0)
            {
                retryPolicy = new RetryPolicy<T>(MaxRetryCount, TimeSpan.FromMilliseconds(RetryInterval), TimeSpan.FromMilliseconds(RetryIncrement));
            }
            // Assume it's a fixed interval retry policy.
            else
            {
                retryPolicy = new RetryPolicy<T>(MaxRetryCount, TimeSpan.FromMilliseconds(RetryInterval));
            }

            return retryPolicy;
        }
        #endregion
    }
}
