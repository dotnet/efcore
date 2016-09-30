// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Microsoft.EntityFrameworkCore.Tests
{
    public class ModelBuilderGenericRelationshipTypeStringTest : ModelBuilderGenericRelationshipTypeTest
    {
        public class GenericOneToOneTypeString : GenericOneToOneType
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
                => new GenericTypeTestModelBuilder(modelBuilder);
        }

        private class GenericTypeTestModelBuilder : TestModelBuilder
        {
            public GenericTypeTestModelBuilder(ModelBuilder modelBuilder)
                : base(modelBuilder)
            {
            }

            public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
                => new GenericTypeTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity<TEntity>());

            public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
                => new GenericTypeTestModelBuilder(ModelBuilder.Entity<TEntity>(entityTypeBuilder =>
                        buildAction(new GenericTypeTestEntityTypeBuilder<TEntity>(entityTypeBuilder))));

            public override TestModelBuilder Ignore<TEntity>()
                => new GenericTypeTestModelBuilder(ModelBuilder.Ignore<TEntity>());
        }

        private class GenericTypeTestEntityTypeBuilder<TEntity> : GenericTestEntityTypeBuilder<TEntity>
            where TEntity : class
        {
            public GenericTypeTestEntityTypeBuilder(EntityTypeBuilder<TEntity> entityTypeBuilder)
                : base(entityTypeBuilder)
            {
            }

            protected override TestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder<TEntity> entityTypeBuilder)
                => new GenericTypeTestEntityTypeBuilder<TEntity>(entityTypeBuilder);

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(Expression<Func<TEntity, TRelatedEntity>> reference = null)
                => new GenericTypeTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasOne(reference));
        }

        private class GenericTypeTestReferenceNavigationBuilder<TEntity, TRelatedEntity> : GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTypeTestReferenceNavigationBuilder(ReferenceNavigationBuilder<TEntity, TRelatedEntity> referenceNavigationBuilder)
                : base(referenceNavigationBuilder)
            {
            }

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(Expression<Func<TRelatedEntity, TEntity>> reference = null)
                => new GenericTypeTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(ReferenceNavigationBuilder.WithOne(reference?.GetPropertyAccess().Name));
        }

        private class GenericTypeTestReferenceReferenceBuilder<TEntity, TRelatedEntity> : GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTypeTestReferenceReferenceBuilder(ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder)
                : base(referenceReferenceBuilder)
            {
            }

            protected override GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder)
                => new GenericTypeTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder);

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(params string[] foreignKeyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.HasForeignKey(typeof(TDependentEntity), foreignKeyPropertyNames));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(params string[] keyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.HasPrincipalKey(typeof(TPrincipalEntity), keyPropertyNames));
        }
    }
}
