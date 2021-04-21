// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
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
    }
}
