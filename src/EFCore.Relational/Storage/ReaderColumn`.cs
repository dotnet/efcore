// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Storage;

/// <summary>
///     <para>
///         An expected column in the relational data reader.
///     </para>
///     <para>
///         This type is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public class ReaderColumn<T> : ReaderColumn
{
    /// <summary>
    ///     Creates a new instance of the <see cref="ReaderColumn{T}" /> class.
    /// </summary>
    /// <param name="nullable">A value indicating if the column is nullable.</param>
    /// <param name="name">The name of the column.</param>
    /// <param name="property">The property being read if any, null otherwise.</param>
    /// <param name="getFieldValue">A function to get field value for the column from the reader.</param>
    public ReaderColumn(
        bool nullable,
        string? name,
        IPropertyBase? property,
        Func<DbDataReader, int[], T> getFieldValue)
        : base(typeof(T), nullable, name, property)
    {
        GetFieldValue = getFieldValue;
    }

    /// <summary>
    ///     The function to get field value for the column from the reader.
    /// </summary>
    public virtual Func<DbDataReader, int[], T> GetFieldValue { get; }
}
