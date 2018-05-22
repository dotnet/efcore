// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class ModelBuilderGenericRelationshipTypeStringTest : ModelBuilderGenericRelationshipTypeTest
    {
        public class GenericOneToOneTypeString : GenericOneToOneType
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new GenericTypeTestModelBuilder(testHelpers);
        }

        private class GenericTypeTestModelBuilder : TestModelBuilder
        {
            public GenericTypeTestModelBuilder(TestHelpers testHelpers)
                : base(testHelpers)
            {
            }

            public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
                => new GenericTypeTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity<TEntity>());

            public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
            {
                ModelBuilder.Entity<TEntity>(
                    entityTypeBuilder =>
                        buildAction(new GenericTypeTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
                return this;
            }

            public override TestOwnedEntityTypeBuilder<TEntity> Owned<TEntity>()
                => new GenericTestOwnedEntityTypeBuilder<TEntity>(ModelBuilder.Owned<TEntity>());

            public override TestQueryTypeBuilder<TQuery> Query<TQuery>()
                => new GenericTestQueryTypeBuilder<TQuery>(ModelBuilder.Query<TQuery>());

            public override TestModelBuilder Ignore<TEntity>()
            {
                ModelBuilder.Ignore<TEntity>();
                return this;
            }
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

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression)
                => new GenericTypeTestReferenceOwnershipBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.OwnsOne(navigationExpression));

            public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression,
                Action<TestReferenceOwnershipBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(
                    EntityTypeBuilder.OwnsOne(
                        navigationExpression,
                        r => buildAction(new GenericTypeTestReferenceOwnershipBuilder<TEntity, TRelatedEntity>(r))));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression = null)
                => new GenericTypeTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasOne(navigationExpression));
        }

        private class GenericTypeTestReferenceNavigationBuilder<TEntity, TRelatedEntity>
            : GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTypeTestReferenceNavigationBuilder(ReferenceNavigationBuilder<TEntity, TRelatedEntity> referenceNavigationBuilder)
                : base(referenceNavigationBuilder)
            {
            }

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null)
                => new GenericTypeTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(ReferenceNavigationBuilder.WithOne(navigationExpression?.GetPropertyAccess().Name));
        }

        private class GenericTypeTestReferenceReferenceBuilder<TEntity, TRelatedEntity>
            : GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTypeTestReferenceReferenceBuilder(ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder)
                : base(referenceReferenceBuilder)
            {
            }

            protected override GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(
                ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder)
                => new GenericTypeTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder);

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
                params string[] foreignKeyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.HasForeignKey(typeof(TDependentEntity), foreignKeyPropertyNames));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                params string[] keyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.HasPrincipalKey(typeof(TPrincipalEntity), keyPropertyNames));
        }

        private class GenericTypeTestReferenceOwnershipBuilder<TEntity, TRelatedEntity>
            : GenericTestReferenceOwnershipBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTypeTestReferenceOwnershipBuilder(ReferenceOwnershipBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder)
                : base(referenceOwnershipBuilder)
            {
            }

            protected override GenericTestReferenceOwnershipBuilder<TNewEntity, TNewRelatedEntity> Wrap<TNewEntity, TNewRelatedEntity>(
                ReferenceOwnershipBuilder<TNewEntity, TNewRelatedEntity> referenceOwnershipBuilder)
                => new GenericTypeTestReferenceOwnershipBuilder<TNewEntity, TNewRelatedEntity>(referenceOwnershipBuilder);

            public override TestReferenceNavigationBuilder<TRelatedEntity, TNewRelatedEntity> HasOne<TNewRelatedEntity>(
                Expression<Func<TRelatedEntity, TNewRelatedEntity>> navigationExpression = null)
                => new GenericTypeTestReferenceNavigationBuilder<TRelatedEntity, TNewRelatedEntity>(
                    ReferenceOwnershipBuilder.HasOne(navigationExpression));
        }
    }
}
