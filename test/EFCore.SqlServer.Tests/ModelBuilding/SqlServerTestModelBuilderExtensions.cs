// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public static class SqlServerTestModelBuilderExtensions
{
    public static ModelBuilderTest.TestIndexBuilder<TEntity> IsClustered<TEntity>(
        this ModelBuilderTest.TestIndexBuilder<TEntity> builder,
        bool clustered = true)
    {
        switch (builder)
        {
            case IInfrastructure<IndexBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.IsClustered(clustered);
                break;
            case IInfrastructure<IndexBuilder> nongenericBuilder:
                nongenericBuilder.Instance.IsClustered(clustered);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TEntity, TDependentEntity> IsMemoryOptimized<TEntity,
        TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TEntity, TDependentEntity> builder,
        bool memoryOptimized = true)
        where TEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.IsMemoryOptimized(memoryOptimized);
                break;
            case IInfrastructure<OwnedNavigationBuilder> nongenericBuilder:
                nongenericBuilder.Instance.IsMemoryOptimized(memoryOptimized);
                break;
        }

        return builder;
    }

    public static RelationalModelBuilderTest.TestTableBuilder<TEntity> IsTemporal<TEntity>(
        this RelationalModelBuilderTest.TestTableBuilder<TEntity> builder,
        bool temporal = true)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<TableBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.IsTemporal(temporal);
                break;
            case IInfrastructure<TableBuilder> nongenericBuilder:
                nongenericBuilder.Instance.IsTemporal(temporal);
                break;
        }

        return builder;
    }

    public static RelationalModelBuilderTest.TestTableBuilder<TEntity> IsTemporal<TEntity>(
        this RelationalModelBuilderTest.TestTableBuilder<TEntity> builder,
        Action<SqlServerModelBuilderTestBase.TestTemporalTableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<TableBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.IsTemporal(
                    b => buildAction(new SqlServerModelBuilderTestBase.GenericTestTemporalTableBuilder<TEntity>(b)));
                break;
            case IInfrastructure<TableBuilder> nongenericBuilder:
                nongenericBuilder.Instance.IsTemporal(
                    b => buildAction(new SqlServerModelBuilderTestBase.NonGenericTestTemporalTableBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }
}
