// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using Xunit.Sdk;

// ReSharper disable AssignNullToNotNullAttribute
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class InMemoryModelBuilderNonGenericStringTest : InMemoryModelBuilderNonGenericTest
{
    public class NonGenericStringOwnedTypes(InMemoryModelBuilderFixture fixture) : InMemoryOwnedTypes(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure)
            => new NonGenericStringTestModelBuilder(Fixture, configure);

        public override void OwnedType_can_derive_from_Collection()
            // Shadow navigations. Issue #3864.
            => Assert.Equal(
                CoreStrings.AmbiguousSharedTypeEntityTypeName(
                    "Microsoft.EntityFrameworkCore.ModelBuilding.ModelBuilderTest+DependentEntity"),
                Assert.Throws<InvalidOperationException>(
                    base.OwnedType_can_derive_from_Collection).Message);
    }

    public class NonGenericStringOneToManyType(InMemoryModelBuilderFixture fixture) : InMemoryOneToMany(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure)
            => new NonGenericStringTestModelBuilder(Fixture, configure);

        public override void WithMany_pointing_to_keyless_entity_throws()
            // Test throws exception before reaching the first exception due to entity type being property bag entity
            => Assert.Throws<EqualException>(
                    base.WithMany_pointing_to_keyless_entity_throws);
    }

    public class NonGenericStringManyToOneType(InMemoryModelBuilderFixture fixture) : InMemoryManyToOne(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure)
            => new NonGenericStringTestModelBuilder(Fixture, configure);
    }

    public class NonGenericStringOneToOneType(InMemoryModelBuilderFixture fixture) : InMemoryOneToOne(fixture)
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
                typeof(TEntity),
                e => buildAction(new NonGenericStringTestEntityTypeBuilder<TEntity>(e)));
            return this;
        }

        public override TestModelBuilder SharedTypeEntity<TEntity>(string name, Action<TestEntityTypeBuilder<TEntity>> buildAction)
        {
            ModelBuilder.SharedTypeEntity(
                name,
                typeof(TEntity),
                e => buildAction(new NonGenericStringTestEntityTypeBuilder<TEntity>(e)));
            return this;
        }

        public override TestOwnedEntityTypeBuilder<TEntity> Owned<TEntity>()
            => new NonGenericTestOwnedEntityTypeBuilder<TEntity>(ModelBuilder.Owned(typeof(TEntity)));

        public override TestModelBuilder Ignore<TEntity>()
        {
            ModelBuilder.Ignore(typeof(TEntity));
            return this;
        }

        public override string GetDisplayName(Type entityType)
            => entityType.FullName!;
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
                    typeof(TRelatedEntity).FullName!, navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
            => Wrap(
                EntityTypeBuilder.OwnsOne(
                    typeof(TRelatedEntity).FullName!,
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericStringTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression)
            where TRelatedEntity : class
            => new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsMany(
                    typeof(TRelatedEntity).FullName!,
                    navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
            => Wrap(
                EntityTypeBuilder.OwnsMany(
                    typeof(TRelatedEntity).FullName!,
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>>? navigationExpression = null)
            where TRelatedEntity : class
            => new NonGenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.HasOne(
                    typeof(TRelatedEntity).FullName!,
                    navigationExpression?.GetMemberAccess().GetSimpleMemberName()));

        public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>>? navigationExpression = null)
            where TRelatedEntity : class
            => new NonGenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.HasMany(
                    typeof(TRelatedEntity).FullName!,
                    navigationExpression?.GetMemberAccess().GetSimpleMemberName()));
    }

    private class NonGenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(ReferenceNavigationBuilder referenceNavigationBuilder)
        : NonGenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(referenceNavigationBuilder)
        where TEntity : class
        where TRelatedEntity : class
    {
        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
            Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression = null)
            => new NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                ReferenceNavigationBuilder.WithOne(
                    navigationExpression?.GetMemberAccess().GetSimpleMemberName()));
    }

    private class NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(ReferenceReferenceBuilder referenceReferenceBuilder)
        : NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder)
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
                    typeof(TDependentEntity).FullName!,
                    foreignKeyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
            Expression<Func<TPrincipalEntity, object?>> keyExpression)
            => Wrap(
                ReferenceReferenceBuilder.HasPrincipalKey(
                    typeof(TPrincipalEntity).FullName!,
                    keyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
            params string[] foreignKeyPropertyNames)
            => Wrap(ReferenceReferenceBuilder.HasForeignKey(typeof(TDependentEntity).FullName!, foreignKeyPropertyNames));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
            params string[] keyPropertyNames)
            => Wrap(ReferenceReferenceBuilder.HasPrincipalKey(typeof(TPrincipalEntity).FullName!, keyPropertyNames));
    }

    private class NonGenericStringTestReferenceReferenceBuilder(ReferenceReferenceBuilder referenceReferenceBuilder)
    {
        public NonGenericStringTestReferenceReferenceBuilder HasForeignKey(
            string dependentEntityTypeName,
            params string[] foreignKeyPropertyNames)
            => new(
                ReferenceReferenceBuilder.HasForeignKey(dependentEntityTypeName, foreignKeyPropertyNames));

        public NonGenericStringTestReferenceReferenceBuilder HasPrincipalKey(
            string principalEntityTypeName,
            params string[] keyPropertyNames)
            => new(ReferenceReferenceBuilder.HasPrincipalKey(principalEntityTypeName, keyPropertyNames));

        private ReferenceReferenceBuilder ReferenceReferenceBuilder { get; } = referenceReferenceBuilder;

        public IMutableForeignKey Metadata
            => ReferenceReferenceBuilder.Metadata;
    }

    private class NonGenericStringTestReferenceCollectionBuilder(ReferenceCollectionBuilder referenceCollectionBuilder)
    {
        private ReferenceCollectionBuilder ReferenceCollectionBuilder { get; } = referenceCollectionBuilder;

        public IMutableForeignKey Metadata
            => ReferenceCollectionBuilder.Metadata;
    }

    private class NonGenericStringTestOwnedNavigationBuilder<TEntity, TDependentEntity>(OwnedNavigationBuilder ownedNavigationBuilder)
        : NonGenericTestOwnedNavigationBuilder<TEntity, TDependentEntity>(ownedNavigationBuilder)
        where TEntity : class
        where TDependentEntity : class
    {
        protected override NonGenericTestOwnedNavigationBuilder<TNewEntity, TNewDependentEntity> Wrap<TNewEntity, TNewDependentEntity>(
            OwnedNavigationBuilder ownedNavigationBuilder)
            => new NonGenericStringTestOwnedNavigationBuilder<TNewEntity, TNewDependentEntity>(ownedNavigationBuilder);

        public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
            Expression<Func<TDependentEntity, TNewDependentEntity?>> navigationExpression)
            where TNewDependentEntity : class
            => new NonGenericStringTestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>(
                OwnedNavigationBuilder.OwnsOne(
                    typeof(TNewDependentEntity).FullName!, navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
            Expression<Func<TDependentEntity, TNewDependentEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
            => Wrap<TEntity, TDependentEntity>(
                OwnedNavigationBuilder.OwnsOne(
                    typeof(TNewDependentEntity).FullName!,
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericStringTestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>(r))));

        public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression)
            where TNewDependentEntity : class
            => Wrap<TDependentEntity, TNewDependentEntity>(
                OwnedNavigationBuilder.OwnsMany(
                    typeof(TNewDependentEntity).FullName!, navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
            => Wrap<TEntity, TDependentEntity>(
                OwnedNavigationBuilder.OwnsMany(
                    typeof(TNewDependentEntity).FullName!,
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(Wrap<TDependentEntity, TNewDependentEntity>(r))));

        public override TestReferenceNavigationBuilder<TDependentEntity, TNewDependentEntity> HasOne<TNewDependentEntity>(
            Expression<Func<TDependentEntity, TNewDependentEntity?>>? navigationExpression = null)
            where TNewDependentEntity : class
            => new NonGenericStringTestReferenceNavigationBuilder<TDependentEntity, TNewDependentEntity>(
                OwnedNavigationBuilder.HasOne(
                    typeof(TNewDependentEntity).FullName!, navigationExpression?.GetMemberAccess().GetSimpleMemberName()));
    }
}
