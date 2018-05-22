// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class ModelBuilderGenericRelationshipStringTest : ModelBuilderGenericTest
    {
        public class GenericOneToManyString : OneToManyTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new GenericStringTestModelBuilder(testHelpers);
        }

        public class GenericManyToOneString : ManyToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new GenericStringTestModelBuilder(testHelpers);
        }

        public class GenericOneToOneString : OneToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new GenericStringTestModelBuilder(testHelpers);
        }

        private class GenericStringTestModelBuilder : TestModelBuilder
        {
            public GenericStringTestModelBuilder(TestHelpers testHelpers)
                : base(testHelpers)
            {
            }

            public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
                => new GenericStringTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity<TEntity>());

            public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
            {
                ModelBuilder.Entity<TEntity>(entityTypeBuilder =>
                    buildAction(new GenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
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

            public override string GetDisplayName(Type entityType) => entityType.FullName;
        }

        private class GenericStringTestEntityTypeBuilder<TEntity> : GenericTestEntityTypeBuilder<TEntity>
            where TEntity : class
        {
            public GenericStringTestEntityTypeBuilder(EntityTypeBuilder<TEntity> entityTypeBuilder)
                : base(entityTypeBuilder)
            {
            }

            protected override TestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder<TEntity> entityTypeBuilder)
                => new GenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder);

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression)
                => new GenericStringTestReferenceOwnershipBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.OwnsOne(navigationExpression));

            public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression,
                Action<TestReferenceOwnershipBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(EntityTypeBuilder.OwnsOne(
                    navigationExpression,
                    r => buildAction(new GenericStringTestReferenceOwnershipBuilder<TEntity, TRelatedEntity>(r))));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression = null)
                => new GenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.HasOne<TRelatedEntity>(navigationExpression?.GetPropertyAccess()?.Name));

            public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression = null)
                => new GenericStringTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasMany(navigationExpression));
        }

        private class GenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity> : GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericStringTestReferenceNavigationBuilder(ReferenceNavigationBuilder<TEntity, TRelatedEntity> referenceNavigationBuilder)
                : base(referenceNavigationBuilder)
            {
            }

            public override TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(
                Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> navigationExpression = null)
                => new GenericStringTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(
                    ReferenceNavigationBuilder.WithMany(navigationExpression?.GetPropertyAccess().Name));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null)
                => new GenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                    ReferenceNavigationBuilder.WithOne(navigationExpression?.GetPropertyAccess().Name));
        }

        private class GenericStringTestCollectionNavigationBuilder<TEntity, TRelatedEntity>
            : GenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericStringTestCollectionNavigationBuilder(CollectionNavigationBuilder<TEntity, TRelatedEntity> collectionNavigationBuilder)
                : base(collectionNavigationBuilder)
            {
            }

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null)
                => new GenericStringTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                    CollectionNavigationBuilder.WithOne(navigationExpression?.GetPropertyAccess().Name));
        }

        private class GenericStringTestReferenceCollectionBuilder<TEntity, TRelatedEntity> : GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericStringTestReferenceCollectionBuilder(ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder)
                : base(referenceCollectionBuilder)
            {
            }

            protected override GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity> Wrap(ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder)
                => new GenericStringTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(referenceCollectionBuilder);

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(
                Expression<Func<TRelatedEntity, object>> foreignKeyExpression)
                => Wrap(ReferenceCollectionBuilder.HasForeignKey(foreignKeyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
                Expression<Func<TEntity, object>> keyExpression)
                => Wrap(ReferenceCollectionBuilder.HasPrincipalKey(keyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));
        }

        private class GenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>
            : GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericStringTestReferenceReferenceBuilder(ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder)
                : base(referenceReferenceBuilder)
            {
            }

            protected override GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(
                ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder)
                => new GenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder);

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                => Wrap(
                    ReferenceReferenceBuilder.HasForeignKey(
                        typeof(TDependentEntity).FullName,
                        foreignKeyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(Expression<Func<TPrincipalEntity, object>> keyExpression)
                => Wrap(
                    ReferenceReferenceBuilder.HasPrincipalKey(
                        typeof(TPrincipalEntity).FullName,
                        keyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));
        }

        private class GenericStringTestReferenceOwnershipBuilder<TEntity, TRelatedEntity>
            : GenericTestReferenceOwnershipBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericStringTestReferenceOwnershipBuilder(ReferenceOwnershipBuilder<TEntity, TRelatedEntity> referenceOwnershipBuilder)
                : base(referenceOwnershipBuilder)
            {
            }

            protected override GenericTestReferenceOwnershipBuilder<TNewEntity, TNewRelatedEntity> Wrap<TNewEntity, TNewRelatedEntity>(
                ReferenceOwnershipBuilder<TNewEntity, TNewRelatedEntity> referenceOwnershipBuilder)
                => new GenericStringTestReferenceOwnershipBuilder<TNewEntity, TNewRelatedEntity>(referenceOwnershipBuilder);

            public override TestReferenceNavigationBuilder<TRelatedEntity, TNewRelatedEntity> HasOne<TNewRelatedEntity>(
                Expression<Func<TRelatedEntity, TNewRelatedEntity>> navigationExpression = null)
                => new GenericStringTestReferenceNavigationBuilder<TRelatedEntity, TNewRelatedEntity>(
                    ReferenceOwnershipBuilder.HasOne(navigationExpression));
        }
    }
}
