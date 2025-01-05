// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SimpleRowIndexValueFactory<TKey> : IRowIndexValueFactory<TKey>
{
    private readonly IColumn _column;
    private readonly ITableIndex _index;
    private readonly ColumnAccessors _columnAccessors;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SimpleRowIndexValueFactory(ITableIndex index)
    {
        _index = index;
        _column = index.Columns.Single();
        _columnAccessors = ((Column)_column).Accessors;
#pragma warning disable EF1001 // Internal EF Core API usage.
        EqualityComparer = NullableComparerAdapter<TKey>.Wrap(_column.ProviderValueComparer);
#pragma warning restore EF1001 // Internal EF Core API usage.
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IEqualityComparer<TKey> EqualityComparer { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateIndexValue(object?[] keyValues, out TKey? key, out bool hasNullValue)
    {
        key = (TKey?)keyValues[0];
        hasNullValue = key == null;
        return true;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateIndexValue(IDictionary<string, object?> keyValues, out TKey? key, out bool hasNullValue)
    {
        if (keyValues.TryGetValue(_column.Name, out var value))
        {
            key = (TKey?)value;
            hasNullValue = key == null;
            return true;
        }

        key = default;
        hasNullValue = true;
        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateIndexValue(
        IReadOnlyModificationCommand command,
        bool fromOriginalValues,
        out TKey? key,
        out bool hasNullValue)
    {
        (key, var present) = fromOriginalValues
            ? ((Func<IReadOnlyModificationCommand, (TKey, bool)>)_columnAccessors.OriginalValueGetter)(command)
            : ((Func<IReadOnlyModificationCommand, (TKey, bool)>)_columnAccessors.CurrentValueGetter)(command);
        hasNullValue = key == null;
        return present;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual (object? Value, bool HasNullValue) CreateEquatableIndexValue(
        IReadOnlyModificationCommand command,
        bool fromOriginalValues = false)
        => TryCreateIndexValue(command, fromOriginalValues, out var keyValue, out var hasNullValue)
            ? (new EquatableKeyValue<TKey>(_index, keyValue, EqualityComparer), hasNullValue)
            : (null, true);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual (object?[]? Value, bool HasNullValue) CreateIndexValue(
        IReadOnlyModificationCommand command,
        bool fromOriginalValues = false)
        => TryCreateIndexValue(command, fromOriginalValues, out var value, out var hasNullValue)
            ? (new object?[] { value }, hasNullValue)
            : (null, true);
}
