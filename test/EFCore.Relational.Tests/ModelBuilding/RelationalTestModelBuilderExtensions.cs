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
            var genericBuilder = (builder as IInfrastructure<PropertyBuilder<TProperty>>)?.Instance;
            if (genericBuilder != null)
            {
                genericBuilder.HasColumnName(name);
            }
            else
            {
                (builder as IInfrastructure<PropertyBuilder>).Instance.HasColumnName(name);
            }

            return builder;
        }

        public static ModelBuilderTest.TestPropertyBuilder<TProperty> HasDefaultValueSql<TProperty>(
            this ModelBuilderTest.TestPropertyBuilder<TProperty> builder, string sql)
        {
            var genericBuilder = (builder as IInfrastructure<PropertyBuilder<TProperty>>)?.Instance;
            if (genericBuilder != null)
            {
                genericBuilder.HasDefaultValueSql(sql);
            }
            else
            {
                (builder as IInfrastructure<PropertyBuilder>).Instance.HasDefaultValueSql(sql);
            }

            return builder;
        }

        public static ModelBuilderTest.TestPropertyBuilder<TProperty> HasComputedColumnSql<TProperty>(
            this ModelBuilderTest.TestPropertyBuilder<TProperty> builder, string sql)
        {
            var genericBuilder = (builder as IInfrastructure<PropertyBuilder<TProperty>>)?.Instance;
            if (genericBuilder != null)
            {
                genericBuilder.HasComputedColumnSql(sql);
            }
            else
            {
                (builder as IInfrastructure<PropertyBuilder>).Instance.HasComputedColumnSql(sql);
            }

            return builder;
        }

        public static ModelBuilderTest.TestPropertyBuilder<TProperty> HasDefaultValue<TProperty>(
            this ModelBuilderTest.TestPropertyBuilder<TProperty> builder, object value)
        {
            var genericBuilder = (builder as IInfrastructure<PropertyBuilder<TProperty>>)?.Instance;
            if (genericBuilder != null)
            {
                genericBuilder.HasDefaultValue(value);
            }
            else
            {
                (builder as IInfrastructure<PropertyBuilder>).Instance.HasDefaultValue(value);
            }

            return builder;
        }

        public static ModelBuilderTest.TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> ToTable<TEntity, TRelatedEntity>(
            this ModelBuilderTest.TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> builder, string name)
            where TEntity : class
            where TRelatedEntity : class
        {
            var genericBuilder = (builder as IInfrastructure<ReferenceOwnershipBuilder<TEntity, TRelatedEntity>>)?.Instance;
            if (genericBuilder != null)
            {
                genericBuilder.ToTable(name);
            }
            else
            {
                (builder as IInfrastructure<ReferenceOwnershipBuilder>).Instance.ToTable(name);
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
    }
}
