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
public class SimpleNullableRowForeignKeyValueFactory<TKey> : RowForeignKeyValueFactory<TKey>
    where TKey : struct
{
    private readonly IColumn _column;
    private readonly ColumnAccessors _columnAccessors;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SimpleNullableRowForeignKeyValueFactory(
        IForeignKeyConstraint foreignKey,
        IColumn column,
        ColumnAccessors columnAccessors)
        : base(foreignKey)
    {
        _column = column;
        _columnAccessors = columnAccessors;
        EqualityComparer = CreateKeyEqualityComparer(column);
    }

    /// <inheritdoc />
    public override IEqualityComparer<TKey> EqualityComparer { get; }

    /// <inheritdoc />
    public override bool TryCreateDependentKeyValue(object?[] keyValues, [NotNullWhen(true)] out TKey key)
    {
        return HandleNullableValue((TKey?)keyValues[0], out key);
    }

    /// <inheritdoc />
    public override bool TryCreateDependentKeyValue(
        IDictionary<string, object?> keyPropertyValues, [NotNullWhen(true)] out TKey key)
    {
        if (keyPropertyValues.TryGetValue(_column.Name, out var value))
        {
            return HandleNullableValue((TKey?)value, out key);
        }

        key = default(TKey);
        return false;
    }

    /// <inheritdoc />
    public override bool TryCreateDependentKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues, [NotNullWhen(true)] out TKey key)
    {
        var (keyValue, present) = fromOriginalValues
            ? ((Func<IReadOnlyModificationCommand, (TKey?, bool)>)_columnAccessors.OriginalValueGetter)(command)
            : ((Func<IReadOnlyModificationCommand, (TKey?, bool)>)_columnAccessors.CurrentValueGetter)(command);
        return HandleNullableValue(present ? keyValue : null, out key);
    }

    private static bool HandleNullableValue(TKey? value, out TKey key)
    {
        if (value.HasValue)
        {
            key = (TKey)value;
            return true;
        }

        key = default;
        return false;
    }
}
