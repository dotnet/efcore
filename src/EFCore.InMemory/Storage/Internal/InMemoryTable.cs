// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Globalization;
using Microsoft.EntityFrameworkCore.InMemory.Internal;
using Microsoft.EntityFrameworkCore.InMemory.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class InMemoryTable<TKey> : IInMemoryTable
    where TKey : notnull
{
    private readonly IPrincipalKeyValueFactory<TKey> _keyValueFactory;
    private readonly bool _sensitiveLoggingEnabled;
    private readonly bool _nullabilityCheckEnabled;
    private readonly Dictionary<TKey, object?[]> _rows;
    private readonly IList<(int, ValueConverter)>? _valueConverters;
    private readonly IList<(int, ValueComparer)>? _valueComparers;
    private readonly int _propertyCount;

    private Dictionary<int, IInMemoryIntegerValueGenerator>? _integerGenerators;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public InMemoryTable(
        IEntityType entityType,
        IInMemoryTable? baseTable,
        bool sensitiveLoggingEnabled,
        bool nullabilityCheckEnabled)
    {
        this.BaseTable = baseTable;
        this._keyValueFactory = entityType.FindPrimaryKey()!.GetPrincipalKeyValueFactory<TKey>();
        this._sensitiveLoggingEnabled = sensitiveLoggingEnabled;
        this._nullabilityCheckEnabled = nullabilityCheckEnabled;
        this._rows = new Dictionary<TKey, object?[]>(this._keyValueFactory.EqualityComparer);
        var properties = entityType.GetFlattenedProperties().ToList();
        this._propertyCount = properties.Count;

        foreach (var property in properties)
        {
            var converter = property.GetValueConverter()
                ?? property.FindTypeMapping()?.Converter;

            if (converter != null)
            {
                this._valueConverters ??= new List<(int, ValueConverter)>();
                this._valueConverters.Add((property.GetIndex(), converter));
            }

            var comparer = property.GetKeyValueComparer();
            if (!comparer.IsDefault())
            {
                this._valueComparers ??= new List<(int, ValueComparer)>();
                this._valueComparers.Add((property.GetIndex(), comparer));
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IInMemoryTable? BaseTable { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual InMemoryIntegerValueGenerator<TProperty> GetIntegerValueGenerator<TProperty>(
        IProperty property,
        IReadOnlyList<IInMemoryTable> tables)
    {
        this._integerGenerators ??= new Dictionary<int, IInMemoryIntegerValueGenerator>();

        var propertyIndex = property.GetIndex();
        if (!this._integerGenerators.TryGetValue(propertyIndex, out var generator))
        {
            generator = new InMemoryIntegerValueGenerator<TProperty>(propertyIndex);
            this._integerGenerators[propertyIndex] = generator;

            foreach (var table in tables)
            {
                foreach (var row in table.Rows)
                {
                    generator.Bump(row);
                }
            }
        }

        return (InMemoryIntegerValueGenerator<TProperty>)generator;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEnumerable<object?[]> Rows
        => this._rows.Values;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IReadOnlyList<object?[]> SnapshotRows()
    {
        var rows = this._rows.Values.ToList();
        var rowCount = rows.Count;

        for (var rowIndex = 0; rowIndex < rowCount; rowIndex++)
        {
            var snapshotRow = new object?[this._propertyCount];
            Array.Copy(rows[rowIndex], snapshotRow, this._propertyCount);

            if (this._valueConverters != null)
            {
                foreach (var (index, converter) in this._valueConverters)
                {
                    snapshotRow[index] = converter.ConvertFromProvider(snapshotRow[index]);
                }
            }

            if (this._valueComparers != null)
            {
                foreach (var (index, comparer) in this._valueComparers)
                {
                    snapshotRow[index] = comparer.Snapshot(snapshotRow[index]);
                }
            }

            rows[rowIndex] = snapshotRow;
        }

        return rows;
    }

    private static List<ValueComparer> GetKeyComparers(IEnumerable<IProperty> properties)
        => properties.Select(p => p.GetKeyValueComparer()).ToList();

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Create(IUpdateEntry entry, IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger)
    {
        var properties = entry.EntityType.GetFlattenedProperties().ToList();
        var row = new object?[properties.Count];
        var nullabilityErrors = new List<IProperty>();

        for (var index = 0; index < properties.Count; index++)
        {
            var propertyValue = SnapshotValue(properties[index], properties[index].GetKeyValueComparer(), entry);

            row[properties[index].GetIndex()] = propertyValue;
            this.HasNullabilityError(properties[index], propertyValue, nullabilityErrors);
        }

        if (nullabilityErrors.Count > 0)
        {
            this.ThrowNullabilityErrorException(entry, nullabilityErrors);
        }

        this._rows.Add(this.CreateKey(entry), row);

        this.BumpValueGenerators(row);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Delete(IUpdateEntry entry, IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger)
    {
        var key = this.CreateKey(entry);

        if (this._rows.TryGetValue(key, out var row))
        {
            var properties = entry.EntityType.GetFlattenedProperties().ToList();
            var concurrencyConflicts = new Dictionary<IProperty, object?>();

            for (var index = 0; index < properties.Count; index++)
            {
                IsConcurrencyConflict(entry, properties[index], row[properties[index].GetIndex()], concurrencyConflicts);
            }

            if (concurrencyConflicts.Count > 0)
            {
                this.ThrowUpdateConcurrencyException(entry, concurrencyConflicts, updateLogger);
            }

            this._rows.Remove(key);
        }
        else
        {
            var entries = new[] { entry };
            var exception = new DbUpdateConcurrencyException(InMemoryStrings.UpdateConcurrencyException, entries);
            if (!updateLogger.OptimisticConcurrencyException(entry.Context, entries, exception, null).IsSuppressed)
            {
                throw exception;
            }
        }
    }

    private static bool IsConcurrencyConflict(
        IUpdateEntry entry,
        IProperty property,
        object? rowValue,
        Dictionary<IProperty, object?> concurrencyConflicts)
    {
        if (!property.IsConcurrencyToken)
        {
            return false;
        }

        var comparer = property.GetKeyValueComparer()
            ?? StructuralComparisons.StructuralEqualityComparer;

        var originalValue = entry.GetOriginalValue(property);

        var converter = property.GetValueConverter()
            ?? property.FindTypeMapping()?.Converter;

        if (converter != null)
        {
            rowValue = converter.ConvertFromProvider(rowValue);
        }

        if (comparer.Equals(rowValue, originalValue))
        {
            return false;
        }

        concurrencyConflicts.Add(property, rowValue);

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void Update(IUpdateEntry entry, IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger)
    {
        var key = this.CreateKey(entry);

        if (this._rows.TryGetValue(key, out var row))
        {
            var properties = entry.EntityType.GetFlattenedProperties().ToList();
            var comparers = GetKeyComparers(properties);
            var valueBuffer = new object?[properties.Count];
            var concurrencyConflicts = new Dictionary<IProperty, object?>();
            var nullabilityErrors = new List<IProperty>();

            for (var index = 0; index < valueBuffer.Length; index++)
            {
                var propertyIndex = properties[index].GetIndex();
                if (IsConcurrencyConflict(entry, properties[index], row[propertyIndex], concurrencyConflicts))
                {
                    continue;
                }

                if (this.HasNullabilityError(properties[index], row[propertyIndex], nullabilityErrors))
                {
                    continue;
                }

                valueBuffer[index] = entry.IsModified(properties[index])
                    ? SnapshotValue(properties[index], comparers[index], entry)
                    : row[propertyIndex];
            }

            if (concurrencyConflicts.Count > 0)
            {
                this.ThrowUpdateConcurrencyException(entry, concurrencyConflicts, updateLogger);
            }

            if (nullabilityErrors.Count > 0)
            {
                this.ThrowNullabilityErrorException(entry, nullabilityErrors);
            }

            this._rows[key] = valueBuffer;

            this.BumpValueGenerators(valueBuffer);
        }
        else
        {
            var entries = new[] { entry };
            var exception = new DbUpdateConcurrencyException(InMemoryStrings.UpdateConcurrencyException, entries);
            if (!updateLogger.OptimisticConcurrencyException(entry.Context, entries, exception, null).IsSuppressed)
            {
                throw exception;
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual void BumpValueGenerators(object?[] row)
    {
        this.BaseTable?.BumpValueGenerators(row);

        if (this._integerGenerators != null)
        {
            foreach (var generator in this._integerGenerators.Values)
            {
                generator.Bump(row);
            }
        }
    }

    private TKey CreateKey(IUpdateEntry entry)
        => this._keyValueFactory.CreateFromCurrentValues(entry)!;

    private static object? SnapshotValue(IProperty property, ValueComparer? comparer, IUpdateEntry entry)
    {
        var value = SnapshotValue(comparer, entry.GetCurrentValue(property));

        var converter = property.GetValueConverter()
            ?? property.FindTypeMapping()?.Converter;

        if (converter != null)
        {
            value = converter.ConvertToProvider(value);
        }

        return value;
    }

    private static object? SnapshotValue(ValueComparer? comparer, object? value)
        => comparer == null ? value : comparer.Snapshot(value);

    private bool HasNullabilityError(
        IProperty property,
        object? propertyValue,
        IList<IProperty> nullabilityErrors)
    {
        if (!this._nullabilityCheckEnabled)
        {
            return false;
        }

        if (!property.IsNullable && propertyValue == null)
        {
            nullabilityErrors.Add(property);

            return true;
        }

        return false;
    }

    private void ThrowNullabilityErrorException(
        IUpdateEntry entry,
        IList<IProperty> nullabilityErrors)
    {
        if (this._sensitiveLoggingEnabled)
        {
            throw new DbUpdateException(
                InMemoryStrings.NullabilityErrorExceptionSensitive(
                    nullabilityErrors.Format(),
                    entry.EntityType.DisplayName(),
                    entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey()!.Properties)),
                new[] { entry });
        }

        throw new DbUpdateException(
            InMemoryStrings.NullabilityErrorException(
                nullabilityErrors.Format(),
                entry.EntityType.DisplayName()),
            new[] { entry });
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual void ThrowUpdateConcurrencyException(
        IUpdateEntry entry,
        Dictionary<IProperty, object?> concurrencyConflicts,
        IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger)
    {
        var entries = new[] { entry };

        var exception = this._sensitiveLoggingEnabled
            ? new DbUpdateConcurrencyException(
                InMemoryStrings.UpdateConcurrencyTokenExceptionSensitive(
                    entry.EntityType.DisplayName(),
                    entry.BuildCurrentValuesString(entry.EntityType.FindPrimaryKey()!.Properties),
                    entry.BuildOriginalValuesString(concurrencyConflicts.Keys),
                    "{"
                    + string.Join(
                        ", ",
                        concurrencyConflicts.Select(c => c.Key.Name + ": " + Convert.ToString(c.Value, CultureInfo.InvariantCulture)))
                    + "}"),
                entries)
            : new DbUpdateConcurrencyException(
                InMemoryStrings.UpdateConcurrencyTokenException(
                    entry.EntityType.DisplayName(),
                    concurrencyConflicts.Keys.Format()),
                entries);

        if (!updateLogger.OptimisticConcurrencyException(entry.Context, entries, exception, null).IsSuppressed)
        {
            throw exception;
        }
    }
}
