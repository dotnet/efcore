// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public abstract class CompositeRowValueFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected CompositeRowValueFactory(IReadOnlyList<IColumn> columns)
    {
        Columns = columns;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual List<ValueConverter?>? ValueConverters { get; set; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEqualityComparer<object?[]> EqualityComparer { get; protected set; } = null!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual IReadOnlyList<IColumn> Columns { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateDependentKeyValue(object?[] keyValues, [NotNullWhen(true)] out object?[]? key)
        => TryCreateDependentKeyValue(keyValues, out key, out var hasNullValue)
            && !hasNullValue;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual bool TryCreateDependentKeyValue(
        object?[] keyValues,
        [NotNullWhen(true)] out object?[]? key,
        out bool hasNullValue)
    {
        key = keyValues;
        hasNullValue = keyValues.All(k => k != null);
        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateDependentKeyValue(IDictionary<string, object?> keyValues, [NotNullWhen(true)] out object?[]? key)
        => TryCreateDependentKeyValue(keyValues, out key, out var hasNullValue)
            && !hasNullValue;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual bool TryCreateDependentKeyValue(
        IDictionary<string, object?> keyValues,
        [NotNullWhen(true)] out object?[]? key,
        out bool hasNullValue)
    {
        key = new object[Columns.Count];
        var index = 0;
        hasNullValue = false;

        foreach (var column in Columns)
        {
            if (!keyValues.TryGetValue(column.Name, out var value))
            {
                return false;
            }

            if (value == null)
            {
                hasNullValue = true;
            }

            key[index++] = value;
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateDependentKeyValue(
        IReadOnlyModificationCommand command,
        bool fromOriginalValues,
        [NotNullWhen(true)] out object?[]? key)
    {
        var result = TryCreateDependentKeyValue(command, fromOriginalValues, out key, out var hasNullValue);
        if (!result
            || hasNullValue)
        {
            key = null;
            return result;
        }

        return result;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected virtual bool TryCreateDependentKeyValue(
        IReadOnlyModificationCommand command,
        bool fromOriginalValues,
        [NotNullWhen(true)] out object?[]? key,
        out bool hasNullValue)
    {
        var converters = ValueConverters;
        key = new object[Columns.Count];
        var index = 0;
        hasNullValue = false;

        for (var i = 0; i < Columns.Count; i++)
        {
            var column = Columns[i];

            if (command.Entries.Count > 0)
            {
                object? value = null;
                var valueFound = false;
                foreach (var entry in command.Entries)
                {
                    var property = column.FindColumnMapping(entry.EntityType)?.Property;
                    if (property == null)
                    {
                        continue;
                    }

                    valueFound = true;
                    value = fromOriginalValues ? entry.GetOriginalProviderValue(property) : entry.GetCurrentProviderValue(property);

                    var converter = converters?[i];
                    if (converter != null)
                    {
                        value = converter.ConvertFromProvider(value);
                    }

                    if (!fromOriginalValues
                        && (entry.EntityState == EntityState.Added
                            || entry.EntityState == EntityState.Modified && entry.IsModified(property)))
                    {
                        break;
                    }

                    if (fromOriginalValues
                        && entry.EntityState != EntityState.Added)
                    {
                        break;
                    }
                }

                if (!valueFound)
                {
                    return false;
                }

                if (value == null)
                {
                    hasNullValue = true;
                }

                key[index++] = value;
            }
            else
            {
                var modification = command.ColumnModifications.FirstOrDefault(m => m.ColumnName == column.Name);
                if (modification == null)
                {
                    return false;
                }

                var value = fromOriginalValues ? modification.OriginalValue : modification.Value;
                if (value == null)
                {
                    hasNullValue = true;
                }

                key[index++] = value;
            }
        }

        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected static IEqualityComparer<object?[]> CreateEqualityComparer(
        IReadOnlyList<IColumn> columns,
        List<ValueConverter?>? valueConverters)
        => new CompositeCustomComparer(columns.Select(c => c.ProviderValueComparer).ToList(), valueConverters);

    private sealed class CompositeCustomComparer : IEqualityComparer<object?[]>
    {
        private readonly Func<object?, object?, bool>[] _equals;
        private readonly Func<object, int>[] _hashCodes;

        public CompositeCustomComparer(List<ValueComparer> comparers, List<ValueConverter?>? valueConverters)
        {
            var columnCount = comparers.Count;
            _equals = new Func<object?, object?, bool>[columnCount];
            _hashCodes = new Func<object, int>[columnCount];

            for (var i = 0; i < columnCount; i++)
            {
                var converter = valueConverters?[i];
                var comparer = comparers[i];
                if (converter != null)
                {
                    _equals[i] = (v1, v2) => comparer.Equals(converter.ConvertToProvider(v1), converter.ConvertToProvider(v2));
                    _hashCodes[i] = v => comparer.GetHashCode(converter.ConvertToProvider(v)!);
                }
                else
                {
                    _equals[i] = comparer.Equals;
                    _hashCodes[i] = comparer.GetHashCode;
                }
            }
        }

        public bool Equals(object?[]? x, object?[]? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x is null)
            {
                return y is null;
            }

            if (y is null)
            {
                return false;
            }

            if (x.Length != y.Length)
            {
                return false;
            }

            for (var i = 0; i < x.Length; i++)
            {
                if (!_equals[i](x[i], y[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode(object?[] obj)
        {
            var hashCode = new HashCode();

            // ReSharper disable once ForCanBeConvertedToForeach
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < obj.Length; i++)
            {
                var value = obj[i];
                hashCode.Add(value == null ? 0 : _hashCodes[i](value));
            }

            return hashCode.ToHashCode();
        }
    }
}
