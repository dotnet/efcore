// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

/// <summary>
///     <para>
///         Implementation of <see cref="IColumnModification" /> interface.
///     </para>
///     <para>
///         This type is typically used by database providers; it is generally not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         Represents an update, insert, or delete operation for a single column. <see cref="IReadOnlyModificationCommand" />
///         contain lists of <see cref="IColumnModification" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public class ColumnModification : IColumnModification
{
    private string? _parameterName;
    private string? _originalParameterName;
    private readonly Func<string>? _generateParameterName;
    private readonly object? _originalValue;
    private object? _value;
    private readonly bool _sensitiveLoggingEnabled;
    private List<IColumnModification>? _sharedColumnModifications;

    /// <summary>
    ///     Creates a new <see cref="ColumnModification" /> instance.
    /// </summary>
    /// <param name="columnModificationParameters">Creation parameters.</param>
    public ColumnModification(in ColumnModificationParameters columnModificationParameters)
    {
        ColumnName = columnModificationParameters.ColumnName;
        _originalValue = columnModificationParameters.OriginalValue;
        _value = columnModificationParameters.Value;
        Property = columnModificationParameters.Property;
        ColumnType = columnModificationParameters.ColumnType;
        TypeMapping = columnModificationParameters.TypeMapping;
        IsRead = columnModificationParameters.IsRead;
        IsWrite = columnModificationParameters.IsWrite;
        IsKey = columnModificationParameters.IsKey;
        IsCondition = columnModificationParameters.IsCondition;
        _sensitiveLoggingEnabled = columnModificationParameters.SensitiveLoggingEnabled;
        IsNullable = columnModificationParameters.IsNullable;
        _generateParameterName = columnModificationParameters.GenerateParameterName;
        Entry = columnModificationParameters.Entry;

        UseParameter = _generateParameterName != null;
    }

    /// <summary>
    ///     The <see cref="IUpdateEntry" /> that represents the entity that is being modified.
    /// </summary>
    public virtual IUpdateEntry? Entry { get; }

    /// <summary>
    ///     The property that maps to the column.
    /// </summary>
    public virtual IProperty? Property { get; }

    /// <summary>
    ///     The relational type mapping for the column.
    /// </summary>
    public virtual RelationalTypeMapping? TypeMapping { get; }

    /// <summary>
    ///     A value indicating whether the column could contain a null value.
    /// </summary>
    public virtual bool? IsNullable { get; }

    /// <summary>
    ///     Indicates whether a value must be read from the database for the column.
    /// </summary>
    public virtual bool IsRead { get; }

    /// <summary>
    ///     Indicates whether a value must be written to the database for the column.
    /// </summary>
    public virtual bool IsWrite { get; }

    /// <summary>
    ///     Indicates whether the column is used in the <c>WHERE</c> clause when updating.
    /// </summary>
    public virtual bool IsCondition { get; }

    /// <summary>
    ///     Indicates whether the column is part of a primary or alternate key.
    /// </summary>
    public virtual bool IsKey { get; }

    /// <summary>
    ///     Indicates whether the original value of the property must be passed as a parameter to the SQL.
    /// </summary>
    public virtual bool UseOriginalValueParameter
        => UseParameter && UseOriginalValue;

    /// <summary>
    ///     Indicates whether the current value of the property must be passed as a parameter to the SQL.
    /// </summary>
    public virtual bool UseCurrentValueParameter
        => UseParameter && UseCurrentValue;

    /// <summary>
    ///     Indicates whether the original value of the property should be used.
    /// </summary>
    public virtual bool UseOriginalValue
        => IsCondition;

    /// <summary>
    ///     Indicates whether the current value of the property should be used.
    /// </summary>
    public virtual bool UseCurrentValue
        => IsWrite;

    /// <summary>
    ///     Indicates whether the value of the property must be passed as a parameter to the SQL as opposed to being inlined.
    /// </summary>
    public virtual bool UseParameter { get; }

    /// <summary>
    ///     The parameter name to use for the current value parameter (<see cref="UseCurrentValueParameter" />), if needed.
    /// </summary>
    public virtual string? ParameterName
        => _parameterName ??= UseCurrentValueParameter ? _generateParameterName!() : null;

    /// <summary>
    ///     The parameter name to use for the original value parameter (<see cref="UseOriginalValueParameter" />), if needed.
    /// </summary>
    public virtual string? OriginalParameterName
        => _originalParameterName ??= UseOriginalValueParameter ? _generateParameterName!() : null;

    /// <summary>
    ///     The name of the column.
    /// </summary>
    public virtual string ColumnName { get; }

    /// <summary>
    ///     The database type of the column.
    /// </summary>
    public virtual string? ColumnType { get; }

    /// <summary>
    ///     The original value of the property mapped to this column.
    /// </summary>
    public virtual object? OriginalValue
        => Entry == null
            ? _originalValue
            : Entry.SharedIdentityEntry == null
                ? Entry.GetOriginalValue(Property!)
                : Entry.SharedIdentityEntry.GetOriginalValue(Property!);

    /// <summary>
    ///     Gets or sets the current value of the property mapped to this column.
    /// </summary>
    public virtual object? Value
    {
        get => Entry == null
            ? _value
            : Entry.EntityState == EntityState.Deleted
                ? null
                : Entry.GetCurrentValue(Property!);
        set
        {
            if (Entry == null)
            {
                _value = value;
            }
            else
            {
                Entry.SetStoreGeneratedValue(Property!, value);
                if (_sharedColumnModifications != null)
                {
                    foreach (var sharedModification in _sharedColumnModifications)
                    {
                        sharedModification.Value = value;
                    }
                }
            }
        }
    }

    /// <summary>
    ///     Adds a modification affecting the same database value.
    /// </summary>
    /// <param name="modification">The modification for the shared column.</param>
    public virtual void AddSharedColumnModification(IColumnModification modification)
    {
        Check.DebugAssert(Entry is not null, "Entry is not null");
        Check.DebugAssert(Property is not null, "Property is not null");
        Check.DebugAssert(modification.Entry is not null, "modification.Entry is not null");
        Check.DebugAssert(modification.Property is not null, "modification.Property is not null");

        _sharedColumnModifications ??= new List<IColumnModification>();

        if (UseCurrentValueParameter
            && !modification.Property.GetValueComparer().Equals(Value, modification.Value))
        {
            if (_sensitiveLoggingEnabled)
            {
                throw new InvalidOperationException(
                    RelationalStrings.ConflictingRowValuesSensitive(
                        Entry.EntityType.DisplayName(),
                        modification.Entry!.EntityType.DisplayName(),
                        Entry.BuildCurrentValuesString(Entry.EntityType.FindPrimaryKey()!.Properties),
                        Entry.BuildCurrentValuesString(new[] { Property }),
                        modification.Entry.BuildCurrentValuesString(new[] { modification.Property }),
                        ColumnName));
            }

            throw new InvalidOperationException(
                RelationalStrings.ConflictingRowValues(
                    Entry.EntityType.DisplayName(),
                    modification.Entry.EntityType.DisplayName(),
                    new[] { Property }.Format(),
                    new[] { modification.Property }.Format(),
                    ColumnName));
        }

        if (UseOriginalValueParameter
            && !modification.Property.GetValueComparer().Equals(OriginalValue, modification.OriginalValue))
        {
            if (Entry.EntityState == EntityState.Modified
                && modification.Entry.EntityState == EntityState.Added
                && modification.Entry.SharedIdentityEntry == null)
            {
                modification.Entry.SetOriginalValue(modification.Property, OriginalValue);
            }
            else
            {
                if (_sensitiveLoggingEnabled)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingOriginalRowValuesSensitive(
                            Entry.EntityType.DisplayName(),
                            modification.Entry.EntityType.DisplayName(),
                            Entry.BuildCurrentValuesString(Entry.EntityType.FindPrimaryKey()!.Properties),
                            Entry.BuildOriginalValuesString(new[] { Property }),
                            modification.Entry.BuildOriginalValuesString(new[] { modification.Property }),
                            ColumnName));
                }

                throw new InvalidOperationException(
                    RelationalStrings.ConflictingOriginalRowValues(
                        Entry.EntityType.DisplayName(),
                        modification.Entry.EntityType.DisplayName(),
                        new[] { Property }.Format(),
                        new[] { modification.Property }.Format(),
                        ColumnName));
            }
        }

        _sharedColumnModifications.Add(modification);
    }
}
