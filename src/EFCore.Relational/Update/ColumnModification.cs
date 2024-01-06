// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

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
        => (UseParameter && UseCurrentValue)
            || (Column is IStoreStoredProcedureParameter { Direction: ParameterDirection.Output or ParameterDirection.InputOutput }
                or IStoreStoredProcedureReturnValue);

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
                ? GetOriginalValue(Entry, Property!)
                : GetOriginalValue(Entry.SharedIdentityEntry, Property!);
        set
        {
            if (Entry == null)
            {
                _originalValue = value;
            }
            else
            {
                SetOriginalValue(value);
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
                : GetCurrentValue(Entry, Property!);
        set
        {
            if (Entry == null)
            {
                _value = value;
            }
            else
            {
                SetStoreGeneratedValue(Entry, Property!, value);
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
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static object? GetOriginalValue(IUpdateEntry entry, IProperty property)
        => entry.GetOriginalOrCurrentValue(property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static object? GetOriginalProviderValue(IUpdateEntry entry, IProperty property)
        => entry.GetOriginalProviderValue(property);

    private void SetOriginalValue(object? value)
        => Entry!.SetOriginalValue(Property!, value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static object? GetCurrentValue(IUpdateEntry entry, IProperty property)
        => entry.GetCurrentValue(property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static object? GetCurrentProviderValue(IUpdateEntry entry, IProperty property)
        => entry.GetCurrentProviderValue(property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static void SetStoreGeneratedValue(IUpdateEntry entry, IProperty property, object? value)
        => entry.SetStoreGeneratedValue(property, value);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsModified(IUpdateEntry entry, IProperty property)
        => entry.IsModified(property);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static bool IsStoreGenerated(IUpdateEntry entry, IProperty property)
        => entry.IsStoreGenerated(property);

    /// <inheritdoc />
    public virtual string? JsonPath { get; }

    /// <inheritdoc />
    public virtual void AddSharedColumnModification(IColumnModification modification)
    {
        Check.DebugAssert(Entry is not null, "Entry is not null");
        Check.DebugAssert(Property is not null, "Property is not null");
        Check.DebugAssert(modification.Entry is not null, "modification.Entry is not null");
        Check.DebugAssert(modification.Property is not null, "modification.Property is not null");

        _sharedColumnModifications ??= [];

        if (UseCurrentValueParameter
            && !Property.GetProviderValueComparer().Equals(
                GetCurrentProviderValue(Entry, Property),
                GetCurrentProviderValue(modification.Entry, modification.Property)))
        {
            var existingEntry = Entry;
            var newEntry = modification.Entry;

            if (_sensitiveLoggingEnabled)
            {
                throw new InvalidOperationException(
                    RelationalStrings.ConflictingRowValuesSensitive(
                        existingEntry.EntityType.DisplayName(),
                        newEntry.EntityType.DisplayName(),
                        Entry.BuildCurrentValuesString(Entry.EntityType.FindPrimaryKey()!.Properties),
                        Entry.BuildCurrentValuesString(new[] { Property }),
                        newEntry.BuildCurrentValuesString(new[] { modification.Property }),
                        ColumnName));
            }

            throw new InvalidOperationException(
                RelationalStrings.ConflictingRowValues(
                    existingEntry.EntityType.DisplayName(),
                    newEntry.EntityType.DisplayName(),
                    new[] { Property }.Format(),
                    new[] { modification.Property }.Format(),
                    ColumnName));
        }

        if (UseOriginalValueParameter)
        {
            var originalValue = Entry.SharedIdentityEntry == null
                ? GetOriginalProviderValue(Entry, Property)
                : GetOriginalProviderValue(Entry.SharedIdentityEntry, Property);
            if (Property.GetProviderValueComparer().Equals(
                    originalValue,
                    modification.Entry.SharedIdentityEntry == null
                        ? GetOriginalProviderValue(modification.Entry, modification.Property)
                        : GetOriginalProviderValue(modification.Entry.SharedIdentityEntry, modification.Property)))
            {
                _sharedColumnModifications.Add(modification);
                return;
            }

            if (Entry.EntityState == EntityState.Modified
                && modification.Entry.EntityState == EntityState.Added
                && modification.Entry.SharedIdentityEntry == null)
            {
                var typeMapping = modification.Property.GetTypeMapping();
                var converter = typeMapping.Converter;
                if (converter != null)
                {
                    originalValue = converter.ConvertFromProvider(originalValue);
                }

                modification.Entry.SetOriginalValue(modification.Property, originalValue);
            }
            else
            {
                var existingEntry = Entry;
                var newEntry = modification.Entry;
                if (_sensitiveLoggingEnabled)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.ConflictingOriginalRowValuesSensitive(
                            existingEntry.EntityType.DisplayName(),
                            newEntry.EntityType.DisplayName(),
                            Entry.BuildCurrentValuesString(Entry.EntityType.FindPrimaryKey()!.Properties),
                            existingEntry.BuildOriginalValuesString(new[] { Property }),
                            newEntry.BuildOriginalValuesString(new[] { modification.Property }),
                            ColumnName));
                }

                throw new InvalidOperationException(
                    RelationalStrings.ConflictingOriginalRowValues(
                        existingEntry.EntityType.DisplayName(),
                        newEntry.EntityType.DisplayName(),
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
