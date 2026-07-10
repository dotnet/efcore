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
public class RowForeignKeyValueFactoryFactory : IRowForeignKeyValueFactoryFactory
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IRowForeignKeyValueFactory Create(IForeignKeyConstraint foreignKey)
    {
        if (foreignKey.Columns.Count != 1)
        {
            return new CompositeRowForeignKeyValueFactory(foreignKey);
        }

        var principalColumn = foreignKey.PrincipalColumns.First();
        var createMethod = principalColumn.ProviderClrType.IsNullableType() || principalColumn.IsNullable
            ? CreateNullableMethod
            : CreateNonNullableMethod;

        return (IRowForeignKeyValueFactory)createMethod
            .MakeGenericMethod(
                foreignKey.PrincipalColumns.First().ProviderClrType,
                foreignKey.Columns.First().ProviderClrType)
            .Invoke(null, [foreignKey])!;
    }

    private static readonly MethodInfo CreateNullableMethod = typeof(RowForeignKeyValueFactoryFactory).GetTypeInfo()
        .GetDeclaredMethod(nameof(CreateSimpleNullableFactory))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IRowForeignKeyValueFactory CreateSimpleNullableFactory<TKey, TForeignKey>(
        IForeignKeyConstraint foreignKey)
        where TKey : notnull
    {
        var dependentColumn = foreignKey.Columns.First();
        var principalColumn = foreignKey.PrincipalColumns.First();
        var columnAccessors = ((Column)dependentColumn).Accessors;

        return principalColumn.ProviderClrType.IsNullableType()
            || (dependentColumn.IsNullable && principalColumn.IsNullable)
                ? new SimpleFullyNullableRowForeignKeyValueFactory<TKey, TForeignKey>(
                    foreignKey, dependentColumn, columnAccessors)
                : new SimpleNullablePrincipalRowForeignKeyValueFactory<TKey, TForeignKey>(
                    foreignKey, dependentColumn, columnAccessors);
    }

    private static readonly MethodInfo CreateNonNullableMethod = typeof(RowForeignKeyValueFactoryFactory).GetTypeInfo()
        .GetDeclaredMethod(nameof(CreateSimpleNonNullableFactory))!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public static IRowForeignKeyValueFactory CreateSimpleNonNullableFactory<TKey, TForeignKey>(
        IForeignKeyConstraint foreignKey)
        where TKey : struct
    {
        var dependentColumn = foreignKey.Columns.First();
        var columnAccessors = ((Column)dependentColumn).Accessors;

        return dependentColumn.IsNullable
            ? new SimpleNullableRowForeignKeyValueFactory<TKey, TForeignKey>(foreignKey, dependentColumn, columnAccessors)
            : new SimpleNonNullableRowForeignKeyValueFactory<TKey, TForeignKey>(
                foreignKey, dependentColumn, columnAccessors);
    }
}
