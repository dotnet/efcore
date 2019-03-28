// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Design
{
    /// <summary>
    ///     Represents an exception whose stack trace should, by default, not be reported by the commands.
    /// </summary>
    [Serializable]
    public class OperationException : Exception
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="OperationException" /> class.
        /// </summary>
        public OperationException()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperationException" /> class.
        /// </summary>
        /// <param name="message"> The message that describes the error. </param>
        public OperationException([NotNull] string message)
            : base(message)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OperationException" /> class.
        /// </summary>
        /// <param name="message"> The message that describes the error. </param>
        /// <param name="innerException"> The exception that is the cause of the current exception. </param>
        public OperationException([NotNull] string message, [CanBeNull] Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbUpdateException" /> class from a serialized form.
        /// </summary>
        /// <param name="info"> The serialization info. </param>
        /// <param name="context"> The streaming context being used. </param>
        public OperationException([NotNull] SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
