// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class ModelBuilderNonGenericStringTest : ModelBuilderNonGenericTest
    {
        public class NonGenericStringOwnedTypes : OwnedTypesTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new NonGenericStringTestModelBuilder(testHelpers);

            public override void Reconfiguring_owned_type_as_non_owned_throws()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Ignore<Customer>();
                var entityType = modelBuilder.Entity<SpecialCustomer>().OwnsOne(c => c.Details).OwnedEntityType;

                Assert.Equal(
                    CoreStrings.ClashingOwnedEntityType(typeof(CustomerDetails).FullName),
                    Assert.Throws<InvalidOperationException>(() =>
                        modelBuilder.Entity<SpecialCustomer>().HasOne(c => c.Details)).Message);
            }

            //Shadow navigations not supported #3864
            public override void Can_configure_owned_type_collection_with_one_call()
            {
            }

            public override void OwnedType_can_derive_from_Collection()
            {
            }
        }

        public class NonGenericStringOneToManyType : OneToManyTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new NonGenericStringTestModelBuilder(testHelpers);
        }

        public class NonGenericStringManyToOneType : ManyToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new NonGenericStringTestModelBuilder(testHelpers);
        }

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
                    typeof(TEntity),
                    e => buildAction(new NonGenericStringTestEntityTypeBuilder<TEntity>(e)));
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

            public override string GetDisplayName(Type entityType) => entityType.FullName;
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
                        typeof(TRelatedEntity).FullName, navigationExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(
                    EntityTypeBuilder.OwnsOne(
                        typeof(TRelatedEntity).FullName,
                        navigationExpression.GetPropertyAccess().GetSimpleMemberName(),
                        r => buildAction(new NonGenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

            public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression)
                => new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.OwnsMany(typeof(TRelatedEntity).FullName,
                        navigationExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(
                    EntityTypeBuilder.OwnsMany(
                        typeof(TRelatedEntity).FullName,
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

        private class NonGenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity>
            : NonGenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>
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

        private class NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>
            : NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>
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
                        typeof(TDependentEntity).FullName,
                        foreignKeyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                Expression<Func<TPrincipalEntity, object>> keyExpression)
                => Wrap(
                    ReferenceReferenceBuilder.HasPrincipalKey(
                        typeof(TPrincipalEntity).FullName,
                        keyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
                params string[] foreignKeyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.HasForeignKey(typeof(TDependentEntity).FullName, foreignKeyPropertyNames));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                params string[] keyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.HasPrincipalKey(typeof(TPrincipalEntity).FullName, keyPropertyNames));
        }

        private class NonGenericStringTestReferenceReferenceBuilder
        {
            public NonGenericStringTestReferenceReferenceBuilder(ReferenceReferenceBuilder referenceReferenceBuilder)
            {
                ReferenceReferenceBuilder = referenceReferenceBuilder;
            }

            public NonGenericStringTestReferenceReferenceBuilder HasForeignKey(
                string dependentEntityTypeName, params string[] foreignKeyPropertyNames)
                => new NonGenericStringTestReferenceReferenceBuilder(
                    ReferenceReferenceBuilder.HasForeignKey(dependentEntityTypeName, foreignKeyPropertyNames));

            public NonGenericStringTestReferenceReferenceBuilder HasPrincipalKey(
                string principalEntityTypeName, params string[] keyPropertyNames)
                => new NonGenericStringTestReferenceReferenceBuilder
                    (ReferenceReferenceBuilder.HasPrincipalKey(principalEntityTypeName, keyPropertyNames));

            private ReferenceReferenceBuilder ReferenceReferenceBuilder { get; }
            public IMutableForeignKey Metadata => ReferenceReferenceBuilder.Metadata;
        }

        private class NonGenericStringTestReferenceCollectionBuilder
        {
            public NonGenericStringTestReferenceCollectionBuilder(ReferenceCollectionBuilder referenceCollectionBuilder)
            {
                ReferenceCollectionBuilder = referenceCollectionBuilder;
            }

            private ReferenceCollectionBuilder ReferenceCollectionBuilder { get; }
            public IMutableForeignKey Metadata => ReferenceCollectionBuilder.Metadata;
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

            public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
                Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression)
                => new NonGenericStringTestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>(
                    OwnedNavigationBuilder.OwnsOne(
                        typeof(TNewDependentEntity).FullName, navigationExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
                Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
                => Wrap<TEntity, TDependentEntity>(
                    OwnedNavigationBuilder.OwnsOne(
                        typeof(TNewDependentEntity).FullName,
                        navigationExpression.GetPropertyAccess().GetSimpleMemberName(),
                        r => buildAction(new NonGenericStringTestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>(r))));

            public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
                Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression)
                => Wrap<TDependentEntity, TNewDependentEntity>(
                    OwnedNavigationBuilder.OwnsMany(
                        typeof(TNewDependentEntity).FullName, navigationExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
                Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
                => Wrap<TEntity, TDependentEntity>(
                    OwnedNavigationBuilder.OwnsMany(
                        typeof(TNewDependentEntity).FullName,
                        navigationExpression.GetPropertyAccess().GetSimpleMemberName(),
                        r => buildAction(Wrap<TDependentEntity, TNewDependentEntity>(r))));

            public override TestReferenceNavigationBuilder<TDependentEntity, TNewDependentEntity> HasOne<TNewDependentEntity>(
                Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression = null)
                => new NonGenericStringTestReferenceNavigationBuilder<TDependentEntity, TNewDependentEntity>(
                    OwnedNavigationBuilder.HasOne(
                        typeof(TNewDependentEntity).FullName, navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));
        }
    }
}
