// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     The exception that is thrown when the action failed more times than the configured limit.
    /// </summary>
    public class RetryLimitExceededException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="RetryLimitExceededException" /> class with a specified error message.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public RetryLimitExceededException([NotNull] string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RetryLimitExceededException" /> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public RetryLimitExceededException([NotNull] string message, [NotNull] Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
