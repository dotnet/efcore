// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public static class RelationalTestModelBuilderExtensions
{
    public static ModelBuilderTest.TestPropertyBuilder<TProperty> HasColumnName<TProperty>(
        this ModelBuilderTest.TestPropertyBuilder<TProperty> builder,
        string name)
    {
        switch (builder)
        {
            case IInfrastructure<PropertyBuilder<TProperty>> genericBuilder:
                genericBuilder.Instance.HasColumnName(name);
                break;
            case IInfrastructure<PropertyBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasColumnName(name);
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
            case IInfrastructure<PropertyBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasColumnType(typeName);
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
            case IInfrastructure<PropertyBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasDefaultValueSql(sql);
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
            case IInfrastructure<PropertyBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasComputedColumnSql(sql);
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
            case IInfrastructure<PropertyBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasDefaultValue(value);
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
            case IInfrastructure<PropertyBuilder> nongenericBuilder:
                nongenericBuilder.Instance.IsFixedLength(fixedLength);
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
            case IInfrastructure<EntityTypeBuilder> nongenericBuilder:
                nongenericBuilder.Instance.ToTable(name);
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
            case IInfrastructure<EntityTypeBuilder> nongenericBuilder:
                nongenericBuilder.Instance.ToTable(name, schema);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> ToTable<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string? name,
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
            case IInfrastructure<EntityTypeBuilder> nongenericBuilder:
                nongenericBuilder.Instance.ToTable(
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
            case IInfrastructure<EntityTypeBuilder> nongenericBuilder:
                nongenericBuilder.Instance.ToTable(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestTableBuilder<TEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToTable<TOwnerEntity, TRelatedEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> builder,
        string? name)
        where TOwnerEntity : class
        where TRelatedEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(name);
                break;
            case IInfrastructure<OwnedNavigationBuilder> nongenericBuilder:
                nongenericBuilder.Instance.ToTable(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToTable<TOwnerEntity, TRelatedEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> builder,
        string name,
        string? schema)
        where TOwnerEntity : class
        where TRelatedEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(name, schema);
                break;
            case IInfrastructure<OwnedNavigationBuilder> nongenericBuilder:
                nongenericBuilder.Instance.ToTable(name, schema);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToTable<TOwnerEntity, TRelatedEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> builder,
        string? name,
        Action<RelationalModelBuilderTest.TestOwnedNavigationTableBuilder<TRelatedEntity>> buildAction)
        where TOwnerEntity : class
        where TRelatedEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(
                    name,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestOwnedNavigationTableBuilder<TRelatedEntity>(b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nongenericBuilder:
                nongenericBuilder.Instance.ToTable(
                    name,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestOwnedNavigationTableBuilder<TRelatedEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> ToTable<TOwnerEntity, TRelatedEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> builder,
        string name,
        string? schema,
        Action<RelationalModelBuilderTest.TestOwnedNavigationTableBuilder<TRelatedEntity>> buildAction)
        where TOwnerEntity : class
        where TRelatedEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>> genericBuilder:
                genericBuilder.Instance.ToTable(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.GenericTestOwnedNavigationTableBuilder<TRelatedEntity>(b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nongenericBuilder:
                nongenericBuilder.Instance.ToTable(
                    name, schema,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestOwnedNavigationTableBuilder<TRelatedEntity>(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> HasCheckConstraint<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        string? sql)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.HasCheckConstraint(name, sql);
                break;
            case IInfrastructure<EntityTypeBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasCheckConstraint(name, sql);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestEntityTypeBuilder<TEntity> HasCheckConstraint<TEntity>(
        this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder,
        string name,
        string sql,
        Action<RelationalModelBuilderTest.TestCheckConstraintBuilder> buildAction)
        where TEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<EntityTypeBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.HasCheckConstraint(
                    name, sql,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestCheckConstraintBuilder(b)));
                break;
            case IInfrastructure<EntityTypeBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasCheckConstraint(
                    name, sql,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestCheckConstraintBuilder(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> HasCheckConstraint<TOwnerEntity,
        TRelatedEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> builder,
        string name,
        string? sql)
        where TOwnerEntity : class
        where TRelatedEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>> genericBuilder:
                genericBuilder.Instance.HasCheckConstraint(name, sql);
                break;
            case IInfrastructure<OwnedNavigationBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasCheckConstraint(name, sql);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> HasCheckConstraint<TOwnerEntity,
        TRelatedEntity>(
        this ModelBuilderTest.TestOwnedNavigationBuilder<TOwnerEntity, TRelatedEntity> builder,
        string name,
        string sql,
        Action<RelationalModelBuilderTest.TestCheckConstraintBuilder> buildAction)
        where TOwnerEntity : class
        where TRelatedEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnedNavigationBuilder<TOwnerEntity, TRelatedEntity>> genericBuilder:
                genericBuilder.Instance.HasCheckConstraint(
                    name, sql,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestCheckConstraintBuilder(b)));
                break;
            case IInfrastructure<OwnedNavigationBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasCheckConstraint(
                    name, sql,
                    b => buildAction(new RelationalModelBuilderTest.NonGenericTestCheckConstraintBuilder(b)));
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestOwnershipBuilder<TOwnerEntity, TRelatedEntity> HasConstraintName<TOwnerEntity, TRelatedEntity>(
        this ModelBuilderTest.TestOwnershipBuilder<TOwnerEntity, TRelatedEntity> builder,
        string name)
        where TOwnerEntity : class
        where TRelatedEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<OwnershipBuilder<TOwnerEntity, TRelatedEntity>> genericBuilder:
                genericBuilder.Instance.HasConstraintName(name);
                break;
            case IInfrastructure<OwnershipBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasConstraintName(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestReferenceReferenceBuilder<TOwnerEntity, TRelatedEntity> HasConstraintName<TOwnerEntity,
        TRelatedEntity>(
        this ModelBuilderTest.TestReferenceReferenceBuilder<TOwnerEntity, TRelatedEntity> builder,
        string name)
        where TOwnerEntity : class
        where TRelatedEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<ReferenceReferenceBuilder<TOwnerEntity, TRelatedEntity>> genericBuilder:
                genericBuilder.Instance.HasConstraintName(name);
                break;
            case IInfrastructure<ReferenceReferenceBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasConstraintName(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestReferenceCollectionBuilder<TOwnerEntity, TRelatedEntity> HasConstraintName<TOwnerEntity,
        TRelatedEntity>(
        this ModelBuilderTest.TestReferenceCollectionBuilder<TOwnerEntity, TRelatedEntity> builder,
        string name)
        where TOwnerEntity : class
        where TRelatedEntity : class
    {
        switch (builder)
        {
            case IInfrastructure<ReferenceCollectionBuilder<TOwnerEntity, TRelatedEntity>> genericBuilder:
                genericBuilder.Instance.HasConstraintName(name);
                break;
            case IInfrastructure<ReferenceCollectionBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasConstraintName(name);
                break;
        }

        return builder;
    }

    public static ModelBuilderTest.TestIndexBuilder<TEntity> HasFilter<TEntity>(
        this ModelBuilderTest.TestIndexBuilder<TEntity> builder,
        string filterExpression)
    {
        switch (builder)
        {
            case IInfrastructure<IndexBuilder<TEntity>> genericBuilder:
                genericBuilder.Instance.HasFilter(filterExpression);
                break;
            case IInfrastructure<IndexBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasFilter(filterExpression);
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
            case IInfrastructure<KeyBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasName(name);
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
            case IInfrastructure<KeyBuilder> nongenericBuilder:
                nongenericBuilder.Instance.HasName(name);
                break;
        }

        return builder;
    }
}
