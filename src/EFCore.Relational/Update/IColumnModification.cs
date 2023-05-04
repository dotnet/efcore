// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         Represents an update, insert, or delete operation for a single column. <see cref="IReadOnlyModificationCommand" />
///         contain lists of <see cref="IColumnModification" />.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public interface IColumnModification
{
    /// <summary>
    ///     The <see cref="IUpdateEntry" /> that represents the entity that is being modified.
    /// </summary>
    public IUpdateEntry? Entry { get; }

    /// <summary>
    ///     The property that maps to the column.
    /// </summary>
    /// <remarks>
    ///     In case of JSON column single scalar property modification, the scalar property that is being modified.
    /// </remarks>
    public IProperty? Property { get; }

    /// <summary>
    ///     The column.
    /// </summary>
    public IColumnBase? Column { get; }

    /// <summary>
    ///     The relational type mapping for the column.
    /// </summary>
    public RelationalTypeMapping? TypeMapping { get; }

    /// <summary>
    ///     A value indicating whether the column could contain a null value.
    /// </summary>
    public bool? IsNullable { get; }

    /// <summary>
    ///     Indicates whether a value must be read from the database for the column.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    ///     Indicates whether a value must be written to the database for the column.
    /// </summary>
    public bool IsWrite { get; set; }

    /// <summary>
    ///     Indicates whether the column is used in the <c>WHERE</c> clause when updating.
    /// </summary>
    public bool IsCondition { get; set; }

    /// <summary>
    ///     Indicates whether the column is part of a primary or alternate key.
    /// </summary>
    public bool IsKey { get; set; }

    /// <summary>
    ///     Indicates whether the original value of the property must be passed as a parameter to the SQL.
    /// </summary>
    [MemberNotNullWhen(true, nameof(OriginalParameterName))]
    public bool UseOriginalValueParameter { get; }

    /// <summary>
    ///     Indicates whether the current value of the property must be passed as a parameter to the SQL.
    /// </summary>
    [MemberNotNullWhen(true, nameof(ParameterName))]
    public bool UseCurrentValueParameter { get; }

    /// <summary>
    ///     Indicates whether the original value of the property should be used.
    /// </summary>
    public bool UseOriginalValue { get; }

    /// <summary>
    ///     Indicates whether the current value of the property should be used.
    /// </summary>
    public bool UseCurrentValue { get; }

    /// <summary>
    ///     Indicates whether the value of the property must be passed as a parameter to the SQL as opposed to being inlined.
    /// </summary>
    public bool UseParameter { get; }

    /// <summary>
    ///     The parameter name to use for the current value parameter (<see cref="UseCurrentValueParameter" />), if needed.
    /// </summary>
    public string? ParameterName { get; }

    /// <summary>
    ///     The parameter name to use for the original value parameter (<see cref="UseOriginalValueParameter" />), if needed.
    /// </summary>
    public string? OriginalParameterName { get; }

    /// <summary>
    ///     The name of the column.
    /// </summary>
    public string ColumnName { get; }

    /// <summary>
    ///     The database type of the column.
    /// </summary>
    public string? ColumnType { get; }

    /// <summary>
    ///     The original value of the property mapped to this column.
    /// </summary>
    public object? OriginalValue { get; set; }

    /// <summary>
    ///     Gets or sets the current value of the property mapped to this column.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    ///     In case of JSON column modification, the JSON path leading to the JSON element that needs to be updated.
    /// </summary>
    public string? JsonPath { get; }

    /// <summary>
    ///     Adds a modification affecting the same database value.
    /// </summary>
    /// <param name="modification">The modification for the shared column.</param>
    public void AddSharedColumnModification(IColumnModification modification);

    /// <summary>
    ///     Resets parameter names, so they can be regenerated if the command needs to be re-added to a new batch.
    /// </summary>
    public void ResetParameterNames();
}
