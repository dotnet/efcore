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
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual IRowForeignKeyValueFactory Create(IForeignKeyConstraint foreignKey)
        => foreignKey.Columns.Count == 1
            ? (IRowForeignKeyValueFactory)_createMethod
                .MakeGenericMethod(foreignKey.Columns.First().ProviderClrType)
                .Invoke(null, new object[] { foreignKey })!
            : new CompositeRowForeignKeyValueFactory(foreignKey);

    private static readonly MethodInfo _createMethod = typeof(RowForeignKeyValueFactoryFactory).GetTypeInfo()
        .GetDeclaredMethod(nameof(CreateSimple))!;

    [UsedImplicitly]
    private static IRowForeignKeyValueFactory CreateSimple<TKey>(IForeignKeyConstraint foreignKey)
        where TKey : notnull
    {
        var dependentColumn = foreignKey.Columns.Single();
        var dependentType = dependentColumn.ProviderClrType;
        var principalType = foreignKey.PrincipalColumns.Single().ProviderClrType;
        var columnAccessors = ((Column)dependentColumn).Accessors;

        if (dependentType.IsNullableType()
            && principalType.IsNullableType())
        {
            return new SimpleFullyNullableRowForeignKeyValueFactory<TKey>(foreignKey, dependentColumn, columnAccessors);
        }

        if (dependentType.IsNullableType())
        {
            return (IRowForeignKeyValueFactory<TKey>)Activator.CreateInstance(
                typeof(SimpleNullableRowForeignKeyValueFactory<>).MakeGenericType(
                    typeof(TKey)), foreignKey, dependentColumn, columnAccessors)!;
        }

        return principalType.IsNullableType()
            ? (IRowForeignKeyValueFactory<TKey>)Activator.CreateInstance(
                typeof(SimpleNullablePrincipalRowForeignKeyValueFactory<,>).MakeGenericType(
                    typeof(TKey), typeof(TKey).UnwrapNullableType()), foreignKey, dependentColumn, columnAccessors)!
            : new SimpleNonNullableRowForeignKeyValueFactory<TKey>(foreignKey, dependentColumn, columnAccessors);
    }
}
