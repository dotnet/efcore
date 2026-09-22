// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public static class RelationalTestModelBuilderExtensions
{
    public static ModelBuilderTest.TestModelBuilder HasDefaultSchema(
        this ModelBuilderTest.TestModelBuilder modelBuilder,
        string? schema)
    {
        modelBuilder.GetInfrastructure().HasDefaultSchema(schema);

        return modelBuilder;
    }

    public static ModelBuilderTest.TestModelBuilder UseCollation(
        this ModelBuilderTest.TestModelBuilder modelBuilder,
        string? collation)
    {
        modelBuilder.GetInfrastructure().UseCollation(collation);

        return modelBuilder;
    }

    public static ModelBuilderTest.TestPropertyBuilder<TProperty> HasColumnName<TProperty>(
        this ModelBuilderTest.TestPropertyBuilder<TProperty> builder,
        string? name)
    {
        switch (builder)
        {
            case IInfrastructure<PropertyBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.HasColumnName(name);
                break;
            case IInfrastructure<PropertyBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasColumnName(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPropertyBuilder<TProperty> HasColumnType<TProperty>(
        this ModelBuilderTest.TestPropertyBuilder<TProperty> builder,
        string typeName)
    {
        switch (builder)
        {
            case IInfrastructure<PropertyBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.HasColumnType(typeName);
                break;
            case IInfrastructure<PropertyBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasColumnType(typeName);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPropertyBuilder<TProperty> HasDefaultValueSql<TProperty>(
        this ModelBuilderTest.TestPropertyBuilder<TProperty> builder,
        string sql)
    {
        switch (builder)
        {
            case IInfrastructure<PropertyBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.HasDefaultValueSql(sql);
                break;
            case IInfrastructure<PropertyBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasDefaultValueSql(sql);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPropertyBuilder<TProperty> HasComputedColumnSql<TProperty>(
        this ModelBuilderTest.TestPropertyBuilder<TProperty> builder,
        string sql)
    {
        switch (builder)
        {
            case IInfrastructure<PropertyBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.HasComputedColumnSql(sql);
                break;
            case IInfrastructure<PropertyBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasComputedColumnSql(sql);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPropertyBuilder<TProperty> HasDefaultValue<TProperty>(
        this ModelBuilderTest.TestPropertyBuilder<TProperty> builder,
        object value)
    {
        switch (builder)
        {
            case IInfrastructure<PropertyBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.HasDefaultValue(value);
                break;
            case IInfrastructure<PropertyBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasDefaultValue(value);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPropertyBuilder<TProperty> IsFixedLength<TProperty>(
        this ModelBuilderTest.TestPropertyBuilder<TProperty> builder,
        bool fixedLength = true)
    {
        switch (builder)
        {
            case IInfrastructure<PropertyBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.IsFixedLength(fixedLength);
                break;
            case IInfrastructure<PropertyBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.IsFixedLength(fixedLength);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPropertyBuilder<TProperty> HasJsonPropertyName<TProperty>(
        this ModelBuilderTest.TestPropertyBuilder<TProperty> builder,
        string? name)
    {
        switch (builder)
        {
            case IInfrastructure<PropertyBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.HasJsonPropertyName(name);
                break;
            case IInfrastructure<PropertyBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasJsonPropertyName(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> HasColumnName<TProperty>(
        this ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> builder,
        string? name)
    {
        switch (builder)
        {
            case IInfrastructure<PrimitiveCollectionBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.HasColumnName(name);
                break;
            case IInfrastructure<PrimitiveCollectionBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasColumnName(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> HasColumnType<TProperty>(
        this ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> builder,
        string typeName)
    {
        switch (builder)
        {
            case IInfrastructure<PrimitiveCollectionBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.HasColumnType(typeName);
                break;
            case IInfrastructure<PrimitiveCollectionBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasColumnType(typeName);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> HasDefaultValueSql<TProperty>(
        this ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> builder,
        string sql)
    {
        switch (builder)
        {
            case IInfrastructure<PrimitiveCollectionBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.HasDefaultValueSql(sql);
                break;
            case IInfrastructure<PrimitiveCollectionBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasDefaultValueSql(sql);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> HasComputedColumnSql<TProperty>(
        this ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> builder,
        string sql)
    {
        switch (builder)
        {
            case IInfrastructure<PrimitiveCollectionBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.HasComputedColumnSql(sql);
                break;
            case IInfrastructure<PrimitiveCollectionBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasComputedColumnSql(sql);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> HasDefaultValue<TProperty>(
        this ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> builder,
        object value)
    {
        switch (builder)
        {
            case IInfrastructure<PrimitiveCollectionBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.HasDefaultValue(value);
                break;
            case IInfrastructure<PrimitiveCollectionBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasDefaultValue(value);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> IsFixedLength<TProperty>(
        this ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> builder,
        bool fixedLength = true)
    {
        switch (builder)
        {
            case IInfrastructure<PrimitiveCollectionBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.IsFixedLength(fixedLength);
                break;
            case IInfrastructure<PrimitiveCollectionBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.IsFixedLength(fixedLength);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> UseCollation<TProperty>(
        this ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> builder,
        string? collation)
    {
        switch (builder)
        {
            case IInfrastructure<PrimitiveCollectionBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.UseCollation(collation);
                break;
            case IInfrastructure<PrimitiveCollectionBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.UseCollation(collation);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> HasJsonPropertyName<TProperty>(
        this ModelBuilderTest.TestPrimitiveCollectionBuilder<TProperty> builder,
        string? name)
    {
        switch (builder)
        {
            case IInfrastructure<PrimitiveCollectionBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.HasJsonPropertyName(name);
                break;
            case IInfrastructure<PrimitiveCollectionBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasJsonPropertyName(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> UseTpcMappingStrategy<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.UseTpcMappingStrategy();
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.UseTpcMappingStrategy();
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> UseTphMappingStrategy<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.UseTphMappingStrategy();
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.UseTphMappingStrategy();
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> UseTptMappingStrategy<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.UseTptMappingStrategy();
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.UseTptMappingStrategy();
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> ToTable<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string? name)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(name);
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToTable(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> ToTable<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        string? schema)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(name, schema);
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToTable(name, schema);
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
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToTable(
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestTableBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> ToTable<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestTableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(
                    name,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestTableBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToTable(
                    name,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestTableBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> ToTable<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestTableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestTableBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToTable(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestTableBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToTable<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string? name)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(name);
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToTable(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToTable<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        string? schema)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(name, schema);
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToTable(name, schema);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToTable<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        Action<RelationalModelBuilderTest.TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToTable(
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToTable<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToTable(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToTable<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToTable(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationTableBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> SplitToTable<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestSplitTableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.SplitToTable(
                    name,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestSplitTableBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.SplitToTable(
                    name,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestSplitTableBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> SplitToTable<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestSplitTableBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.SplitToTable(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestSplitTableBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.SplitToTable(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestSplitTableBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> SplitToTable<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.SplitToTable(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.SplitToTable(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> SplitToTable<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.SplitToTable(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.SplitToTable(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationSplitTableBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> ToView<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string? name)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.ToView(name);
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToView(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> ToView<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string? name,
        string? schema)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.ToView(name, schema);
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToView(name, schema);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> ToView<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestViewBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.ToView(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestViewBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToView(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestViewBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> ToView<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestViewBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.ToView(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestViewBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToView(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestViewBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToView<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string? name)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.ToView(name);
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToView(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToView<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string? name,
        string? schema)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.ToView(name, schema);
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToView(name, schema);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToView<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.ToView(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToView(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToView<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.ToView(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToView(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationViewBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> SplitToView<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestSplitViewBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.SplitToView(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestSplitViewBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.SplitToView(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestSplitViewBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> SplitToView<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestSplitViewBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.SplitToView(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestSplitViewBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.SplitToView(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestSplitViewBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> SplitToView<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.SplitToView(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.SplitToView(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> SplitToView<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.SplitToView(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.SplitToView(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationSplitViewBuilder<TOwnerEntity, TDependentEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> ToFunction<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestTableValuedFunctionBuilder<TEntity>> buildAction)
        where TEntity : class
        => builder;

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> UpdateUsingStoredProcedure<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        Action<RelationalModelBuilderTest.TestStoredProcedureBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.UpdateUsingStoredProcedure(
                    b => buildAction(new RelationalModelBuilderTest.GenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.UpdateUsingStoredProcedure(
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> UpdateUsingStoredProcedure<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestStoredProcedureBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.UpdateUsingStoredProcedure(
                    name,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.UpdateUsingStoredProcedure(
                    name,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> UpdateUsingStoredProcedure<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestStoredProcedureBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.UpdateUsingStoredProcedure(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.UpdateUsingStoredProcedure(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> UpdateUsingStoredProcedure<TOwnerEntity,
        TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        Action<RelationalModelBuilderTest.TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.UpdateUsingStoredProcedure(
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.UpdateUsingStoredProcedure(
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> UpdateUsingStoredProcedure<TOwnerEntity,
        TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.UpdateUsingStoredProcedure(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.UpdateUsingStoredProcedure(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> UpdateUsingStoredProcedure<TOwnerEntity,
        TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.UpdateUsingStoredProcedure(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.UpdateUsingStoredProcedure(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> InsertUsingStoredProcedure<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        Action<RelationalModelBuilderTest.TestStoredProcedureBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.InsertUsingStoredProcedure(
                    b => buildAction(new RelationalModelBuilderTest.GenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.InsertUsingStoredProcedure(
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> InsertUsingStoredProcedure<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestStoredProcedureBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.InsertUsingStoredProcedure(
                    name,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.InsertUsingStoredProcedure(
                    name,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> InsertUsingStoredProcedure<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestStoredProcedureBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.InsertUsingStoredProcedure(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.InsertUsingStoredProcedure(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> InsertUsingStoredProcedure<TOwnerEntity,
        TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        Action<RelationalModelBuilderTest.TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.InsertUsingStoredProcedure(
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.InsertUsingStoredProcedure(
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> InsertUsingStoredProcedure<TOwnerEntity,
        TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.InsertUsingStoredProcedure(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.InsertUsingStoredProcedure(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> InsertUsingStoredProcedure<TOwnerEntity,
        TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.InsertUsingStoredProcedure(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.InsertUsingStoredProcedure(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> DeleteUsingStoredProcedure<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        Action<RelationalModelBuilderTest.TestStoredProcedureBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.DeleteUsingStoredProcedure(
                    b => buildAction(new RelationalModelBuilderTest.GenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.DeleteUsingStoredProcedure(
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> DeleteUsingStoredProcedure<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestStoredProcedureBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.DeleteUsingStoredProcedure(
                    name,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.DeleteUsingStoredProcedure(
                    name,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> DeleteUsingStoredProcedure<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestStoredProcedureBuilder<TEntity>> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.DeleteUsingStoredProcedure(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.DeleteUsingStoredProcedure(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestStoredProcedureBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> DeleteUsingStoredProcedure<TOwnerEntity,
        TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        Action<RelationalModelBuilderTest.TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.DeleteUsingStoredProcedure(
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.DeleteUsingStoredProcedure(
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> DeleteUsingStoredProcedure<TOwnerEntity,
        TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        Action<RelationalModelBuilderTest.TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.DeleteUsingStoredProcedure(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.DeleteUsingStoredProcedure(
                    name,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> DeleteUsingStoredProcedure<TOwnerEntity,
        TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>> buildAction)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.DeleteUsingStoredProcedure(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.GenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.DeleteUsingStoredProcedure(
                    name, schema,
                    b => buildAction(
                        new RelationalModelBuilderTest.NonGenericTestOwnedNavigationStoredProcedureBuilder<TOwnerEntity, TDependentEntity>(
                            b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToJson<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.ToJson();
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToJson();
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> ToJson<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string? jsonColumnName)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.ToJson(jsonColumnName);
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.ToJson(jsonColumnName);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> HasJsonPropertyName<TOwnerEntity,
        TDependentEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TDependentEntity> builder,
        string? name)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.HasJsonPropertyName(name);
                break;
            case IInfrastructure<OwnedNavigationBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasJsonPropertyName(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnershipBuilder<TOwnerEntity, TDependentEntity> HasConstraintName<TOwnerEntity, TDependentEntity>(
        this ModelBuilderTest.TestOwnershipBuilder<TOwnerEntity, TDependentEntity> builder,
        string name)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnershipBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.HasConstraintName(name);
                break;
            case IInfrastructure<OwnershipBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasConstraintName(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestReferenceReferenceBuilder<TOwnerEntity, TDependentEntity> HasConstraintName
        <TOwnerEntity, TDependentEntity>(
            this ModelBuilderTest.TestReferenceReferenceBuilder<TOwnerEntity, TDependentEntity> builder,
            string name)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<ReferenceReferenceBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.HasConstraintName(name);
                break;
            case IInfrastructure<ReferenceReferenceBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasConstraintName(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestReferenceCollectionBuilder<TOwnerEntity, TDependentEntity> HasConstraintName
        <TOwnerEntity, TDependentEntity>(
            this ModelBuilderTest.TestReferenceCollectionBuilder<TOwnerEntity, TDependentEntity> builder,
            string name)
        where TOwnerEntity : class
        where TDependentEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<ReferenceCollectionBuilder<TOwnerEntity, TDependentEntity>> genericBuilder:
                genericBuilder.Instance.HasConstraintName(name);
                break;
            case IInfrastructure<ReferenceCollectionBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasConstraintName(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestIndexBuilder<TEntity> HasFilter<TEntity>(
        this ModelBuilderTest.TestIndexBuilder<TEntity> builder,
        string? filterExpression)
    {
        switch (builder)
        {
            case IInfrastructure<IndexBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.HasFilter(filterExpression);
                break;
            case IInfrastructure<IndexBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasFilter(filterExpression);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestIndexBuilder<TEntity> HasName<TEntity>(
        this ModelBuilderTest.TestIndexBuilder<TEntity> builder,
        string name)
    {
        switch (builder)
        {
            case IInfrastructure<KeyBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.HasName(name);
                break;
            case IInfrastructure<KeyBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasName(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestKeyBuilder<TEntity> HasName<TEntity>(
        this ModelBuilderTest.TestKeyBuilder<TEntity> builder,
        string name)
    {
        switch (builder)
        {
            case IInfrastructure<KeyBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.HasName(name);
                break;
            case IInfrastructure<KeyBuilder> nonGenericBuilder:
                nonGenericBuilder.Instance.HasName(name);
                break;
        }

        return builder;
    }
}
