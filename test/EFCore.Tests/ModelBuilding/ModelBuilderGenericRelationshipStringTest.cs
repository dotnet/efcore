// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        public class GenericOwnedTypesString : OwnedTypesTestBase
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
                ModelBuilder.Entity<TEntity>(
                    entityTypeBuilder =>
                        buildAction(new GenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
                return this;
            }

            public override TestOwnedEntityTypeBuilder<TEntity> Owned<TEntity>()
                => new GenericTestOwnedEntityTypeBuilder<TEntity>(ModelBuilder.Owned<TEntity>());

            public override TestQueryTypeBuilder<TQuery> Query<TQuery>()
                => new GenericStringTestQueryTypeBuilder<TQuery>(ModelBuilder.Query<TQuery>());

            public override TestModelBuilder Query<TQuery>(Action<TestQueryTypeBuilder<TQuery>> buildAction)
            {
                ModelBuilder.Query<TQuery>(
                    queryTypeBuilder => buildAction(new GenericStringTestQueryTypeBuilder<TQuery>(queryTypeBuilder)));
                return this;
            }

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

            public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression)
                => new GenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.OwnsOne<TRelatedEntity>(navigationExpression?.GetPropertyAccess()?.GetSimpleMemberName()));

            public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(
                    EntityTypeBuilder.OwnsOne<TRelatedEntity>(
                        navigationExpression?.GetPropertyAccess()?.GetSimpleMemberName(),
                        r => buildAction(new GenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

            public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression)
                => new GenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.OwnsMany<TRelatedEntity>(navigationExpression?.GetPropertyAccess()?.GetSimpleMemberName()));

            public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(
                    EntityTypeBuilder.OwnsMany<TRelatedEntity>(
                        navigationExpression?.GetPropertyAccess()?.GetSimpleMemberName(),
                        r => buildAction(new GenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression = null)
                => new GenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.HasOne<TRelatedEntity>(navigationExpression?.GetPropertyAccess()?.GetSimpleMemberName()));

            public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression = null)
                => new GenericStringTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.HasMany<TRelatedEntity>(navigationExpression?.GetPropertyAccess()?.GetSimpleMemberName()));
        }

        private class GenericStringTestQueryTypeBuilder<TQuery> : GenericTestQueryTypeBuilder<TQuery>
            where TQuery : class
        {
            public GenericStringTestQueryTypeBuilder(QueryTypeBuilder<TQuery> queryTypeBuilder)
                : base(queryTypeBuilder)
            {
            }

            protected override TestQueryTypeBuilder<TQuery> Wrap(QueryTypeBuilder<TQuery> queryTypeBuilder)
                => new GenericStringTestQueryTypeBuilder<TQuery>(queryTypeBuilder);

            public override TestReferenceNavigationBuilder<TQuery, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TQuery, TRelatedEntity>> navigationExpression = null)
                => new GenericStringTestReferenceNavigationBuilder<TQuery, TRelatedEntity>(QueryTypeBuilder.HasOne(navigationExpression));
        }

        private class GenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity> :
            GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericStringTestReferenceNavigationBuilder(
                ReferenceNavigationBuilder<TEntity, TRelatedEntity> referenceNavigationBuilder)
                : base(referenceNavigationBuilder)
            {
            }

            public override TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(
                Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> navigationExpression = null)
                => new GenericStringTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(
                    ReferenceNavigationBuilder.WithMany(navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null)
                => new GenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                    ReferenceNavigationBuilder.WithOne(navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));
        }

        private class GenericStringTestCollectionNavigationBuilder<TEntity, TRelatedEntity>
            : GenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericStringTestCollectionNavigationBuilder(
                CollectionNavigationBuilder<TEntity, TRelatedEntity> collectionNavigationBuilder)
                : base(collectionNavigationBuilder)
            {
            }

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null)
                => new GenericStringTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                    CollectionNavigationBuilder.WithOne(navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));
        }

        private class GenericStringTestReferenceCollectionBuilder<TEntity, TRelatedEntity> : GenericTestReferenceCollectionBuilder<TEntity,
            TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericStringTestReferenceCollectionBuilder(
                ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder)
                : base(referenceCollectionBuilder)
            {
            }

            protected override GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity> Wrap(
                ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder)
                => new GenericStringTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(referenceCollectionBuilder);

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(
                Expression<Func<TRelatedEntity, object>> foreignKeyExpression)
                => Wrap(
                    ReferenceCollectionBuilder.HasForeignKey(
                        foreignKeyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
                Expression<Func<TEntity, object>> keyExpression)
                => Wrap(
                    ReferenceCollectionBuilder.HasPrincipalKey(
                        keyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));
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

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
                Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                => Wrap(
                    ReferenceReferenceBuilder.HasForeignKey(
                        typeof(TDependentEntity).FullName,
                        foreignKeyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
                params string[] foreignKeyPropertyNames)
                => Wrap(
                    ReferenceReferenceBuilder.HasForeignKey(
                        typeof(TDependentEntity).FullName, foreignKeyPropertyNames));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                Expression<Func<TPrincipalEntity, object>> keyExpression)
                => Wrap(
                    ReferenceReferenceBuilder.HasPrincipalKey(
                        typeof(TPrincipalEntity).FullName,
                        keyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                params string[] keyPropertyNames)
                => Wrap(
                    ReferenceReferenceBuilder.HasPrincipalKey(
                        typeof(TPrincipalEntity).FullName, keyPropertyNames));
        }

        private class GenericStringTestOwnedNavigationBuilder<TEntity, TDependentEntity>
            : GenericTestOwnedNavigationBuilder<TEntity, TDependentEntity>
            where TEntity : class
            where TDependentEntity : class
        {
            public GenericStringTestOwnedNavigationBuilder(OwnedNavigationBuilder<TEntity, TDependentEntity> ownedNavigationBuilder)
                : base(ownedNavigationBuilder)
            {
            }

            protected override GenericTestOwnedNavigationBuilder<TNewEntity, TNewRelatedEntity> Wrap<TNewEntity, TNewRelatedEntity>(
                OwnedNavigationBuilder<TNewEntity, TNewRelatedEntity> referenceOwnershipBuilder)
                => new GenericStringTestOwnedNavigationBuilder<TNewEntity, TNewRelatedEntity>(referenceOwnershipBuilder);

            public override TestOwnershipBuilder<TEntity, TDependentEntity> WithOwner(
                Expression<Func<TDependentEntity, TEntity>> referenceExpression)
                => new GenericTestOwnershipBuilder<TEntity, TDependentEntity>(
                    OwnedNavigationBuilder.WithOwner(referenceExpression?.GetPropertyAccess().GetSimpleMemberName()));

            public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
                Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression)
                => Wrap(OwnedNavigationBuilder.OwnsOne<TNewDependentEntity>(navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
                Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
                => Wrap(OwnedNavigationBuilder.OwnsOne<TNewDependentEntity>(navigationExpression?.GetPropertyAccess().GetSimpleMemberName(),
                    r => buildAction(Wrap(r))));

            public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression)
                => Wrap(OwnedNavigationBuilder.OwnsMany<TNewDependentEntity>(navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
                Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
                => Wrap(OwnedNavigationBuilder.OwnsMany<TNewDependentEntity>(navigationExpression?.GetPropertyAccess().GetSimpleMemberName(),
                    r => buildAction(Wrap(r))));

            public override TestReferenceNavigationBuilder<TDependentEntity, TNewRelatedEntity> HasOne<TNewRelatedEntity>(
                Expression<Func<TDependentEntity, TNewRelatedEntity>> navigationExpression = null)
                => new GenericStringTestReferenceNavigationBuilder<TDependentEntity, TNewRelatedEntity>(
                    OwnedNavigationBuilder.HasOne<TNewRelatedEntity>(navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));
        }
    }
}
