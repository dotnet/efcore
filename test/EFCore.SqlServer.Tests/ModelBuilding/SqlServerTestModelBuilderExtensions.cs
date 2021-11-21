// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> ToTable<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        Action<RelationalModelBuilderTest.TestTableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(b => buildAction(new RelationalModelBuilderTest.GenericTestTableBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nongenericBuilder:
                nongenericBuilder.Instance.ToTable(
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestTableBuilder<TEntity>(b)));
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
        Action<SqlServerModelBuilderGenericTest.TestTemporalTableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<TableBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.IsTemporal(
                    b => buildAction(new SqlServerModelBuilderGenericTest.GenericTestTemporalTableBuilder<TEntity>(b)));
                break;
            case IInfrastructure<TableBuilder> nongenericBuilder:
                nongenericBuilder.Instance.IsTemporal(
                    b => buildAction(new SqlServerModelBuilderGenericTest.NonGenericTestTemporalTableBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }
}
