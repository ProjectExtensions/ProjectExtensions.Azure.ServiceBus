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
    using System.Collections.Generic;
    using System.Configuration;
    #endregion

    /// <summary>
    /// Helper class that exposes all strongly-typed configuration sections and also provides an ability to save the
    /// configuration changes for custom sections.
    /// </summary>
    public sealed class ApplicationConfiguration
    {
        #region Private members
        /// <summary>
        /// A pre-initialized instance of the current configuration.
        /// </summary>
        private static volatile ApplicationConfiguration currentConfiguration;

        /// <summary>
        /// A lock object.
        /// </summary>
        private static readonly object initLock = new object();

        /// <summary>
        /// A dictionary object containing cached instances of the configuration sections.
        /// </summary>
        private readonly IDictionary<string, ConfigurationSection> configSectionCache;

        /// <summary>
        /// A lock object for the configuration section cache.
        /// </summary>
        private readonly object configSectionCacheLock = new object();
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ApplicationConfiguration class using default configuration source.
        /// </summary>
        private ApplicationConfiguration()
        {
            this.configSectionCache = new Dictionary<string, ConfigurationSection>(16);
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Returns an instance of the ApplicationConfiguration class by enforcing a singleton design pattern with a lazy initialization.
        /// </summary>
        public static ApplicationConfiguration Current
        {
            get
            {
                if (null == currentConfiguration)
                {
                    lock (initLock)
                    {
                        // If we were the second process attempting to initialize the currentConfiguration member, when we enter
                        // this critical section let's check once again if the member was not already initialized by the another
                        // process. As the lock will ensure a serialized execution, we need to make sure that we don't attempt
                        // to re-initialize the ready-to-go instance.
                        if (null == currentConfiguration)
                        {
                            currentConfiguration = new ApplicationConfiguration();
                        }
                    }
                }

                return currentConfiguration;
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns a configuration section defined by the given type <typeparamref name="T"/> and name that corresponds to the type's fully qualified name.
        /// </summary>
        /// <typeparam name="T">The type of the configuration section.</typeparam>
        /// <returns>An instance of the type <typeparamref name="T"/> containing the configuration section or a null reference if configuration section was not found.</returns>
        public T GetConfigurationSection<T>() where T : ConfigurationSection
        {
            return GetConfigurationSection<T>(typeof(T).FullName);
        }

        /// <summary>
        /// Returns a configuration section defined by the given type <typeparamref name="T"/> and specified section name.
        /// </summary>
        /// <typeparam name="T">The type of the configuration section.</typeparam>
        /// <param name="sectionName">The name of the configuration section.</param>
        /// <returns>An instance of the type <typeparamref name="T"/> containing the configuration section or a null reference if configuration section was not found.</returns>
        public T GetConfigurationSection<T>(string sectionName) where T: ConfigurationSection
        {
            ConfigurationSection configSection = null;

            if (!this.configSectionCache.TryGetValue(sectionName, out configSection))
            {
                lock (this.configSectionCacheLock)
                {
                    if (!this.configSectionCache.TryGetValue(sectionName, out configSection))
                    {
                        configSection = ConfigurationManager.GetSection(sectionName) as ConfigurationSection;

                        if (configSection != null)
                        {
                            this.configSectionCache.Add(sectionName, configSection);
                        }
                    }
                }
            }

            return configSection as T;
        }

        /// <summary>
        /// Signals the configuration manager to unload all currently loaded configuration sections by removing them from in-memory cache.
        /// </summary>
        public void Unload()
        {
            lock (this.configSectionCacheLock)
            {
                this.configSectionCache.Clear();
            }
        }
        #endregion
    }
}
