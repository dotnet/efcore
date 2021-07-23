// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestUtilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class ModelBuilderGenericRelationshipTypeTest : ModelBuilderGenericTest
    {
        public class GenericOneToOneType : OneToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers, Action<ModelConfigurationBuilder>? configure)
                => new GenericTypeTestModelBuilder(testHelpers, configure);
        }

        private class GenericTypeTestModelBuilder : TestModelBuilder
        {
            public GenericTypeTestModelBuilder(TestHelpers testHelpers, Action<ModelConfigurationBuilder>? configure)
                : base(testHelpers, configure)
            {
            }

            public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
                => new GenericTypeTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity<TEntity>());

            public override TestEntityTypeBuilder<TEntity> SharedTypeEntity<TEntity>(string name)
                => new GenericTypeTestEntityTypeBuilder<TEntity>(ModelBuilder.SharedTypeEntity<TEntity>(name));

            public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
            {
                ModelBuilder.Entity<TEntity>(
                    entityTypeBuilder =>
                        buildAction(new GenericTypeTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
                return this;
            }

            public override TestModelBuilder SharedTypeEntity<TEntity>(string name, Action<TestEntityTypeBuilder<TEntity>> buildAction)
            {
                ModelBuilder.SharedTypeEntity<TEntity>(
                    name,
                    entityTypeBuilder =>
                        buildAction(new GenericTypeTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
                return this;
            }

            public override TestOwnedEntityTypeBuilder<TEntity> Owned<TEntity>()
                => new GenericTestOwnedEntityTypeBuilder<TEntity>(ModelBuilder.Owned<TEntity>());

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

            public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity?>> navigationExpression)
                where TRelatedEntity : class
                => new GenericTypeTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.OwnsOne(navigationExpression));

            public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity?>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                where TRelatedEntity : class
                => Wrap(
                    EntityTypeBuilder.OwnsOne(
                        navigationExpression,
                        r => buildAction(new GenericTypeTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity?>>? navigationExpression = null)
                where TRelatedEntity : class
                => new GenericTypeTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.HasOne(navigationExpression));
        }

        private class GenericTypeTestReferenceNavigationBuilder<TEntity, TRelatedEntity> : GenericTestReferenceNavigationBuilder<TEntity,
            TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTypeTestReferenceNavigationBuilder(ReferenceNavigationBuilder<TEntity, TRelatedEntity> referenceNavigationBuilder)
                : base(referenceNavigationBuilder)
            {
            }

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression = null)
                => new GenericTypeTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                    ReferenceNavigationBuilder.WithOne(navigationExpression));
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
                Expression<Func<TDependentEntity, object?>> foreignKeyExpression)
                => Wrap(
                    ReferenceReferenceBuilder.HasForeignKey(
                        typeof(TDependentEntity),
                        foreignKeyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
                params string[] foreignKeyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.HasForeignKey(typeof(TDependentEntity), foreignKeyPropertyNames));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                Expression<Func<TPrincipalEntity, object?>> keyExpression)
                => Wrap(
                    ReferenceReferenceBuilder.HasPrincipalKey(
                        typeof(TPrincipalEntity),
                        keyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                params string[] keyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.HasPrincipalKey(typeof(TPrincipalEntity), keyPropertyNames));
        }

        private class GenericTypeTestOwnedNavigationBuilder<TEntity, TRelatedEntity>
            : GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTypeTestOwnedNavigationBuilder(OwnedNavigationBuilder<TEntity, TRelatedEntity> ownedNavigationBuilder)
                : base(ownedNavigationBuilder)
            {
            }

            protected override GenericTestOwnedNavigationBuilder<TNewEntity, TNewRelatedEntity> Wrap<TNewEntity, TNewRelatedEntity>(
                OwnedNavigationBuilder<TNewEntity, TNewRelatedEntity> ownedNavigationBuilder)
                => new GenericTypeTestOwnedNavigationBuilder<TNewEntity, TNewRelatedEntity>(ownedNavigationBuilder);

            public override TestReferenceNavigationBuilder<TRelatedEntity, TNewRelatedEntity> HasOne<TNewRelatedEntity>(
                Expression<Func<TRelatedEntity, TNewRelatedEntity?>>? navigationExpression = null)
                where TNewRelatedEntity : class
                => new GenericTypeTestReferenceNavigationBuilder<TRelatedEntity, TNewRelatedEntity>(
                    OwnedNavigationBuilder.HasOne(navigationExpression));
        }
    }
}
