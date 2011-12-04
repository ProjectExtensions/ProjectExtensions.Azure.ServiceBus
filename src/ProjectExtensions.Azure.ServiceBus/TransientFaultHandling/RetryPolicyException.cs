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
    using System.Runtime.Serialization;

    /// <summary>
    /// Represents an error that occurs while using the Transient Fault Handling Application Block.
    /// </summary>
    [Serializable]
    public class RetryPolicyException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicyException"/> class.
        /// </summary>
        public RetryPolicyException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicyException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RetryPolicyException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicyException"/> class with a specified error message 
        /// and a reference to the inner exception that is the cause of this exception. 
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The inner exception reference.</param>
        public RetryPolicyException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryPolicyException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information about the source or destination.</param>
        protected RetryPolicyException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}
