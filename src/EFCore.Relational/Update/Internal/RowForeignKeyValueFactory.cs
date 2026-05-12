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
public abstract class RowForeignKeyValueFactory<TKey, TForeignKey> : IRowForeignKeyValueFactory<TKey>
{
    private readonly IForeignKeyConstraint _foreignKey;
    private readonly IRowKeyValueFactory<TKey> _principalKeyValueFactory;
    private ValueConverter? _valueConverter;
    private IEqualityComparer<TKey>? _equalityComparer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected RowForeignKeyValueFactory(
        IForeignKeyConstraint foreignKey,
        IColumn column,
        ColumnAccessors columnAccessors)
    {
        _foreignKey = foreignKey;
        Column = column;
        ColumnAccessors = typeof(TKey) == typeof(TForeignKey)
            ? columnAccessors
            : new ColumnAccessors(
                ConvertAccessor((Func<IReadOnlyModificationCommand, (TForeignKey, bool)>)columnAccessors.CurrentValueGetter),
                ConvertAccessor((Func<IReadOnlyModificationCommand, (TForeignKey, bool)>)columnAccessors.OriginalValueGetter));
        _principalKeyValueFactory =
            (IRowKeyValueFactory<TKey>)((UniqueConstraint)foreignKey.PrincipalUniqueConstraint).GetRowKeyValueFactory();
    }

    private Func<IReadOnlyModificationCommand, (TKey, bool)> ConvertAccessor(
        Func<IReadOnlyModificationCommand, (TForeignKey, bool)> columnAccessor)
        => command =>
        {
            var tuple = columnAccessor(command);
            return (tuple.Item1 == null
                ? (default, tuple.Item2)
                : ((TKey)ValueConverter!.ConvertFromProvider(tuple.Item1)!, tuple.Item2))!;
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
#pragma warning disable EF1001 // Internal EF Core API usage.
    public virtual IEqualityComparer<TKey> EqualityComparer
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _equalityComparer, this, static factory
                => NullableComparerAdapter<TKey>.Wrap(factory.Column.ProviderValueComparer, factory.ValueConverter));
#pragma warning restore EF1001 // Internal EF Core API usage.

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IColumn Column { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ColumnAccessors ColumnAccessors { get; }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual ValueConverter? ValueConverter
        => typeof(TKey) == typeof(TForeignKey)
            ? null
            : NonCapturingLazyInitializer.EnsureInitialized(
                ref _valueConverter, this, static factory =>
                {
                    var foreignKey = factory._foreignKey;
                    var column = factory.Column;
                    var valueConverterSelector = foreignKey.Table.Model.Model.GetRelationalDependencies().ValueConverterSelector;
                    var converterInfos = valueConverterSelector.Select(typeof(TKey), typeof(TForeignKey)).ToList();
                    if (converterInfos.Count == 0)
                    {
                        var pkColumn = foreignKey.PrincipalColumns[0];
                        throw new InvalidOperationException(
                            RelationalStrings.StoredKeyTypesNotConvertable(
                                column.Name, column.StoreType, pkColumn.StoreType, pkColumn.Name));
                    }

                    return converterInfos.First().Create();
                });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object CreatePrincipalEquatableKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => new EquatableKeyValue<TKey>(
            _foreignKey,
            _principalKeyValueFactory.CreateKeyValue(command, fromOriginalValues),
            EqualityComparer);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object? CreateDependentEquatableKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => TryCreateDependentKeyValue(command, fromOriginalValues, out var keyValue)
            ? new EquatableKeyValue<TKey>(_foreignKey, keyValue, EqualityComparer)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract bool TryCreateDependentKeyValue(
        object?[] keyValues,
        [NotNullWhen(true)] out TKey? key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract bool TryCreateDependentKeyValue(
        IDictionary<string, object?> keyPropertyValues,
        [NotNullWhen(true)] out TKey? key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract bool TryCreateDependentKeyValue(
        IReadOnlyModificationCommand command,
        bool fromOriginalValues,
        [NotNullWhen(true)] out TKey? key);

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object[] CreatePrincipalKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => [_principalKeyValueFactory.CreateKeyValue(command, fromOriginalValues)!];

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object[]? CreateDependentKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => TryCreateDependentKeyValue(command, fromOriginalValues, out var value)
            ? [value]
            : null;
}
