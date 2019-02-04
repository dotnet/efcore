// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public static class RelationalTestModelBuilderExtensions
    {
        public static ModelBuilderTest.TestPropertyBuilder<TProperty> HasColumnName<TProperty>(
            this ModelBuilderTest.TestPropertyBuilder<TProperty> builder, string name)
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
            this ModelBuilderTest.TestPropertyBuilder<TProperty> builder, string typeName)
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
            this ModelBuilderTest.TestPropertyBuilder<TProperty> builder, string sql)
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
            this ModelBuilderTest.TestPropertyBuilder<TProperty> builder, string sql)
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
            this ModelBuilderTest.TestPropertyBuilder<TProperty> builder, object value)
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
            this ModelBuilderTest.TestPropertyBuilder<TProperty> builder, bool fixedLength = true)
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
            this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder, string name)
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
            this ModelBuilderTest.TestEntityTypeBuilder<TEntity> builder, string name, string schema)
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

        public static ModelBuilderTest.TestOwnedNavigationBuilder<TEntity, TRelatedEntity> ToTable<TEntity, TRelatedEntity>(
            this ModelBuilderTest.TestOwnedNavigationBuilder<TEntity, TRelatedEntity> builder, string name)
            where TEntity : class
            where TRelatedEntity : class
        {
            switch (builder)
            {
                case IInfrastructure<OwnedNavigationBuilder<TEntity, TRelatedEntity>> genericBuilder:
                    genericBuilder.Instance.ToTable(name);
                    break;
                case IInfrastructure<OwnedNavigationBuilder> nongenericBuilder:
                    nongenericBuilder.Instance.ToTable(name);
                    break;
            }

            return builder;
        }

        public static ModelBuilderTest.TestOwnedNavigationBuilder<TEntity, TRelatedEntity> ToTable<TEntity, TRelatedEntity>(
            this ModelBuilderTest.TestOwnedNavigationBuilder<TEntity, TRelatedEntity> builder, string name, string schema)
            where TEntity : class
            where TRelatedEntity : class
        {
            switch (builder)
            {
                case IInfrastructure<OwnedNavigationBuilder<TEntity, TRelatedEntity>> genericBuilder:
                    genericBuilder.Instance.ToTable(name, schema);
                    break;
                case IInfrastructure<OwnedNavigationBuilder> nongenericBuilder:
                    nongenericBuilder.Instance.ToTable(name, schema);
                    break;
            }

            return builder;
        }

        public static ModelBuilderTest.TestOwnershipBuilder<TEntity, TRelatedEntity> HasConstraintName<TEntity, TRelatedEntity>(
            this ModelBuilderTest.TestOwnershipBuilder<TEntity, TRelatedEntity> builder,
            string name)
            where TEntity : class
            where TRelatedEntity : class
        {
            switch (builder)
            {
                case IInfrastructure<OwnershipBuilder<TEntity, TRelatedEntity>> genericBuilder:
                    genericBuilder.Instance.HasConstraintName(name);
                    break;
                case IInfrastructure<OwnershipBuilder> nongenericBuilder:
                    nongenericBuilder.Instance.HasConstraintName(name);
                    break;
            }

            return builder;
        }

        public static ModelBuilderTest.TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasConstraintName<TEntity, TRelatedEntity>(
            this ModelBuilderTest.TestReferenceReferenceBuilder<TEntity, TRelatedEntity> builder,
            string name)
            where TEntity : class
            where TRelatedEntity : class
        {
            switch (builder)
            {
                case IInfrastructure<ReferenceReferenceBuilder<TEntity, TRelatedEntity>> genericBuilder:
                    genericBuilder.Instance.HasConstraintName(name);
                    break;
                case IInfrastructure<ReferenceReferenceBuilder> nongenericBuilder:
                    nongenericBuilder.Instance.HasConstraintName(name);
                    break;
            }

            return builder;
        }

        public static ModelBuilderTest.TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasConstraintName<TEntity, TRelatedEntity>(
            this ModelBuilderTest.TestReferenceCollectionBuilder<TEntity, TRelatedEntity> builder,
            string name)
            where TEntity : class
            where TRelatedEntity : class
        {
            switch (builder)
            {
                case IInfrastructure<ReferenceCollectionBuilder<TEntity, TRelatedEntity>> genericBuilder:
                    genericBuilder.Instance.HasConstraintName(name);
                    break;
                case IInfrastructure<ReferenceCollectionBuilder> nongenericBuilder:
                    nongenericBuilder.Instance.HasConstraintName(name);
                    break;
            }

            return builder;
        }

        public static ModelBuilderTest.TestIndexBuilder HasFilter(
            this ModelBuilderTest.TestIndexBuilder builder, string filterExpression)
        {
            var indexBuilder = builder.GetInfrastructure();
            indexBuilder.HasFilter(filterExpression);
            return builder;
        }

        public static ModelBuilderTest.TestIndexBuilder HasName(
            this ModelBuilderTest.TestIndexBuilder builder, string name)
        {
            var indexBuilder = builder.GetInfrastructure();
            indexBuilder.HasName(name);
            return builder;
        }

        public static ModelBuilderTest.TestKeyBuilder HasName(
            this ModelBuilderTest.TestKeyBuilder builder, string name)
        {
            var keyBuilder = builder.GetInfrastructure();
            keyBuilder.HasName(name);
            return builder;
        }
    }
}
