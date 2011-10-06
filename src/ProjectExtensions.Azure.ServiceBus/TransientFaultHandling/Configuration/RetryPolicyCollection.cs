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
    /// Implements a collection of the <see cref="RetryPolicyInfo"/> configuration elements holding retry policy parameters.
    /// </summary>
    public sealed class RetryPolicyCollection : ConfigurationElementCollection
    {
        #region Public methods
        /// <summary>
        /// Returns a <see cref="RetryPolicyInfo"/> element from the collection by the specified index.
        /// </summary>
        /// <param name="idx">The item index.</param>
        /// <returns>The <see cref="RetryPolicyInfo"/> element at the specified index.</returns>
        public RetryPolicyInfo this[int idx]
        {
            get { return (RetryPolicyInfo)BaseGet(idx); }
        }

        /// <summary>
        /// Adds a <see cref="RetryPolicyInfo"/> element to the collection.
        /// </summary>
        /// <param name="element">A <see cref="RetryPolicyInfo"/> element to add.</param>
        public void Add(RetryPolicyInfo element)
        {
            BaseAdd(element);
        }

        /// <summary>
        ///  Returns a <see cref="RetryPolicyInfo"/> element with the specified name.
        /// </summary>
        /// <param name="name">The name of the element to return.</param>
        /// <returns>The <see cref="RetryPolicyInfo"/> element with the specified name; otherwise null.</returns>
        public RetryPolicyInfo Get(string name)
        {
            return (RetryPolicyInfo)BaseGet(name);
        }

        /// <summary>
        /// Determines whether or not a <see cref="RetryPolicyInfo"/> element with the specified name exists in the collection.
        /// </summary>
        /// <param name="name">The name of the element to find.</param>
        /// <returns>True if the element was found, otherwise false.</returns>
        public bool Contains(string name)
        {
            return BaseGet(name) != null;
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Creates a new instance of a configuration element which this section contains.
        /// </summary>
        /// <returns>An instance of the <see cref="RetryPolicyInfo"/> object.</returns>
        protected override ConfigurationElement CreateNewElement()
        {
            return new RetryPolicyInfo();
        }

        /// <summary>
        /// Gets the element key for a specified configuration element.
        /// </summary>
        /// <param name="element">The configuration element to return the key for.</param>
        /// <returns>An object that acts as the key for the specified configuration element.</returns>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RetryPolicyInfo)element).Name;
        }
        #endregion
    }
}
