// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace Microsoft.EntityFrameworkCore.Design;

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
    /// <param name="message">The message that describes the error.</param>
    public OperationException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="OperationException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public OperationException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="OperationException" /> class from a serialized form.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context being used.</param>
    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public OperationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
