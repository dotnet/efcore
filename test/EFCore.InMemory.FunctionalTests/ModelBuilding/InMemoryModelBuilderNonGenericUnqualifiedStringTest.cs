// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class InMemoryModelBuilderNonGenericUnqualifiedStringTest : InMemoryModelBuilderNonGenericTest
{
    public class NonGenericStringOneToOne(InMemoryModelBuilderFixture fixture) : InMemoryOneToOne(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure)
            => new NonGenericStringTestModelBuilder(Fixture, configure);
    }

    private class NonGenericStringTestModelBuilder(ModelBuilderFixtureBase fixture, Action<ModelConfigurationBuilder>? configure) : TestModelBuilder(fixture, configure)
    {
        public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
            => new NonGenericStringTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity(typeof(TEntity)));

        public override TestEntityTypeBuilder<TEntity> SharedTypeEntity<TEntity>(string name)
            => new NonGenericStringTestEntityTypeBuilder<TEntity>(ModelBuilder.SharedTypeEntity(name, typeof(TEntity)));

        public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
        {
            ModelBuilder.Entity(
                typeof(TEntity), entityTypeBuilder =>
                    buildAction(new NonGenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
            return this;
        }

        public override TestModelBuilder SharedTypeEntity<TEntity>(string name, Action<TestEntityTypeBuilder<TEntity>> buildAction)
        {
            ModelBuilder.SharedTypeEntity(
                name,
                typeof(TEntity), entityTypeBuilder =>
                    buildAction(new NonGenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
            return this;
        }

        public override TestOwnedEntityTypeBuilder<TEntity> Owned<TEntity>()
            => new NonGenericTestOwnedEntityTypeBuilder<TEntity>(ModelBuilder.Owned(typeof(TEntity)));

        public override TestModelBuilder Ignore<TEntity>()
        {
            ModelBuilder.Ignore(typeof(TEntity));
            return this;
        }
    }

    private class NonGenericStringTestEntityTypeBuilder<TEntity>(EntityTypeBuilder entityTypeBuilder) : NonGenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder)
        where TEntity : class
    {
        protected override NonGenericTestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder entityTypeBuilder)
            => new NonGenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder);

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression)
            where TRelatedEntity : class
            => new NonGenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsOne(
                    typeof(TRelatedEntity).Name, navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
            => Wrap(
                EntityTypeBuilder.OwnsOne(
                    typeof(TRelatedEntity).Name,
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression)
            => new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsMany(
                    typeof(TRelatedEntity).Name,
                    navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            => Wrap(
                EntityTypeBuilder.OwnsMany(
                    typeof(TRelatedEntity).Name,
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>>? navigationExpression = null)
            where TRelatedEntity : class
        {
            var navigationName = navigationExpression?.GetMemberAccess().GetSimpleMemberName();

            return new NonGenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                navigationName == null
                    ? EntityTypeBuilder.HasOne(typeof(TRelatedEntity).FullName!, navigationName)
                    : EntityTypeBuilder.HasOne(navigationName));
        }

        public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>>? navigationExpression = null)
            where TRelatedEntity : class
        {
            var navigationName = navigationExpression?.GetMemberAccess().GetSimpleMemberName();

            return new NonGenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(
                navigationName == null
                    ? EntityTypeBuilder.HasMany(typeof(TRelatedEntity).FullName!, navigationName)
                    : EntityTypeBuilder.HasMany(navigationName));
        }
    }

    private class NonGenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(ReferenceNavigationBuilder referenceNavigationBuilder) : NonGenericTestReferenceNavigationBuilder<
        TEntity, TRelatedEntity>(referenceNavigationBuilder)
        where TEntity : class
        where TRelatedEntity : class
    {
        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
            Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression = null)
            => new NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                ReferenceNavigationBuilder.WithOne(
                    navigationExpression?.GetMemberAccess().GetSimpleMemberName()));
    }

    private class NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(ReferenceReferenceBuilder referenceReferenceBuilder) : NonGenericTestReferenceReferenceBuilder<
        TEntity, TRelatedEntity>(referenceReferenceBuilder)
        where TEntity : class
        where TRelatedEntity : class
    {
        protected override NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(
            ReferenceReferenceBuilder referenceReferenceBuilder)
            => new NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder);

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
            Expression<Func<TDependentEntity, object?>> foreignKeyExpression)
            => Wrap(
                ReferenceReferenceBuilder.HasForeignKey(
                    typeof(TDependentEntity).Name,
                    foreignKeyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
            Expression<Func<TPrincipalEntity, object?>> keyExpression)
            => Wrap(
                ReferenceReferenceBuilder.HasPrincipalKey(
                    typeof(TPrincipalEntity).Name,
                    keyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
            params string[] foreignKeyPropertyNames)
            => Wrap(ReferenceReferenceBuilder.HasForeignKey(typeof(TDependentEntity).Name, foreignKeyPropertyNames));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
            params string[] keyPropertyNames)
            => Wrap(ReferenceReferenceBuilder.HasPrincipalKey(typeof(TPrincipalEntity).Name, keyPropertyNames));
    }

    private class NonGenericStringTestOwnedNavigationBuilder<TEntity, TDependentEntity>(OwnedNavigationBuilder ownedNavigationBuilder)
        : NonGenericTestOwnedNavigationBuilder<TEntity, TDependentEntity>(ownedNavigationBuilder)
        where TEntity : class
        where TDependentEntity : class
    {
        protected override NonGenericTestOwnedNavigationBuilder<TNewEntity, TNewDependentEntity> Wrap<TNewEntity, TNewDependentEntity>(
            OwnedNavigationBuilder ownedNavigationBuilder)
            => new NonGenericStringTestOwnedNavigationBuilder<TNewEntity, TNewDependentEntity>(ownedNavigationBuilder);

        public override TestReferenceNavigationBuilder<TDependentEntity, TNewDependentEntity> HasOne<TNewDependentEntity>(
            Expression<Func<TDependentEntity, TNewDependentEntity?>>? navigationExpression = null)
            where TNewDependentEntity : class
        {
            var navigationName = navigationExpression?.GetMemberAccess().GetSimpleMemberName();

            return new NonGenericStringTestReferenceNavigationBuilder<TDependentEntity, TNewDependentEntity>(
                navigationName == null
                    ? OwnedNavigationBuilder.HasOne(typeof(TNewDependentEntity).FullName!, navigationName)
                    : OwnedNavigationBuilder.HasOne(navigationName));
        }
    }
}
