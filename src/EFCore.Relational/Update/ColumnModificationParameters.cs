// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         Parameters for creating a <see cref="ColumnModification" /> instance.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///     for more information and examples.
/// </remarks>
public readonly record struct ColumnModificationParameters
{
    /// <summary>
    ///     A delegate for generating parameter names for the update SQL.
    /// </summary>
    public Func<string>? GenerateParameterName { get; init; }

    /// <summary>
    ///     The original value of the property mapped to column.
    /// </summary>
    public object? OriginalValue { get; init; }

    /// <summary>
    ///     The current value of the property mapped to column.
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    ///     Indicates whether potentially sensitive data (e.g. database values) can be logged.
    /// </summary>
    public bool SensitiveLoggingEnabled { get; init; }

    /// <summary>
    ///     The <see cref="IUpdateEntry" /> that represents the entity that is being modified.
    /// </summary>
    public IUpdateEntry? Entry { get; init; }

    /// <summary>
    ///     The property that maps to the column.
    /// </summary>
    public IProperty? Property { get; init; }

    /// <summary>
    ///     The relational type mapping for the column.
    /// </summary>
    public RelationalTypeMapping? TypeMapping { get; init; }

    /// <summary>
    ///     A value indicating whether the column could contain a null value.
    /// </summary>
    public bool? IsNullable { get; init; }

    /// <summary>
    ///     Indicates whether a value must be read from the database for the column.
    /// </summary>
    public bool IsRead { get; init; }

    /// <summary>
    ///     Indicates whether a value must be written to the database for the column.
    /// </summary>
    public bool IsWrite { get; init; }

    /// <summary>
    ///     Indicates whether the column is used in the <c>WHERE</c> clause when updating.
    /// </summary>
    public bool IsCondition { get; init; }

    /// <summary>
    ///     Indicates whether the column is part of a primary or alternate key.
    /// </summary>
    public bool IsKey { get; init; }

    /// <summary>
    ///     The column.
    /// </summary>
    public IColumnBase? Column { get; init; }

    /// <summary>
    ///     The name of the column.
    /// </summary>
    public string ColumnName { get; init; }

    /// <summary>
    ///     The database type of the column.
    /// </summary>
    public string? ColumnType { get; init; }

    /// <summary>
    ///     In case of JSON column modification, the JSON path leading to the JSON element that needs to be updated.
    /// </summary>
    public string? JsonPath { get; init; }

    /// <summary>
    ///     Creates a new <see cref="ColumnModificationParameters" /> instance.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="originalValue">The original value of the property mapped to this column.</param>
    /// <param name="value">The current value of the property mapped to this column.</param>
    /// <param name="property">The property that maps to the column.</param>
    /// <param name="columnType">The database type of the column.</param>
    /// <param name="typeMapping">The relational type mapping to be used for the command parameter.</param>
    /// <param name="read">Indicates whether a value must be read from the database for the column.</param>
    /// <param name="write">Indicates whether a value must be written to the database for the column.</param>
    /// <param name="key">Indicates whether the column part of a primary or alternate key.</param>
    /// <param name="condition">Indicates whether the column is used in the <c>WHERE</c> clause when updating.</param>
    /// <param name="sensitiveLoggingEnabled">Indicates whether potentially sensitive data (e.g. database values) can be logged.</param>
    /// <param name="isNullable">A value indicating whether the value could be null.</param>
    public ColumnModificationParameters(
        string columnName,
        object? originalValue,
        object? value,
        IProperty? property,
        string? columnType,
        RelationalTypeMapping? typeMapping,
        bool read,
        bool write,
        bool key,
        bool condition,
        bool sensitiveLoggingEnabled,
        bool? isNullable = null)
    {
        Column = null;
        ColumnName = columnName;
        OriginalValue = originalValue;
        Value = value;
        Property = property;
        ColumnType = columnType;
        TypeMapping = typeMapping;
        IsRead = read;
        IsWrite = write;
        IsKey = key;
        IsCondition = condition;
        SensitiveLoggingEnabled = sensitiveLoggingEnabled;
        IsNullable = isNullable;

        GenerateParameterName = null;
        Entry = null;
        JsonPath = null;
    }

    /// <summary>
    ///     Creates a new <see cref="ColumnModificationParameters" /> instance.
    /// </summary>
    /// <param name="column">The column.</param>
    /// <param name="originalValue">The original value of the property mapped to this column.</param>
    /// <param name="value">The current value of the property mapped to this column.</param>
    /// <param name="property">The property that maps to the column.</param>
    /// <param name="typeMapping">The relational type mapping to be used for the command parameter.</param>
    /// <param name="read">Indicates whether a value must be read from the database for the column.</param>
    /// <param name="write">Indicates whether a value must be written to the database for the column.</param>
    /// <param name="key">Indicates whether the column part of a primary or alternate key.</param>
    /// <param name="condition">Indicates whether the column is used in the <c>WHERE</c> clause when updating.</param>
    /// <param name="sensitiveLoggingEnabled">Indicates whether potentially sensitive data (e.g. database values) can be logged.</param>
    /// <param name="isNullable">A value indicating whether the value could be null.</param>
    public ColumnModificationParameters(
        IColumn column,
        object? originalValue,
        object? value,
        IProperty? property,
        RelationalTypeMapping? typeMapping,
        bool read,
        bool write,
        bool key,
        bool condition,
        bool sensitiveLoggingEnabled,
        bool? isNullable = null)
    {
        Column = column;
        ColumnName = column.Name;
        OriginalValue = originalValue;
        Value = value;
        Property = property;
        ColumnType = column.StoreType;
        TypeMapping = typeMapping;
        IsRead = read;
        IsWrite = write;
        IsKey = key;
        IsCondition = condition;
        SensitiveLoggingEnabled = sensitiveLoggingEnabled;
        IsNullable = isNullable;

        GenerateParameterName = null;
        Entry = null;
        JsonPath = null;
    }

    /// <summary>
    ///     Creates a new <see cref="ColumnModificationParameters" /> instance.
    /// </summary>
    /// <param name="entry">The <see cref="IUpdateEntry" /> that represents the entity that is being modified.</param>
    /// <param name="property">The property that maps to the column.</param>
    /// <param name="column">The column to be modified.</param>
    /// <param name="generateParameterName">A delegate for generating parameter names for the update SQL.</param>
    /// <param name="typeMapping">The relational type mapping to be used for the command parameter.</param>
    /// <param name="valueIsRead">Indicates whether a value must be read from the database for the column.</param>
    /// <param name="valueIsWrite">Indicates whether a value must be written to the database for the column.</param>
    /// <param name="columnIsKey">Indicates whether the column part of a primary or alternate key.</param>
    /// <param name="columnIsCondition">Indicates whether the column is used in the <c>WHERE</c> clause when updating.</param>
    /// <param name="sensitiveLoggingEnabled">Indicates whether potentially sensitive data (e.g. database values) can be logged.</param>
    public ColumnModificationParameters(
        IUpdateEntry? entry,
        IProperty? property,
        IColumnBase column,
        Func<string> generateParameterName,
        RelationalTypeMapping typeMapping,
        bool valueIsRead,
        bool valueIsWrite,
        bool columnIsKey,
        bool columnIsCondition,
        bool sensitiveLoggingEnabled)
    {
        Column = column;
        ColumnName = column.Name;
        OriginalValue = null;
        Value = null;
        Property = property;
        ColumnType = column.StoreType;
        TypeMapping = typeMapping;
        IsRead = valueIsRead;
        IsWrite = valueIsWrite;
        IsKey = columnIsKey;
        IsCondition = columnIsCondition;
        SensitiveLoggingEnabled = sensitiveLoggingEnabled;
        IsNullable = column.IsNullable;

        GenerateParameterName = generateParameterName;
        Entry = entry;
        JsonPath = null;
    }

    /// <summary>
    ///     Creates a new <see cref="ColumnModificationParameters" /> instance specific for updating objects mapped to JSON column.
    /// </summary>
    /// <param name="columnName">The name of the JSON column.</param>
    /// <param name="value">The current value of the JSON element located at the given JSON path.</param>
    /// <param name="property">
    ///     In case of JSON column single scalar property modification, the scalar property that is being modified, null
    ///     otherwise.
    /// </param>
    /// <param name="columnType">The database type of the JSON column.</param>
    /// <param name="typeMapping">The relational type mapping to be used for the command parameter.</param>
    /// <param name="jsonPath">The JSON path leading to the JSON element that needs to be updated.</param>
    /// <param name="read">Indicates whether a value must be read from the database for the column.</param>
    /// <param name="write">Indicates whether a value must be written to the database for the column.</param>
    /// <param name="key">Indicates whether the column part of a primary or alternate key.</param>
    /// <param name="condition">Indicates whether the column is used in the <c>WHERE</c> clause when updating.</param>
    /// <param name="sensitiveLoggingEnabled">Indicates whether potentially sensitive data (e.g. database values) can be logged.</param>
    /// <param name="isNullable">A value indicating whether the value could be null.</param>
    public ColumnModificationParameters(
        string columnName,
        object? value,
        IProperty? property,
        string? columnType,
        RelationalTypeMapping? typeMapping,
        string jsonPath,
        bool read,
        bool write,
        bool key,
        bool condition,
        bool sensitiveLoggingEnabled,
        bool? isNullable = null)
    {
        Column = null;
        ColumnName = columnName;
        OriginalValue = null;
        Value = value;
        Property = property;
        ColumnType = columnType;
        TypeMapping = typeMapping;
        IsRead = read;
        IsWrite = write;
        IsKey = key;
        IsCondition = condition;
        SensitiveLoggingEnabled = sensitiveLoggingEnabled;
        IsNullable = isNullable;

        GenerateParameterName = null;
        Entry = null;
        JsonPath = jsonPath;
    }
}
