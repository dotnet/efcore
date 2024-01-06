// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Update.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class RowForeignKeyValueFactoryFactory : IRowForeignKeyValueFactoryFactory
{
    private readonly IValueConverterSelector _valueConverterSelector;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public RowForeignKeyValueFactoryFactory(IValueConverterSelector valueConverterSelector)
    {
        _valueConverterSelector = valueConverterSelector;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IRowForeignKeyValueFactory Create(IForeignKeyConstraint foreignKey)
        => foreignKey.Columns.Count == 1
            ? (IRowForeignKeyValueFactory)CreateMethod
                .MakeGenericMethod(
                    foreignKey.PrincipalColumns.First().ProviderClrType,
                    foreignKey.Columns.First().ProviderClrType)
                .Invoke(null, [foreignKey, _valueConverterSelector])!
            : new CompositeRowForeignKeyValueFactory(foreignKey, _valueConverterSelector);

    private static readonly MethodInfo CreateMethod = typeof(RowForeignKeyValueFactoryFactory).GetTypeInfo()
        .GetDeclaredMethod(nameof(CreateSimple))!;

    [UsedImplicitly]
    private static IRowForeignKeyValueFactory CreateSimple<TKey, TForeignKey>(
        IForeignKeyConstraint foreignKey,
        IValueConverterSelector valueConverterSelector)
        where TKey : notnull
    {
        var dependentColumn = foreignKey.Columns.First();
        var principalColumn = foreignKey.PrincipalColumns.First();
        var columnAccessors = ((Column)dependentColumn).Accessors;

        if (principalColumn.ProviderClrType.IsNullableType()
            || (dependentColumn.IsNullable
                && principalColumn.IsNullable))
        {
            return new SimpleFullyNullableRowForeignKeyValueFactory<TKey, TForeignKey>(
                foreignKey, dependentColumn, columnAccessors, valueConverterSelector);
        }

        if (dependentColumn.IsNullable)
        {
            return (IRowForeignKeyValueFactory<TKey>)Activator.CreateInstance(
                typeof(SimpleNullableRowForeignKeyValueFactory<,>).MakeGenericType(
                    typeof(TKey), typeof(TForeignKey)), foreignKey, dependentColumn, columnAccessors, valueConverterSelector)!;
        }

        return principalColumn.IsNullable
            ? (IRowForeignKeyValueFactory<TKey>)Activator.CreateInstance(
                typeof(SimpleNullablePrincipalRowForeignKeyValueFactory<,>).MakeGenericType(
                    typeof(TKey), typeof(TKey), typeof(TForeignKey)), foreignKey, dependentColumn, columnAccessors)!
            : new SimpleNonNullableRowForeignKeyValueFactory<TKey, TForeignKey>(
                foreignKey, dependentColumn, columnAccessors, valueConverterSelector);
    }
}
