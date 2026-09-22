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
public class CompositeRowForeignKeyValueFactory : CompositeRowValueFactory, IRowForeignKeyValueFactory<object?[]>
{
    private readonly IForeignKeyConstraint _foreignKey;
    private readonly IRowKeyValueFactory<object?[]> _principalKeyValueFactory;
    private List<ValueConverter?>? _valueConverters;
    private IEqualityComparer<object?[]>? _equalityComparer;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CompositeRowForeignKeyValueFactory(IForeignKeyConstraint foreignKey)
        : base(foreignKey.Columns)
    {
        _foreignKey = foreignKey;
        _principalKeyValueFactory =
            (IRowKeyValueFactory<object?[]>)((UniqueConstraint)foreignKey.PrincipalUniqueConstraint).GetRowKeyValueFactory();
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override List<ValueConverter?>? ValueConverters
        => NonCapturingLazyInitializer.EnsureInitialized(
            ref _valueConverters, this, static factory =>
            {
                var foreignKey = factory._foreignKey;
                var valueConverterSelector = foreignKey.Table.Model.Model.GetRelationalDependencies().ValueConverterSelector;
                var columns = foreignKey.Columns;
                var valueConverters = new List<ValueConverter?>(columns.Count);
                for (var i = 0; i < columns.Count; i++)
                {
                    var fkColumn = columns[i];
                    var pkColumn = foreignKey.PrincipalColumns[i];
                    var fkType = fkColumn.ProviderClrType;
                    var pkType = pkColumn.ProviderClrType;
                    if (fkType != pkType)
                    {
                        var converterInfos = valueConverterSelector.Select(pkType, fkType).ToList();
                        if (converterInfos.Count == 0)
                        {
                            throw new InvalidOperationException(
                                RelationalStrings.StoredKeyTypesNotConvertable(
                                    fkColumn.Name, fkColumn.StoreType, pkColumn.StoreType, pkColumn.Name));
                        }

                        valueConverters.Add(converterInfos.First().Create());
                    }
                    else
                    {
                        valueConverters.Add(null);
                    }
                }

                return valueConverters;
            });

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override IEqualityComparer<object?[]> EqualityComparer
    {
        get => NonCapturingLazyInitializer.EnsureInitialized(
            ref _equalityComparer, this, static factory => CreateEqualityComparer(factory.Columns, factory.ValueConverters));
        protected set => _equalityComparer = value;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object CreatePrincipalEquatableKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => new EquatableKeyValue<object?[]>(
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
            ? new EquatableKeyValue<object?[]>(_foreignKey, keyValue, EqualityComparer)
            : null;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object[] CreatePrincipalKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => _principalKeyValueFactory.CreateKeyValue(command, fromOriginalValues)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual object[]? CreateDependentKeyValue(IReadOnlyModificationCommand command, bool fromOriginalValues = false)
        => TryCreateDependentKeyValue(command, fromOriginalValues, out var keyValue)
            ? (object[])keyValue
            : null;
}
