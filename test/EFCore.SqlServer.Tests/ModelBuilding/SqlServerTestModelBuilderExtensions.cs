// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
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

        public static ModelBuilderTest.TestOwnedNavigationBuilder<TEntity, TDependentEntity> ForSqlServerIsMemoryOptimized<TEntity,
            TDependentEntity>(
            this ModelBuilderTest.TestOwnedNavigationBuilder<TEntity, TDependentEntity> builder, bool memoryOptimized = true)
            where TEntity : class
            where TDependentEntity : class
        {
            switch (builder)
            {
                case IInfrastructure<OwnedNavigationBuilder<TEntity, TDependentEntity>> genericBuilder:
                    genericBuilder.Instance.ForSqlServerIsMemoryOptimized(memoryOptimized);
                    break;
                case IInfrastructure<OwnedNavigationBuilder> nongenericBuilder:
                    nongenericBuilder.Instance.ForSqlServerIsMemoryOptimized(memoryOptimized);
                    break;
            }

            return builder;
        }
    }
}
