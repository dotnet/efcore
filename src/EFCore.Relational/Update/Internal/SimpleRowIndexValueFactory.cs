// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
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
        EqualityComparer = NullableComparerAdapter<TKey>.Wrap(_column.PropertyMappings.First().TypeMapping.ProviderComparer);
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
    public virtual bool TryCreateIndexValue(object?[] keyValues, [NotNullWhen(true)] out TKey? key)
    {
        key = (TKey?)keyValues[0];
        return key != null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateIndexValue(IDictionary<string, object?> keyValues, [NotNullWhen(true)] out TKey? key)
    {
        if (keyValues.TryGetValue(_column.Name, out var value))
        {
            key = (TKey?)value;
            return key != null;
        }

        key = default;
        return false;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual bool TryCreateIndexValue(IReadOnlyModificationCommand command, bool fromOriginalValues, [NotNullWhen(true)] out TKey? key)
    {
        (key, var present) = fromOriginalValues
            ? ((Func<IReadOnlyModificationCommand, (TKey, bool)>)_columnAccessors.OriginalValueGetter)(command)
            : ((Func<IReadOnlyModificationCommand, (TKey, bool)>)_columnAccessors.CurrentValueGetter)(command);
        return present && key != null;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? CreateValueIndex(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => TryCreateIndexValue(command, fromOriginalValues, out var keyValue)
            ? new ValueIndex<TKey>(_index, keyValue, EqualityComparer)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object[]? CreateValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => TryCreateIndexValue(command, fromOriginalValues, out var value)
            ? (new object[] { value })
            : null;
}
