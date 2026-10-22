// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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

    public static RelationalModelBuilderTest.TestTableBuilder<TEntity> IsMemoryOptimized<TEntity>(
        this RelationalModelBuilderTest.TestTableBuilder<TEntity> builder,
        bool memoryOptimized = true)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<TableBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.IsMemoryOptimized(memoryOptimized);
                break;
            case IInfrastructure<TableBuilder> nongenericBuilder:
                nongenericBuilder.Instance.IsMemoryOptimized(memoryOptimized);
                break;
        }

        return builder;
    }

    public static RelationalModelBuilderTest.TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> IsMemoryOptimized<
        TOwnerEntity, TDependentEntity>(
        this RelationalModelBuilderTest.TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> builder,
        bool memoryOptimized = true)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.IsMemoryOptimized(memoryOptimized);
                break;
            case IInfrastructure<OwnedNavigationTableBuilder> nongenericBuilder:
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

    public static RelationalModelBuilderTest.TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> IsTemporal
        <TOwnerEntity, TDependentEntity>(
            this RelationalModelBuilderTest.TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> builder,
            bool temporal = true)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.IsTemporal(temporal);
                break;
            case IInfrastructure<OwnedNavigationTableBuilder> nongenericBuilder:
                nongenericBuilder.Instance.IsTemporal(temporal);
                break;
        }

        return builder;
    }

    public static RelationalModelBuilderTest.TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> IsTemporal<
        TOwnerEntity, TDependentEntity>(
        this RelationalModelBuilderTest.TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity> builder,
        Action<SqlServerModelBuilderTestBase.TestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.IsTemporal(
                    b => buildAction(
                        new SqlServerModelBuilderTestBase.GenericTestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
            case IInfrastructure<OwnedNavigationTableBuilder> nongenericBuilder:
                nongenericBuilder.Instance.IsTemporal(
                    b => buildAction(
                        new SqlServerModelBuilderTestBase.NonGenericTestOwnedNavigationTemporalTableBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
        }

        return builder;
    }
}
