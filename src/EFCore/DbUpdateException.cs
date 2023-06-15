// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.Serialization;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
///     An exception that is thrown when an error is encountered while saving to the database.
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-saving-data">Saving data with EF Core</see> for more information and examples.
/// </remarks>
[Serializable]
public class DbUpdateException : Exception
{
    [NonSerialized]
    private IReadOnlyList<EntityEntry>? _entries;

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbUpdateException" /> class.
    /// </summary>
    public DbUpdateException()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbUpdateException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public DbUpdateException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbUpdateException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public DbUpdateException(string message, Exception? innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbUpdateException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="entries">The entries that were involved in the error.</param>
    public DbUpdateException(
        string message,
        IReadOnlyList<IUpdateEntry> entries)
        : this(message, null, entries)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbUpdateException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="entries">The entries that were involved in the error.</param>
    public DbUpdateException(
        string message,
        Exception? innerException,
        IReadOnlyList<IUpdateEntry> entries)
        : base(message, innerException)
    {
        _entries = entries
            .Where(e => e.EntityState != EntityState.Unchanged)
            .Select(e => e.ToEntityEntry()).ToList();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbUpdateException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="entries">The entries that were involved in the error.</param>
    public DbUpdateException(
        string message,
        IReadOnlyList<EntityEntry> entries)
        : this(message, null, entries)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbUpdateException" /> class.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    /// <param name="entries">The entries that were involved in the error.</param>
    public DbUpdateException(
        string message,
        Exception? innerException,
        IReadOnlyList<EntityEntry> entries)
        : base(message, innerException)
    {
        _entries = entries;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="DbUpdateException" /> class from a serialized form.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context being used.</param>
    [Obsolete("This API supports obsolete formatter-based serialization. It should not be called or extended by application code.")]
    public DbUpdateException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    /// <summary>
    ///     Gets the entries that were involved in the error. Typically this is a single entry, but in some cases it
    ///     may be zero or multiple entries.
    /// </summary>
    public virtual IReadOnlyList<EntityEntry> Entries
        => _entries ??= new List<EntityEntry>();
}
