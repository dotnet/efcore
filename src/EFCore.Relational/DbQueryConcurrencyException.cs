// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     An exception that is thrown when a query which loads related collections using
///     <see cref="QuerySplittingBehavior.SplitQuery" /> cannot correlate the parent and child rows because the data was
///     modified concurrently while the query was executing.
/// </summary>
/// <remarks>
///     <para>
///         Split queries execute multiple SQL statements, and the results can become inconsistent when the underlying data is
///         modified between those statements. When Entity Framework detects that it can no longer reliably correlate the parent
///         and child rows, it throws this exception instead of returning incorrect results.
///     </para>
///     <para>
///         The recommended way to handle this exception is to catch it and execute the query again, ideally within a
///         serializable or snapshot transaction to prevent the concurrent modification from recurring.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-split-queries">Split queries</see> for more information and examples.
///     </para>
/// </remarks>
[Serializable]
public class DbQueryConcurrencyException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DbQueryConcurrencyException" /> class.
    /// </summary>
    public DbQueryConcurrencyException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbQueryConcurrencyException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public DbQueryConcurrencyException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbQueryConcurrencyException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DbQueryConcurrencyException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbQueryConcurrencyException" /> class from a serialized form.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context being used.</param>
    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    protected DbQueryConcurrencyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
