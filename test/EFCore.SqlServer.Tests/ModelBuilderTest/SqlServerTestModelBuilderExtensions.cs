// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Tests;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.SqlServer.Tests
{
    public static class SqlServerTestModelBuilderExtensions
    {
        public static ModelBuilderTest.TestIndexBuilder ForSqlServerIsClustered(
            this ModelBuilderTest.TestIndexBuilder builder, bool clustered = true)
        {
            var indexBuilder = builder.GetInfrastructure();
            indexBuilder.ForSqlServerIsClustered(clustered);
            return builder;
        }

        public static ModelBuilderTest.TestPropertyBuilder<TProperty> ForSqlServerHasColumnName<TProperty>(
            this ModelBuilderTest.TestPropertyBuilder<TProperty> builder, string name)
        {
            var genericBuilder = (builder as IInfrastructure<PropertyBuilder<TProperty>>)?.Instance;
            genericBuilder?.ForSqlServerHasColumnName(name);

            return builder;
        }

        public static ModelBuilderTest.TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> ForSqlServerToTable<TEntity, TRelatedEntity>(
            this ModelBuilderTest.TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> builder, string name)
            where TEntity : class
            where TRelatedEntity : class
        {
            var genericBuilder = (builder as IInfrastructure<ReferenceOwnershipBuilder<TEntity, TRelatedEntity>>)?.Instance;
            genericBuilder?.ForSqlServerToTable(name);

            return builder;
        }
    }
}
