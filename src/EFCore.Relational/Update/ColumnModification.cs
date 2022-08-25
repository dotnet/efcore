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
    private object? _originalValue;
    private object? _value;
    private readonly bool _sensitiveLoggingEnabled;
    private List<IColumnModification>? _sharedColumnModifications;

    /// <summary>
    ///     Creates a new <see cref="ColumnModification" /> instance.
    /// </summary>
    /// <param name="columnModificationParameters">Creation parameters.</param>
    public ColumnModification(in ColumnModificationParameters columnModificationParameters)
    {
        Column = columnModificationParameters.Column;
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
        JsonPath = columnModificationParameters.JsonPath;

        UseParameter = _generateParameterName != null;
    }

    /// <inheritdoc />
    public virtual IUpdateEntry? Entry { get; }

    /// <inheritdoc />
    public virtual IProperty? Property { get; }

    /// <inheritdoc />
    public virtual IColumnBase? Column { get; }

    /// <inheritdoc />
    public virtual RelationalTypeMapping? TypeMapping { get; }

    /// <inheritdoc />
    public virtual bool? IsNullable { get; }

    /// <inheritdoc />
    public virtual bool IsRead { get; set; }

    /// <inheritdoc />
    public virtual bool IsWrite { get; set; }

    /// <inheritdoc />
    public virtual bool IsCondition { get; set; }

    /// <inheritdoc />
    public virtual bool IsKey { get; set; }

    /// <inheritdoc />
    public virtual bool UseOriginalValueParameter
        => UseParameter && UseOriginalValue;

    /// <inheritdoc />
    public virtual bool UseCurrentValueParameter
        => (UseParameter && UseCurrentValue) || (IsRead && Column is IStoreStoredProcedureParameter or IStoreStoredProcedureReturnValue);

    /// <inheritdoc />
    public virtual bool UseOriginalValue
        => IsCondition;

    /// <inheritdoc />
    public virtual bool UseCurrentValue
        => IsWrite;

    /// <inheritdoc />
    public virtual bool UseParameter { get; }

    /// <inheritdoc />
    public virtual string? ParameterName
        => _parameterName ??= UseCurrentValueParameter ? _generateParameterName!() : null;

    /// <inheritdoc />
    public virtual string? OriginalParameterName
        => _originalParameterName ??= UseOriginalValueParameter ? _generateParameterName!() : null;

    /// <inheritdoc />
    public virtual string ColumnName { get; }

    /// <inheritdoc />
    public virtual string? ColumnType { get; }

    /// <inheritdoc />
    public virtual object? OriginalValue
    {
        get => Entry == null
            ? _originalValue
            : Entry.SharedIdentityEntry == null
                ? Entry.GetOriginalValue(Property!)
                : Entry.SharedIdentityEntry.GetOriginalValue(Property!);
        set
        {
            if (Entry == null)
            {
                _originalValue = value;
            }
            else
            {
                Entry.SetOriginalValue(Property!, value);
                if (_sharedColumnModifications != null)
                {
                    foreach (var sharedModification in _sharedColumnModifications)
                    {
                        sharedModification.OriginalValue = value;
                    }
                }
            }
        }
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual string? JsonPath { get; }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public virtual void ResetParameterNames()
        => _parameterName = _originalParameterName = null;
}
