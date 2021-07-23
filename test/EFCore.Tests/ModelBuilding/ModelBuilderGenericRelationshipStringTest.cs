// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestUtilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class ModelBuilderGenericRelationshipStringTest : ModelBuilderGenericTest
    {
        public class GenericOneToManyString : OneToManyTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers, Action<ModelConfigurationBuilder>? configure)
                => new GenericStringTestModelBuilder(testHelpers, configure);
        }

        public class GenericManyToOneString : ManyToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers, Action<ModelConfigurationBuilder>? configure)
                => new GenericStringTestModelBuilder(testHelpers, configure);
        }

        public class GenericOneToOneString : OneToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers, Action<ModelConfigurationBuilder>? configure)
                => new GenericStringTestModelBuilder(testHelpers, configure);
        }

        public class GenericOwnedTypesString : OwnedTypesTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers, Action<ModelConfigurationBuilder>? configure)
                => new GenericStringTestModelBuilder(testHelpers, configure);
        }

        private class GenericStringTestModelBuilder : TestModelBuilder
        {
            public GenericStringTestModelBuilder(TestHelpers testHelpers, Action<ModelConfigurationBuilder>? configure)
                : base(testHelpers, configure)
            {
            }

            public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
                => new GenericStringTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity<TEntity>());

            public override TestEntityTypeBuilder<TEntity> SharedTypeEntity<TEntity>(string name)
                => new GenericStringTestEntityTypeBuilder<TEntity>(ModelBuilder.SharedTypeEntity<TEntity>(name));

            public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
            {
                ModelBuilder.Entity<TEntity>(
                    entityTypeBuilder =>
                        buildAction(new GenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
                return this;
            }

            public override TestModelBuilder SharedTypeEntity<TEntity>(string name, Action<TestEntityTypeBuilder<TEntity>> buildAction)
            {
                ModelBuilder.SharedTypeEntity<TEntity>(
                    name,
                    entityTypeBuilder =>
                        buildAction(new GenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
                return this;
            }

            public override TestOwnedEntityTypeBuilder<TEntity> Owned<TEntity>()
                => new GenericTestOwnedEntityTypeBuilder<TEntity>(ModelBuilder.Owned<TEntity>());

            public override TestModelBuilder Ignore<TEntity>()
            {
                ModelBuilder.Ignore<TEntity>();
                return this;
            }

            public override string GetDisplayName(Type entityType)
                => entityType.FullName!;
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
                Expression<Func<TEntity, TRelatedEntity?>> navigationExpression)
                where TRelatedEntity : class
                => new GenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.OwnsOne<TRelatedEntity>(navigationExpression.GetMemberAccess().GetSimpleMemberName()));

            public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity?>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                where TRelatedEntity : class
                => Wrap(
                    EntityTypeBuilder.OwnsOne<TRelatedEntity>(
                        navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                        r => buildAction(new GenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

            public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression)
                where TRelatedEntity : class
                => new GenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.OwnsMany<TRelatedEntity>(navigationExpression.GetMemberAccess().GetSimpleMemberName()));

            public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                where TRelatedEntity : class
                => Wrap(
                    EntityTypeBuilder.OwnsMany<TRelatedEntity>(
                        navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                        r => buildAction(new GenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity?>>? navigationExpression = null)
                where TRelatedEntity : class
                => new GenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.HasOne<TRelatedEntity>(
                        navigationExpression?.GetMemberAccess().GetSimpleMemberName()));

            public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>>? navigationExpression = null)
                where TRelatedEntity : class
                => new GenericStringTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.HasMany<TRelatedEntity>(
                        navigationExpression?.GetMemberAccess().GetSimpleMemberName()));
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
                Expression<Func<TRelatedEntity, IEnumerable<TEntity>?>>? navigationExpression = null)
                => new GenericStringTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(
                    ReferenceNavigationBuilder.WithMany(
                        navigationExpression?.GetMemberAccess().GetSimpleMemberName()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression = null)
                => new GenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                    ReferenceNavigationBuilder.WithOne(
                        navigationExpression?.GetMemberAccess().GetSimpleMemberName()));
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
                Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression = null)
                => new GenericStringTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                    CollectionNavigationBuilder.WithOne(
                        navigationExpression?.GetMemberAccess().GetSimpleMemberName()));

            public override TestCollectionCollectionBuilder<TRelatedEntity, TEntity> WithMany(
                Expression<Func<TRelatedEntity, IEnumerable<TEntity>?>> navigationExpression)
                => new GenericStringTestCollectionCollectionBuilder<TRelatedEntity, TEntity>(
                    CollectionNavigationBuilder.WithMany(
                        navigationExpression.GetMemberAccess().GetSimpleMemberName()));
        }

        private class GenericStringTestReferenceCollectionBuilder<TEntity, TRelatedEntity>
            : GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>
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
                Expression<Func<TRelatedEntity, object?>> foreignKeyExpression)
                => Wrap(
                    ReferenceCollectionBuilder.HasForeignKey(
                        foreignKeyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
                Expression<Func<TEntity, object?>> keyExpression)
                => Wrap(
                    ReferenceCollectionBuilder.HasPrincipalKey(
                        keyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));
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
                Expression<Func<TDependentEntity, object?>> foreignKeyExpression)
                => Wrap(
                    ReferenceReferenceBuilder.HasForeignKey(
                        typeof(TDependentEntity).FullName!,
                        foreignKeyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
                params string[] foreignKeyPropertyNames)
                => Wrap(
                    ReferenceReferenceBuilder.HasForeignKey(
                        typeof(TDependentEntity).FullName!, foreignKeyPropertyNames));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                Expression<Func<TPrincipalEntity, object?>> keyExpression)
                => Wrap(
                    ReferenceReferenceBuilder.HasPrincipalKey(
                        typeof(TPrincipalEntity).FullName!,
                        keyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                params string[] keyPropertyNames)
                => Wrap(
                    ReferenceReferenceBuilder.HasPrincipalKey(
                        typeof(TPrincipalEntity).FullName!, keyPropertyNames));
        }

        private class GenericStringTestCollectionCollectionBuilder<TLeftEntity, TRightEntity> :
            GenericTestCollectionCollectionBuilder<TLeftEntity, TRightEntity>
            where TLeftEntity : class
            where TRightEntity : class
        {
            public GenericStringTestCollectionCollectionBuilder(
                CollectionCollectionBuilder<TLeftEntity, TRightEntity> collectionCollectionBuilder)
                : base(collectionCollectionBuilder)
            {

            }
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
                Expression<Func<TDependentEntity, TEntity?>>? referenceExpression)
                => new GenericTestOwnershipBuilder<TEntity, TDependentEntity>(
                    OwnedNavigationBuilder.WithOwner(referenceExpression?.GetMemberAccess().GetSimpleMemberName()));

            public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
                Expression<Func<TDependentEntity, TNewDependentEntity?>> navigationExpression)
                where TNewDependentEntity : class
                => Wrap(
                    OwnedNavigationBuilder.OwnsOne<TNewDependentEntity>(navigationExpression.GetMemberAccess().GetSimpleMemberName()));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
                Expression<Func<TDependentEntity, TNewDependentEntity?>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
                where TNewDependentEntity : class
                => Wrap(
                    OwnedNavigationBuilder.OwnsOne<TNewDependentEntity>(
                        navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                        r => buildAction(Wrap(r))));

            public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
                Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression)
                where TNewDependentEntity : class
                => Wrap(
                    OwnedNavigationBuilder.OwnsMany<TNewDependentEntity>(navigationExpression.GetMemberAccess().GetSimpleMemberName()));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
                Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
                where TNewDependentEntity : class
                => Wrap(
                    OwnedNavigationBuilder.OwnsMany<TNewDependentEntity>(
                        navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                        r => buildAction(Wrap(r))));

            public override TestReferenceNavigationBuilder<TDependentEntity, TNewRelatedEntity> HasOne<TNewRelatedEntity>(
                Expression<Func<TDependentEntity, TNewRelatedEntity?>>? navigationExpression = null)
                where TNewRelatedEntity : class
                => new GenericStringTestReferenceNavigationBuilder<TDependentEntity, TNewRelatedEntity>(
                    OwnedNavigationBuilder.HasOne<TNewRelatedEntity>(navigationExpression?.GetMemberAccess().GetSimpleMemberName()));
        }
    }
}
