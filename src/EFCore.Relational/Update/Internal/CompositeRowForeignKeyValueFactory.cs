// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
    private readonly List<ValueConverter?> _valueConverters;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public CompositeRowForeignKeyValueFactory(
        IForeignKeyConstraint foreignKey,
        IValueConverterSelector valueConverterSelector)
        : base(foreignKey.Columns)
    {
        _foreignKey = foreignKey;
        _principalKeyValueFactory =
            (IRowKeyValueFactory<object?[]>)((UniqueConstraint)foreignKey.PrincipalUniqueConstraint).GetRowKeyValueFactory();

        var columns = foreignKey.Columns;
        _valueConverters = new List<ValueConverter?>(columns.Count);

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

                _valueConverters.Add(converterInfos.First().Create());
            }
            else
            {
                _valueConverters.Add(null);
            }
        }

        ValueConverters = _valueConverters;
        EqualityComparer = CreateEqualityComparer(columns, _valueConverters);
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
