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
public class SimpleNullablePrincipalRowForeignKeyValueFactory<TKey, TForeignKey>
    : RowForeignKeyValueFactory<TKey, TForeignKey>
    where TKey : notnull
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SimpleNullablePrincipalRowForeignKeyValueFactory(
        IForeignKeyConstraint foreignKey,
        IColumn column,
        ColumnAccessors columnAccessors,
        IValueConverterSelector valueConverterSelector)
        : base(foreignKey, column, columnAccessors, valueConverterSelector)
    {
        EqualityComparer = CreateKeyEqualityComparer(column);
    }

    /// <inheritdoc />
    public override IEqualityComparer<TKey> EqualityComparer { get; }

    /// <inheritdoc />
    public override bool TryCreateDependentKeyValue(object?[] keyValues, [NotNullWhen(true)] out TKey? key)
    {
        key = (TKey?)keyValues[0]!;
        return true;
    }

    /// <inheritdoc />
    public override bool TryCreateDependentKeyValue(
        IDictionary<string, object?> keyPropertyValues,
        [NotNullWhen(true)] out TKey? key)
    {
        if (keyPropertyValues.TryGetValue(Column.Name, out var value))
        {
            key = (TKey?)value!;
            return true;
        }

        key = default;
        return false;
    }

    /// <inheritdoc />
    public override bool TryCreateDependentKeyValue(
        IReadOnlyModificationCommand command,
        bool fromOriginalValues,
        [NotNullWhen(true)] out TKey? key)
    {
        (key, var present) = fromOriginalValues
            ? ((Func<IReadOnlyModificationCommand, (TKey, bool)>)ColumnAccessors.OriginalValueGetter)(command)
            : ((Func<IReadOnlyModificationCommand, (TKey, bool)>)ColumnAccessors.CurrentValueGetter)(command);
        return present;
    }
}
