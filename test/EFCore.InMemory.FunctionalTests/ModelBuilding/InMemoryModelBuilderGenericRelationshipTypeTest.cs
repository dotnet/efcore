// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public class InMemoryModelBuilderGenericRelationshipTypeTest : InMemoryModelBuilderGenericTest
{
    public class GenericOneToOneType(InMemoryModelBuilderFixture fixture) : InMemoryOneToOne(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new GenericTypeTestModelBuilder(Fixture, configure);
    }

    public class GenericNonRelationshipTest(InMemoryModelBuilderFixture fixture) : InMemoryNonRelationship(fixture)
    {
        protected override TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null)
            => new GenericTypeTestModelBuilder(Fixture, configure);
    }

    public class GenericTypeTestModelBuilder(ModelBuilderFixtureBase fixture, Action<ModelConfigurationBuilder>? configure) : TestModelBuilder(fixture, configure)
    {
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

    protected class GenericTypeTestEntityTypeBuilder<TEntity>(EntityTypeBuilder<TEntity> entityTypeBuilder) : GenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder)
        where TEntity : class
    {
        protected override TestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder<TEntity> entityTypeBuilder)
            => new GenericTypeTestEntityTypeBuilder<TEntity>(entityTypeBuilder);

        protected override TestPropertyBuilder<TProperty> Wrap<TProperty>(PropertyBuilder<TProperty> propertyBuilder)
            => new GenericTypeTestPropertyBuilder<TProperty>(propertyBuilder);

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

        public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>>? navigationExpression = null)
            where TRelatedEntity : class
            => new GenericTypeTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasMany(navigationExpression));
    }

    protected class GenericTypeTestPropertyBuilder<TProperty>(PropertyBuilder<TProperty> propertyBuilder) : GenericTestPropertyBuilder<TProperty>(propertyBuilder)
    {
        protected override TestPropertyBuilder<TProperty> Wrap(PropertyBuilder<TProperty> propertyBuilder)
            => new GenericTypeTestPropertyBuilder<TProperty>(propertyBuilder);

        public override TestPropertyBuilder<TProperty> HasConversion<TProvider>()
            => Wrap(PropertyBuilder.HasConversion(typeof(TProvider)));

        public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(ValueComparer? valueComparer)
            => Wrap(PropertyBuilder.HasConversion(typeof(TProvider), valueComparer));

        public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType)
            => Wrap(PropertyBuilder.HasConversion(typeof(TProvider), valueComparer, providerComparerType));

        public override TestPropertyBuilder<TProperty> HasConversion<TConverter, TComparer>()
            => Wrap(PropertyBuilder.HasConversion(typeof(TConverter), typeof(TComparer)));

        public override TestPropertyBuilder<TProperty> HasConversion<TConverter, TComparer, TProviderComparer>()
            => Wrap(PropertyBuilder.HasConversion(typeof(TConverter), typeof(TComparer), typeof(TProviderComparer)));
    }

    protected class GenericTypeTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(ReferenceNavigationBuilder<TEntity, TRelatedEntity> referenceNavigationBuilder) : GenericTestReferenceNavigationBuilder<TEntity,
        TRelatedEntity>(referenceNavigationBuilder)
        where TEntity : class
        where TRelatedEntity : class
    {
        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
            Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression = null)
            => new GenericTypeTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                ReferenceNavigationBuilder.WithOne(navigationExpression));
    }

    protected class GenericTypeTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(
        CollectionNavigationBuilder<TEntity, TRelatedEntity> collectionNavigationBuilder) :
        GenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(collectionNavigationBuilder)
        where TEntity : class
        where TRelatedEntity : class
    {
        public override TestCollectionCollectionBuilder<TRelatedEntity, TEntity> WithMany(
            Expression<Func<TRelatedEntity, IEnumerable<TEntity>?>> navigationExpression)
            => new GenericTypeTestCollectionCollectionBuilder<TRelatedEntity, TEntity>(
                CollectionNavigationBuilder.WithMany(navigationExpression));
    }

    protected class GenericTypeTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder)
        : GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder)
        where TEntity : class
        where TRelatedEntity : class
    {
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

    protected class GenericTypeTestCollectionCollectionBuilder<TLeftEntity, TRightEntity>(
        CollectionCollectionBuilder<TLeftEntity, TRightEntity> collectionCollectionBuilder) :
        GenericTestCollectionCollectionBuilder<TLeftEntity, TRightEntity>(collectionCollectionBuilder)
        where TLeftEntity : class
        where TRightEntity : class
    {
        public override TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>()
            => new GenericTypeTestEntityTypeBuilder<TJoinEntity>(
                new EntityTypeBuilder<TJoinEntity>(
                    CollectionCollectionBuilder.UsingEntity(
                        typeof(TJoinEntity)).Metadata));

        public override TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            string joinEntityName)
            => new GenericTypeTestEntityTypeBuilder<TJoinEntity>(
                new EntityTypeBuilder<TJoinEntity>(
                    CollectionCollectionBuilder.UsingEntity(
                        joinEntityName,
                        typeof(TJoinEntity)).Metadata));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            => new GenericTypeTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity<TJoinEntity>(
                    e => configureJoinEntityType(new GenericTypeTestEntityTypeBuilder<TJoinEntity>(e))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            string joinEntityName,
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            => new GenericTypeTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    joinEntityName,
                    typeof(TJoinEntity),
                    e => configureJoinEntityType(
                        new GenericTypeTestEntityTypeBuilder<TJoinEntity>(new EntityTypeBuilder<TJoinEntity>(e.Metadata)))));

        public override TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft)
            => new GenericTypeTestEntityTypeBuilder<TJoinEntity>(
                new EntityTypeBuilder<TJoinEntity>(
                    CollectionCollectionBuilder.UsingEntity(
                        typeof(TJoinEntity),
                        l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                                new GenericTypeTestEntityTypeBuilder<TJoinEntity>(new EntityTypeBuilder<TJoinEntity>(l.Metadata))))
                            .ReferenceCollectionBuilder,
                        r => ((GenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                                new GenericTypeTestEntityTypeBuilder<TJoinEntity>(new EntityTypeBuilder<TJoinEntity>(r.Metadata))))
                            .ReferenceCollectionBuilder).Metadata));

        public override TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            string joinEntityName,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft)
            => new GenericTypeTestEntityTypeBuilder<TJoinEntity>(
                new EntityTypeBuilder<TJoinEntity>(
                    CollectionCollectionBuilder.UsingEntity(
                        joinEntityName,
                        typeof(TJoinEntity),
                        l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                                new GenericTypeTestEntityTypeBuilder<TJoinEntity>(new EntityTypeBuilder<TJoinEntity>(l.Metadata))))
                            .ReferenceCollectionBuilder,
                        r => ((GenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                                new GenericTypeTestEntityTypeBuilder<TJoinEntity>(new EntityTypeBuilder<TJoinEntity>(r.Metadata))))
                            .ReferenceCollectionBuilder).Metadata));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            where TJoinEntity : class
            => new GenericTypeTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    typeof(TJoinEntity),
                    l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                            new GenericTypeTestEntityTypeBuilder<TJoinEntity>(new EntityTypeBuilder<TJoinEntity>(l.Metadata))))
                        .ReferenceCollectionBuilder,
                    r => ((GenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                            new GenericTypeTestEntityTypeBuilder<TJoinEntity>(new EntityTypeBuilder<TJoinEntity>(r.Metadata))))
                        .ReferenceCollectionBuilder,
                    e => configureJoinEntityType(
                        new GenericTypeTestEntityTypeBuilder<TJoinEntity>(new EntityTypeBuilder<TJoinEntity>(e.Metadata)))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            string joinEntityName,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            where TJoinEntity : class
            => new GenericTypeTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    joinEntityName,
                    typeof(TJoinEntity),
                    l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                            new GenericTypeTestEntityTypeBuilder<TJoinEntity>(new EntityTypeBuilder<TJoinEntity>(l.Metadata))))
                        .ReferenceCollectionBuilder,
                    r => ((GenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                            new GenericTypeTestEntityTypeBuilder<TJoinEntity>(new EntityTypeBuilder<TJoinEntity>(r.Metadata))))
                        .ReferenceCollectionBuilder,
                    e => configureJoinEntityType(
                        new GenericTypeTestEntityTypeBuilder<TJoinEntity>(new EntityTypeBuilder<TJoinEntity>(e.Metadata)))));
    }

    protected class GenericTypeTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(OwnedNavigationBuilder<TEntity, TRelatedEntity> ownedNavigationBuilder)
        : GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(ownedNavigationBuilder)
        where TEntity : class
        where TRelatedEntity : class
    {
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
