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
    #endregion
    
    /// <summary>
    /// Provides a set of extension methods that supplement various .NET Framework classes with value-add functionality.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Verifies whether the specified exception object contains an inner exception of the specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the inner exception to look for.</typeparam>
        /// <param name="ex">The exception object to be inspected.</param>
        /// <returns>The instance of the inner exception of the specified type <typeparamref name="T"/> if found, otherwise null.</returns>
        public static Exception GetInnerException<T>(this Exception ex)
        {
            var innerEx = ex != null ? ex.InnerException : null;
            var exceptionType = typeof(T);

            while (innerEx != null)
            {
                if (exceptionType.IsAssignableFrom(innerEx.GetType()))
                {
                    return innerEx;
                }

                innerEx = innerEx.InnerException;
            }

            return null;
        }
    }
}
