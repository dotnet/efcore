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
    public class ModelBuilderNonGenericUnqualifiedStringTest : ModelBuilderNonGenericTest
    {
        public class NonGenericStringOneToOneType : OneToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new NonGenericStringTestModelBuilder(testHelpers);
        }

        private class NonGenericStringTestModelBuilder : TestModelBuilder
        {
            public NonGenericStringTestModelBuilder(TestHelpers testHelpers)
                : base(testHelpers)
            {
            }

            public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
                => new NonGenericStringTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity(typeof(TEntity)));

            public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
            {
                ModelBuilder.Entity(
                    typeof(TEntity), entityTypeBuilder =>
                        buildAction(new NonGenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
                return this;
            }

            public override TestOwnedEntityTypeBuilder<TEntity> Owned<TEntity>()
                => new NonGenericTestOwnedEntityTypeBuilder<TEntity>(ModelBuilder.Owned(typeof(TEntity)));

            public override TestQueryTypeBuilder<TQuery> Query<TQuery>()
                => new NonGenericStringTestQueryTypeBuilder<TQuery>(ModelBuilder.Query(typeof(TQuery)));

            public override TestModelBuilder Query<TQuery>(Action<TestQueryTypeBuilder<TQuery>> buildAction)
            {
                ModelBuilder.Query(
                    typeof(TQuery), queryTypeBuilder => buildAction(new NonGenericStringTestQueryTypeBuilder<TQuery>(queryTypeBuilder)));
                return this;
            }

            public override TestModelBuilder Ignore<TEntity>()
            {
                ModelBuilder.Ignore(typeof(TEntity));
                return this;
            }
        }

        private class NonGenericStringTestEntityTypeBuilder<TEntity> : NonGenericTestEntityTypeBuilder<TEntity>
            where TEntity : class
        {
            public NonGenericStringTestEntityTypeBuilder(EntityTypeBuilder entityTypeBuilder)
                : base(entityTypeBuilder)
            {
            }

            protected override NonGenericTestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder entityTypeBuilder)
                => new NonGenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder);

            public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression)
                => new NonGenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.OwnsOne(
                        typeof(TRelatedEntity).Name, navigationExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(
                    EntityTypeBuilder.OwnsOne(
                        typeof(TRelatedEntity).Name,
                        navigationExpression.GetPropertyAccess().GetSimpleMemberName(),
                        r => buildAction(new NonGenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

            public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression)
                => new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.OwnsMany(typeof(TRelatedEntity).Name,
                        navigationExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(
                    EntityTypeBuilder.OwnsMany(
                        typeof(TRelatedEntity).Name,
                        navigationExpression.GetPropertyAccess().GetSimpleMemberName(),
                        r => buildAction(new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression = null)
                => new NonGenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.HasOne(
                        typeof(TRelatedEntity).FullName, navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));

            public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression = null)
                => new NonGenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.HasMany(
                        typeof(TRelatedEntity).FullName, navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));
        }

        private class NonGenericStringTestQueryTypeBuilder<TQuery> : NonGenericTestQueryTypeBuilder<TQuery>
            where TQuery : class
        {
            public NonGenericStringTestQueryTypeBuilder(QueryTypeBuilder queryTypeBuilder)
                : base(queryTypeBuilder)
            {
            }

            protected override NonGenericTestQueryTypeBuilder<TQuery> Wrap(QueryTypeBuilder queryTypeBuilder)
                => new NonGenericStringTestQueryTypeBuilder<TQuery>(queryTypeBuilder);

            public override TestReferenceNavigationBuilder<TQuery, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TQuery, TRelatedEntity>> navigationExpression = null)
                => new NonGenericStringTestReferenceNavigationBuilder<TQuery, TRelatedEntity>(
                    QueryTypeBuilder.HasOne(
                        typeof(TRelatedEntity).FullName, navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));
        }

        private class NonGenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity> : NonGenericTestReferenceNavigationBuilder<
            TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public NonGenericStringTestReferenceNavigationBuilder(ReferenceNavigationBuilder referenceNavigationBuilder)
                : base(referenceNavigationBuilder)
            {
            }

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null)
                => new NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                    ReferenceNavigationBuilder.WithOne(navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));
        }

        private class NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity> : NonGenericTestReferenceReferenceBuilder<
            TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public NonGenericStringTestReferenceReferenceBuilder(ReferenceReferenceBuilder referenceReferenceBuilder)
                : base(referenceReferenceBuilder)
            {
            }

            protected override NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(
                ReferenceReferenceBuilder referenceReferenceBuilder)
                => new NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder);

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
                Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                => Wrap(
                    ReferenceReferenceBuilder.HasForeignKey(
                        typeof(TDependentEntity).Name,
                        foreignKeyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                Expression<Func<TPrincipalEntity, object>> keyExpression)
                => Wrap(
                    ReferenceReferenceBuilder.HasPrincipalKey(
                        typeof(TPrincipalEntity).Name,
                        keyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
                params string[] foreignKeyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.HasForeignKey(typeof(TDependentEntity).Name, foreignKeyPropertyNames));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                params string[] keyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.HasPrincipalKey(typeof(TPrincipalEntity).Name, keyPropertyNames));
        }

        private class NonGenericStringTestOwnedNavigationBuilder<TEntity, TDependentEntity>
            : NonGenericTestOwnedNavigationBuilder<TEntity, TDependentEntity>
            where TEntity : class
            where TDependentEntity : class
        {
            public NonGenericStringTestOwnedNavigationBuilder(OwnedNavigationBuilder ownedNavigationBuilder)
                : base(ownedNavigationBuilder)
            {
            }

            protected override NonGenericTestOwnedNavigationBuilder<TNewEntity, TNewDependentEntity> Wrap<TNewEntity, TNewDependentEntity>(
                OwnedNavigationBuilder ownedNavigationBuilder)
                => new NonGenericStringTestOwnedNavigationBuilder<TNewEntity, TNewDependentEntity>(ownedNavigationBuilder);

            public override TestReferenceNavigationBuilder<TDependentEntity, TNewDependentEntity> HasOne<TNewDependentEntity>(
                Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression = null)
                => new NonGenericStringTestReferenceNavigationBuilder<TDependentEntity, TNewDependentEntity>(
                    OwnedNavigationBuilder.HasOne(
                        typeof(TNewDependentEntity).FullName, navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));
        }
    }
}
