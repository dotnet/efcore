// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Runtime.Serialization;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     The exception that is thrown when the action failed more times than the configured limit.
    /// </summary>
    [Serializable]
    public class RetryLimitExceededException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RetryLimitExceededException" /> class.
        /// </summary>
        public RetryLimitExceededException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RetryLimitExceededException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RetryLimitExceededException(string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RetryLimitExceededException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public RetryLimitExceededException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbUpdateException" /> class from a serialized form.
        /// </summary>
        /// <param name="info"> The serialization info. </param>
        /// <param name="context"> The streaming context being used. </param>
        public RetryLimitExceededException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
