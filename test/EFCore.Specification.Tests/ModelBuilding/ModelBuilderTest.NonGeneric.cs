// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public abstract partial class ModelBuilderTest
{
    public class NonGenericTestModelBuilder(ModelBuilderFixtureBase fixture, Action<ModelConfigurationBuilder>? configure) : TestModelBuilder(fixture, configure)
    {
        public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
            => new NonGenericTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity(typeof(TEntity)));

        public override TestEntityTypeBuilder<TEntity> SharedTypeEntity<TEntity>(string name)
            => new NonGenericTestEntityTypeBuilder<TEntity>(ModelBuilder.SharedTypeEntity(name, typeof(TEntity)));

        public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
        {
            ModelBuilder.Entity(
                typeof(TEntity), entityTypeBuilder =>
                    buildAction(new NonGenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
            return this;
        }

        public override TestModelBuilder SharedTypeEntity<TEntity>(string name, Action<TestEntityTypeBuilder<TEntity>> buildAction)
        {
            ModelBuilder.SharedTypeEntity(
                name,
                typeof(TEntity), entityTypeBuilder =>
                    buildAction(new NonGenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
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

    protected class NonGenericTestEntityTypeBuilder<TEntity>(EntityTypeBuilder entityTypeBuilder) : TestEntityTypeBuilder<TEntity>, IInfrastructure<EntityTypeBuilder>
        where TEntity : class
    {
        protected EntityTypeBuilder EntityTypeBuilder { get; } = entityTypeBuilder;

        public override IMutableEntityType Metadata
            => EntityTypeBuilder.Metadata;

        protected virtual NonGenericTestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder entityTypeBuilder)
            => new(entityTypeBuilder);

        protected virtual TestPropertyBuilder<TProperty> Wrap<TProperty>(PropertyBuilder propertyBuilder)
            => new NonGenericTestPropertyBuilder<TProperty>(propertyBuilder);

        protected virtual TestPrimitiveCollectionBuilder<TProperty> Wrap<TProperty>(PrimitiveCollectionBuilder propertyBuilder)
            => new NonGenericTestPrimitiveCollectionBuilder<TProperty>(propertyBuilder);

        public override TestEntityTypeBuilder<TEntity> HasAnnotation(string annotation, object? value)
            => Wrap(EntityTypeBuilder.HasAnnotation(annotation, value));

        public override TestEntityTypeBuilder<TEntity> HasBaseType<TBaseEntity>()
            => Wrap(EntityTypeBuilder.HasBaseType(typeof(TBaseEntity)));

        public override TestEntityTypeBuilder<TEntity> HasBaseType(string? baseEntityTypeName)
            => Wrap(EntityTypeBuilder.HasBaseType(baseEntityTypeName));

        public override TestKeyBuilder<TEntity> HasKey(Expression<Func<TEntity, object?>> keyExpression)
            => new NonGenericTestKeyBuilder<TEntity>(
                EntityTypeBuilder.HasKey(keyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestKeyBuilder<TEntity> HasKey(params string[] propertyNames)
            => new NonGenericTestKeyBuilder<TEntity>(EntityTypeBuilder.HasKey(propertyNames));

        public override TestKeyBuilder<TEntity> HasAlternateKey(Expression<Func<TEntity, object?>> keyExpression)
            => new NonGenericTestKeyBuilder<TEntity>(
                EntityTypeBuilder.HasAlternateKey(
                    keyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestKeyBuilder<TEntity> HasAlternateKey(params string[] propertyNames)
            => new NonGenericTestKeyBuilder<TEntity>(EntityTypeBuilder.HasAlternateKey(propertyNames));

        public override TestEntityTypeBuilder<TEntity> HasNoKey()
            => Wrap(EntityTypeBuilder.HasNoKey());

        public override TestPropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            return Wrap<TProperty>(EntityTypeBuilder.Property(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName()));
        }

        public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
            => Wrap<TProperty>(EntityTypeBuilder.Property<TProperty>(propertyName));

        public override TestPrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression)
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            return Wrap<TProperty>(EntityTypeBuilder.PrimitiveCollection(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName()));
        }

        public override TestPrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(string propertyName)
            => Wrap<TProperty>(EntityTypeBuilder.PrimitiveCollection<TProperty>(propertyName));

        public override TestPropertyBuilder<TProperty> IndexerProperty<TProperty>(string propertyName)
            => Wrap<TProperty>(EntityTypeBuilder.IndexerProperty<TProperty>(propertyName));

        public override TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(string propertyName)
            => new NonGenericTestComplexPropertyBuilder<TProperty>(EntityTypeBuilder.ComplexProperty<TProperty>(propertyName));

        public override TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
            Expression<Func<TEntity, TProperty?>> propertyExpression)
            where TProperty : default
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            return new NonGenericTestComplexPropertyBuilder<TProperty>(
                EntityTypeBuilder.ComplexProperty(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName()));
        }

        public override TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
            Expression<Func<TEntity, TProperty?>> propertyExpression,
            string complexTypeName)
            where TProperty : default
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            return new NonGenericTestComplexPropertyBuilder<TProperty>(
                EntityTypeBuilder.ComplexProperty(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName(), complexTypeName));
        }

        public override TestEntityTypeBuilder<TEntity> ComplexProperty<TProperty>(
            Expression<Func<TEntity, TProperty?>> propertyExpression,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction)
            where TProperty : default
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            buildAction(
                new NonGenericTestComplexPropertyBuilder<TProperty>(
                    EntityTypeBuilder.ComplexProperty(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName())));

            return this;
        }

        public override TestEntityTypeBuilder<TEntity> ComplexProperty<TProperty>(
            Expression<Func<TEntity, TProperty?>> propertyExpression,
            string complexTypeName,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction)
            where TProperty : default
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            buildAction(
                new NonGenericTestComplexPropertyBuilder<TProperty>(
                    EntityTypeBuilder.ComplexProperty(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName(), complexTypeName)));

            return this;
        }

        public override TestEntityTypeBuilder<TEntity> ComplexProperty<TProperty>(
            string propertyName,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction)
        {
            buildAction(new NonGenericTestComplexPropertyBuilder<TProperty>(EntityTypeBuilder.ComplexProperty<TProperty>(propertyName)));

            return this;
        }

        public override TestNavigationBuilder Navigation<TNavigation>(Expression<Func<TEntity, TNavigation?>> navigationExpression)
            where TNavigation : class
            => new NonGenericTestNavigationBuilder(
                EntityTypeBuilder.Navigation(navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestNavigationBuilder Navigation<TNavigation>(
            Expression<Func<TEntity, IEnumerable<TNavigation>?>> navigationExpression)
            where TNavigation : class
            => new NonGenericTestNavigationBuilder(
                EntityTypeBuilder.Navigation(navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestEntityTypeBuilder<TEntity> Ignore(Expression<Func<TEntity, object?>> propertyExpression)
            => Wrap(EntityTypeBuilder.Ignore(propertyExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestEntityTypeBuilder<TEntity> Ignore(string propertyName)
            => Wrap(EntityTypeBuilder.Ignore(propertyName));

        public override TestIndexBuilder<TEntity> HasIndex(Expression<Func<TEntity, object?>> indexExpression)
            => new NonGenericTestIndexBuilder<TEntity>(
                EntityTypeBuilder.HasIndex(indexExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestIndexBuilder<TEntity> HasIndex(Expression<Func<TEntity, object?>> indexExpression, string name)
            => new NonGenericTestIndexBuilder<TEntity>(
                EntityTypeBuilder.HasIndex(indexExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray(), name));

        public override TestIndexBuilder<TEntity> HasIndex(params string[] propertyNames)
            => new NonGenericTestIndexBuilder<TEntity>(EntityTypeBuilder.HasIndex(propertyNames));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(string navigationName)
            => new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsOne(typeof(TRelatedEntity), navigationName));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
            string entityTypeName,
            string navigationName)
            => new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsOne(entityTypeName, typeof(TRelatedEntity), navigationName));

        public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            string navigationName,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            => Wrap(
                EntityTypeBuilder.OwnsOne(
                    typeof(TRelatedEntity),
                    navigationName,
                    r => buildAction(new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            string entityTypeName,
            string navigationName,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            => Wrap(
                EntityTypeBuilder.OwnsOne(
                    entityTypeName,
                    typeof(TRelatedEntity),
                    navigationName,
                    r => buildAction(new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression)
            where TRelatedEntity : class
            => new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsOne(typeof(TRelatedEntity), navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
            string entityTypeName,
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression)
            where TRelatedEntity : class
            => new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsOne(
                    entityTypeName, typeof(TRelatedEntity), navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
            => Wrap(
                EntityTypeBuilder.OwnsOne(
                    typeof(TRelatedEntity),
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            string entityTypeName,
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
            => Wrap(
                EntityTypeBuilder.OwnsOne(
                    entityTypeName,
                    typeof(TRelatedEntity),
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(string navigationName)
            => new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsMany(typeof(TRelatedEntity), navigationName));

        public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            string navigationName,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            => Wrap(
                EntityTypeBuilder.OwnsMany(
                    typeof(TRelatedEntity),
                    navigationName,
                    r => buildAction(new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
            string entityTypeName,
            string navigationName)
            => new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsMany(entityTypeName, typeof(TRelatedEntity), navigationName));

        public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            string entityTypeName,
            string navigationName,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            => Wrap(
                EntityTypeBuilder.OwnsMany(
                    entityTypeName,
                    typeof(TRelatedEntity),
                    navigationName,
                    r => buildAction(new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression)
            => new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsMany(typeof(TRelatedEntity), navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
            string entityTypeName,
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression)
            => new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsMany(
                    entityTypeName, typeof(TRelatedEntity), navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            => Wrap(
                EntityTypeBuilder.OwnsMany(
                    typeof(TRelatedEntity),
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            string entityTypeName,
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            => Wrap(
                EntityTypeBuilder.OwnsMany(
                    entityTypeName,
                    typeof(TRelatedEntity),
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(string? navigationName)
            where TRelatedEntity : class
            => new NonGenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.HasOne(navigationName));

        public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>>? navigationExpression = null)
            where TRelatedEntity : class
            => new NonGenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.HasOne(
                    typeof(TRelatedEntity),
                    navigationExpression?.GetMemberAccess().GetSimpleMemberName()));

        public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(string? navigationName)
            where TRelatedEntity : class
            => new NonGenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.HasMany(typeof(TRelatedEntity), navigationName));

        public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>>? navigationExpression = null)
            where TRelatedEntity : class
            => new NonGenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.HasMany(
                    typeof(TRelatedEntity),
                    navigationExpression?.GetMemberAccess().GetSimpleMemberName()));

        public override TestEntityTypeBuilder<TEntity> HasQueryFilter(Expression<Func<TEntity, bool>> filter)
            => Wrap(EntityTypeBuilder.HasQueryFilter(filter));

        public override TestEntityTypeBuilder<TEntity> HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
            => Wrap(EntityTypeBuilder.HasChangeTrackingStrategy(changeTrackingStrategy));

        public override TestEntityTypeBuilder<TEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => Wrap(EntityTypeBuilder.UsePropertyAccessMode(propertyAccessMode));

        public override DataBuilder<TEntity> HasData(params TEntity[] data)
        {
            EntityTypeBuilder.HasData(data);
            return new DataBuilder<TEntity>();
        }

        public override DataBuilder<TEntity> HasData(params object[] data)
        {
            EntityTypeBuilder.HasData(data);
            return new DataBuilder<TEntity>();
        }

        public override DataBuilder<TEntity> HasData(IEnumerable<TEntity> data)
        {
            EntityTypeBuilder.HasData(data);
            return new DataBuilder<TEntity>();
        }

        public override DataBuilder<TEntity> HasData(IEnumerable<object> data)
        {
            EntityTypeBuilder.HasData(data);
            return new DataBuilder<TEntity>();
        }

        public override TestDiscriminatorBuilder<TDiscriminator> HasDiscriminator<TDiscriminator>(
            Expression<Func<TEntity, TDiscriminator>> propertyExpression)
            => new NonGenericTestDiscriminatorBuilder<TDiscriminator>(
                EntityTypeBuilder.HasDiscriminator(
                    propertyExpression.GetMemberAccess().GetSimpleMemberName(), typeof(TDiscriminator)));

        public override TestDiscriminatorBuilder<TDiscriminator> HasDiscriminator<TDiscriminator>(string propertyName)
            => new NonGenericTestDiscriminatorBuilder<TDiscriminator>(
                EntityTypeBuilder.HasDiscriminator(propertyName, typeof(TDiscriminator)));

        public override TestEntityTypeBuilder<TEntity> HasNoDiscriminator()
            => Wrap(EntityTypeBuilder.HasNoDiscriminator());

        public EntityTypeBuilder Instance
            => EntityTypeBuilder;
    }

    protected class NonGenericTestComplexPropertyBuilder<TComplex>(ComplexPropertyBuilder complexPropertyBuilder) :
        TestComplexPropertyBuilder<TComplex>,
        IInfrastructure<ComplexPropertyBuilder>
    {
        protected ComplexPropertyBuilder PropertyBuilder { get; } = complexPropertyBuilder;

        public override IMutableComplexProperty Metadata
            => PropertyBuilder.Metadata;

        protected virtual NonGenericTestComplexPropertyBuilder<T> Wrap<T>(ComplexPropertyBuilder entityTypeBuilder)
            => new(entityTypeBuilder);

        protected virtual TestComplexTypePropertyBuilder<TProperty> Wrap<TProperty>(ComplexTypePropertyBuilder propertyBuilder)
            => new NonGenericTestComplexTypePropertyBuilder<TProperty>(propertyBuilder);

        protected virtual TestComplexTypePrimitiveCollectionBuilder<TProperty> Wrap<TProperty>(
            ComplexTypePrimitiveCollectionBuilder propertyBuilder)
            => new NonGenericTestComplexTypePrimitiveCollectionBuilder<TProperty>(propertyBuilder);

        public override TestComplexPropertyBuilder<TComplex> HasPropertyAnnotation(string annotation, object? value)
            => Wrap<TComplex>(PropertyBuilder.HasPropertyAnnotation(annotation, value));

        public override TestComplexPropertyBuilder<TComplex> HasTypeAnnotation(string annotation, object? value)
            => Wrap<TComplex>(PropertyBuilder.HasTypeAnnotation(annotation, value));

        public override TestComplexTypePropertyBuilder<TProperty> Property<TProperty>(
            Expression<Func<TComplex, TProperty>> propertyExpression)
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            return Wrap<TProperty>(PropertyBuilder.Property(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName()));
        }

        public override TestComplexTypePropertyBuilder<TProperty> Property<TProperty>(string propertyName)
            => Wrap<TProperty>(PropertyBuilder.Property<TProperty>(propertyName));

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(
            Expression<Func<TComplex, TProperty>> propertyExpression)
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            return Wrap<TProperty>(PropertyBuilder.PrimitiveCollection(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName()));
        }

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(string propertyName)
            => Wrap<TProperty>(PropertyBuilder.PrimitiveCollection<TProperty>(propertyName));

        public override TestComplexTypePropertyBuilder<TProperty> IndexerProperty<TProperty>(string propertyName)
            => Wrap<TProperty>(PropertyBuilder.IndexerProperty<TProperty>(propertyName));

        public override TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(string propertyName)
            => Wrap<TProperty>(PropertyBuilder.ComplexProperty<TProperty>(propertyName));

        public override TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
            Expression<Func<TComplex, TProperty?>> propertyExpression)
            where TProperty : default
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            return Wrap<TProperty>(PropertyBuilder.ComplexProperty(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName()));
        }

        public override TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
            Expression<Func<TComplex, TProperty?>> propertyExpression,
            string complexTypeName)
            where TProperty : default
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            return Wrap<TProperty>(
                PropertyBuilder.ComplexProperty(
                    memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName(), complexTypeName));
        }

        public override TestComplexPropertyBuilder<TComplex> ComplexProperty<TProperty>(
            string propertyName,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction)
        {
            buildAction(Wrap<TProperty>(PropertyBuilder.ComplexProperty<TProperty>(propertyName)));

            return this;
        }

        public override TestComplexPropertyBuilder<TComplex> ComplexProperty<TProperty>(
            Expression<Func<TComplex, TProperty?>> propertyExpression,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction)
            where TProperty : default
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            buildAction(Wrap<TProperty>(PropertyBuilder.ComplexProperty(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName())));

            return this;
        }

        public override TestComplexPropertyBuilder<TComplex> ComplexProperty<TProperty>(
            Expression<Func<TComplex, TProperty?>> propertyExpression,
            string complexTypeName,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction)
            where TProperty : default
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            buildAction(
                Wrap<TProperty>(
                    PropertyBuilder.ComplexProperty(
                        memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName(), complexTypeName)));

            return this;
        }

        public override TestComplexPropertyBuilder<TComplex> Ignore(Expression<Func<TComplex, object?>> propertyExpression)
            => Wrap<TComplex>(PropertyBuilder.Ignore(propertyExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestComplexPropertyBuilder<TComplex> Ignore(string propertyName)
            => Wrap<TComplex>(PropertyBuilder.Ignore(propertyName));

        public override TestComplexPropertyBuilder<TComplex> IsRequired(bool isRequired = true)
            => Wrap<TComplex>(PropertyBuilder.IsRequired(isRequired));

        public override TestComplexPropertyBuilder<TComplex> HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
            => Wrap<TComplex>(PropertyBuilder.HasChangeTrackingStrategy(changeTrackingStrategy));

        public override TestComplexPropertyBuilder<TComplex> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => Wrap<TComplex>(PropertyBuilder.UsePropertyAccessMode(propertyAccessMode));

        public override TestComplexPropertyBuilder<TComplex> UseDefaultPropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => Wrap<TComplex>(PropertyBuilder.UseDefaultPropertyAccessMode(propertyAccessMode));

        public ComplexPropertyBuilder Instance
            => PropertyBuilder;
    }

    protected class NonGenericTestDiscriminatorBuilder<TDiscriminator>(DiscriminatorBuilder discriminatorBuilder) : TestDiscriminatorBuilder<TDiscriminator>
    {
        protected DiscriminatorBuilder DiscriminatorBuilder { get; } = discriminatorBuilder;

        protected virtual TestDiscriminatorBuilder<TDiscriminator> Wrap(DiscriminatorBuilder discriminatorBuilder)
            => new NonGenericTestDiscriminatorBuilder<TDiscriminator>(discriminatorBuilder);

        public override TestDiscriminatorBuilder<TDiscriminator> IsComplete(bool complete)
            => Wrap(DiscriminatorBuilder.IsComplete(complete));

        public override TestDiscriminatorBuilder<TDiscriminator> HasValue(TDiscriminator? value)
            => Wrap(DiscriminatorBuilder.HasValue(value));

        public override TestDiscriminatorBuilder<TDiscriminator> HasValue<TEntity>(TDiscriminator? value)
            => Wrap(DiscriminatorBuilder.HasValue<TEntity>(value));

        public override TestDiscriminatorBuilder<TDiscriminator> HasValue(Type entityType, TDiscriminator? value)
            => Wrap(DiscriminatorBuilder.HasValue(entityType, value));

        public override TestDiscriminatorBuilder<TDiscriminator> HasValue(string entityTypeName, TDiscriminator? value)
            => Wrap(DiscriminatorBuilder.HasValue(entityTypeName, value));
    }

    protected class NonGenericTestOwnedEntityTypeBuilder<TEntity>(OwnedEntityTypeBuilder ownedEntityTypeBuilder) : TestOwnedEntityTypeBuilder<TEntity>,
        IInfrastructure<OwnedEntityTypeBuilder>
        where TEntity : class
    {
        protected OwnedEntityTypeBuilder OwnedEntityTypeBuilder { get; } = ownedEntityTypeBuilder;

        public OwnedEntityTypeBuilder Instance
            => OwnedEntityTypeBuilder;
    }

    protected class NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder propertyBuilder) : TestPropertyBuilder<TProperty>, IInfrastructure<PropertyBuilder>
    {
        private PropertyBuilder PropertyBuilder { get; } = propertyBuilder;

        public override IMutableProperty Metadata
            => PropertyBuilder.Metadata;

        protected virtual TestPropertyBuilder<TProperty> Wrap(PropertyBuilder propertyBuilder)
            => new NonGenericTestPropertyBuilder<TProperty>(propertyBuilder);

        public override TestPropertyBuilder<TProperty> HasAnnotation(string annotation, object? value)
            => Wrap(PropertyBuilder.HasAnnotation(annotation, value));

        public override TestPropertyBuilder<TProperty> IsRequired(bool isRequired = true)
            => Wrap(PropertyBuilder.IsRequired(isRequired));

        public override TestPropertyBuilder<TProperty> HasMaxLength(int maxLength)
            => Wrap(PropertyBuilder.HasMaxLength(maxLength));

        public override TestPropertyBuilder<TProperty> HasSentinel(TProperty? sentinel)
            => Wrap(PropertyBuilder.HasSentinel(sentinel));

        public override TestPropertyBuilder<TProperty> HasPrecision(int precision)
            => Wrap(PropertyBuilder.HasPrecision(precision));

        public override TestPropertyBuilder<TProperty> HasPrecision(int precision, int scale)
            => Wrap(PropertyBuilder.HasPrecision(precision, scale));

        public override TestPropertyBuilder<TProperty> IsUnicode(bool unicode = true)
            => Wrap(PropertyBuilder.IsUnicode(unicode));

        public override TestPropertyBuilder<TProperty> IsRowVersion()
            => Wrap(PropertyBuilder.IsRowVersion());

        public override TestPropertyBuilder<TProperty> IsConcurrencyToken(bool isConcurrencyToken = true)
            => Wrap(PropertyBuilder.IsConcurrencyToken(isConcurrencyToken));

        public override TestPropertyBuilder<TProperty> ValueGeneratedNever()
            => Wrap(PropertyBuilder.ValueGeneratedNever());

        public override TestPropertyBuilder<TProperty> ValueGeneratedOnAdd()
            => Wrap(PropertyBuilder.ValueGeneratedOnAdd());

        public override TestPropertyBuilder<TProperty> ValueGeneratedOnAddOrUpdate()
            => Wrap(PropertyBuilder.ValueGeneratedOnAddOrUpdate());

        public override TestPropertyBuilder<TProperty> ValueGeneratedOnUpdate()
            => Wrap(PropertyBuilder.ValueGeneratedOnUpdate());

        public override TestPropertyBuilder<TProperty> HasValueGenerator<TGenerator>()
            => Wrap(PropertyBuilder.HasValueGenerator<TGenerator>());

        public override TestPropertyBuilder<TProperty> HasValueGenerator(Type valueGeneratorType)
            => Wrap(PropertyBuilder.HasValueGenerator(valueGeneratorType));

        public override TestPropertyBuilder<TProperty> HasValueGenerator(
            Func<IReadOnlyProperty, ITypeBase, ValueGenerator> factory)
            => Wrap(PropertyBuilder.HasValueGenerator(factory));

        public override TestPropertyBuilder<TProperty> HasValueGeneratorFactory<TFactory>()
            => Wrap(PropertyBuilder.HasValueGeneratorFactory<TFactory>());

        public override TestPropertyBuilder<TProperty> HasValueGeneratorFactory(Type valueGeneratorFactoryType)
            => Wrap(PropertyBuilder.HasValueGeneratorFactory(valueGeneratorFactoryType));

        public override TestPropertyBuilder<TProperty> HasField(string fieldName)
            => Wrap(PropertyBuilder.HasField(fieldName));

        public override TestPropertyBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => Wrap(PropertyBuilder.UsePropertyAccessMode(propertyAccessMode));

        public override TestPropertyBuilder<TProperty> HasConversion<TConversion>()
            => Wrap(PropertyBuilder.HasConversion(typeof(TConversion)));

        public override TestPropertyBuilder<TProperty> HasConversion<TConversion>(ValueComparer? valueComparer)
            => Wrap(PropertyBuilder.HasConversion(typeof(TConversion), valueComparer));

        public override TestPropertyBuilder<TProperty> HasConversion<TConversion>(
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType)
            => Wrap(PropertyBuilder.HasConversion(typeof(TConversion), valueComparer, providerComparerType));

        public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression)
            => Wrap(
                PropertyBuilder.HasConversion(
                    new ValueConverter<TProperty, TProvider>(convertToProviderExpression, convertFromProviderExpression)));

        public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            ValueComparer? valueComparer)
            => Wrap(
                PropertyBuilder.HasConversion(
                    new ValueConverter<TProperty, TProvider>(convertToProviderExpression, convertFromProviderExpression),
                    valueComparer));

        public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType)
            => Wrap(
                PropertyBuilder.HasConversion(
                    new ValueConverter<TProperty, TProvider>(convertToProviderExpression, convertFromProviderExpression),
                    valueComparer,
                    providerComparerType));

        public override TestPropertyBuilder<TProperty> HasConversion<TStore>(ValueConverter<TProperty, TStore> converter)
            => Wrap(PropertyBuilder.HasConversion(converter));

        public override TestPropertyBuilder<TProperty> HasConversion<TStore>(
            ValueConverter<TProperty, TStore> converter,
            ValueComparer? valueComparer)
            => Wrap(PropertyBuilder.HasConversion(converter, valueComparer));

        public override TestPropertyBuilder<TProperty> HasConversion<TStore>(
            ValueConverter<TProperty, TStore> converter,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType)
            => Wrap(PropertyBuilder.HasConversion(converter, valueComparer, providerComparerType));

        public override TestPropertyBuilder<TProperty> HasConversion(ValueConverter? converter)
            => Wrap(PropertyBuilder.HasConversion(converter));

        public override TestPropertyBuilder<TProperty> HasConversion(ValueConverter? converter, ValueComparer? valueComparer)
            => Wrap(PropertyBuilder.HasConversion(converter, valueComparer));

        public override TestPropertyBuilder<TProperty> HasConversion(
            ValueConverter? converter,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType)
            => Wrap(PropertyBuilder.HasConversion(converter, valueComparer, providerComparerType));

        public override TestPropertyBuilder<TProperty> HasConversion<TConverter, TComparer>()
            => Wrap(PropertyBuilder.HasConversion<TConverter, TComparer>());

        public override TestPropertyBuilder<TProperty> HasConversion<TConverter, TComparer, TProviderComparer>()
            => Wrap(PropertyBuilder.HasConversion<TConverter, TComparer, TProviderComparer>());

        PropertyBuilder IInfrastructure<PropertyBuilder>.Instance
            => PropertyBuilder;
    }

    protected class NonGenericTestPrimitiveCollectionBuilder<TProperty>(PrimitiveCollectionBuilder primitiveCollectionBuilder) : TestPrimitiveCollectionBuilder<TProperty>,
        IInfrastructure<PrimitiveCollectionBuilder>
    {
        private PrimitiveCollectionBuilder PrimitiveCollectionBuilder { get; } = primitiveCollectionBuilder;

        public override IMutableProperty Metadata
            => PrimitiveCollectionBuilder.Metadata;

        public override TestElementTypeBuilder ElementType()
            => new(PrimitiveCollectionBuilder.ElementType());

        public override TestPrimitiveCollectionBuilder<TProperty> ElementType(Action<TestElementTypeBuilder> builderAction)
            => Wrap(PrimitiveCollectionBuilder.ElementType(b => builderAction(new TestElementTypeBuilder(b))));

        protected virtual TestPrimitiveCollectionBuilder<TProperty> Wrap(PrimitiveCollectionBuilder primitiveCollectionBuilder)
            => new NonGenericTestPrimitiveCollectionBuilder<TProperty>(primitiveCollectionBuilder);

        public override TestPrimitiveCollectionBuilder<TProperty> HasAnnotation(string annotation, object? value)
            => Wrap(PrimitiveCollectionBuilder.HasAnnotation(annotation, value));

        public override TestPrimitiveCollectionBuilder<TProperty> IsRequired(bool isRequired = true)
            => Wrap(PrimitiveCollectionBuilder.IsRequired(isRequired));

        public override TestPrimitiveCollectionBuilder<TProperty> HasMaxLength(int maxLength)
            => Wrap(PrimitiveCollectionBuilder.HasMaxLength(maxLength));

        public override TestPrimitiveCollectionBuilder<TProperty> HasSentinel(TProperty? sentinel)
            => Wrap(PrimitiveCollectionBuilder.HasSentinel(sentinel));

        public override TestPrimitiveCollectionBuilder<TProperty> IsUnicode(bool unicode = true)
            => Wrap(PrimitiveCollectionBuilder.IsUnicode(unicode));

        public override TestPrimitiveCollectionBuilder<TProperty> IsConcurrencyToken(bool isConcurrencyToken = true)
            => Wrap(PrimitiveCollectionBuilder.IsConcurrencyToken(isConcurrencyToken));

        public override TestPrimitiveCollectionBuilder<TProperty> ValueGeneratedNever()
            => Wrap(PrimitiveCollectionBuilder.ValueGeneratedNever());

        public override TestPrimitiveCollectionBuilder<TProperty> ValueGeneratedOnAdd()
            => Wrap(PrimitiveCollectionBuilder.ValueGeneratedOnAdd());

        public override TestPrimitiveCollectionBuilder<TProperty> ValueGeneratedOnAddOrUpdate()
            => Wrap(PrimitiveCollectionBuilder.ValueGeneratedOnAddOrUpdate());

        public override TestPrimitiveCollectionBuilder<TProperty> ValueGeneratedOnUpdate()
            => Wrap(PrimitiveCollectionBuilder.ValueGeneratedOnUpdate());

        public override TestPrimitiveCollectionBuilder<TProperty> HasValueGenerator<TGenerator>()
            => Wrap(PrimitiveCollectionBuilder.HasValueGenerator<TGenerator>());

        public override TestPrimitiveCollectionBuilder<TProperty> HasValueGenerator(Type valueGeneratorType)
            => Wrap(PrimitiveCollectionBuilder.HasValueGenerator(valueGeneratorType));

        public override TestPrimitiveCollectionBuilder<TProperty> HasValueGeneratorFactory<TFactory>()
            => Wrap(PrimitiveCollectionBuilder.HasValueGeneratorFactory<TFactory>());

        public override TestPrimitiveCollectionBuilder<TProperty> HasValueGeneratorFactory(Type valueGeneratorFactoryType)
            => Wrap(PrimitiveCollectionBuilder.HasValueGeneratorFactory(valueGeneratorFactoryType));

        public override TestPrimitiveCollectionBuilder<TProperty> HasField(string fieldName)
            => Wrap(PrimitiveCollectionBuilder.HasField(fieldName));

        public override TestPrimitiveCollectionBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => Wrap(PrimitiveCollectionBuilder.UsePropertyAccessMode(propertyAccessMode));

        PrimitiveCollectionBuilder IInfrastructure<PrimitiveCollectionBuilder>.Instance
            => PrimitiveCollectionBuilder;
    }

    protected class NonGenericTestComplexTypePropertyBuilder<TProperty>(ComplexTypePropertyBuilder propertyBuilder) :
        TestComplexTypePropertyBuilder<TProperty>,
        IInfrastructure<ComplexTypePropertyBuilder>
    {
        private ComplexTypePropertyBuilder PropertyBuilder { get; } = propertyBuilder;

        public override IMutableProperty Metadata
            => PropertyBuilder.Metadata;

        protected virtual TestComplexTypePropertyBuilder<TProperty> Wrap(ComplexTypePropertyBuilder propertyBuilder)
            => new NonGenericTestComplexTypePropertyBuilder<TProperty>(propertyBuilder);

        public override TestComplexTypePropertyBuilder<TProperty> HasAnnotation(string annotation, object? value)
            => Wrap(PropertyBuilder.HasAnnotation(annotation, value));

        public override TestComplexTypePropertyBuilder<TProperty> IsRequired(bool isRequired = true)
            => Wrap(PropertyBuilder.IsRequired(isRequired));

        public override TestComplexTypePropertyBuilder<TProperty> HasMaxLength(int maxLength)
            => Wrap(PropertyBuilder.HasMaxLength(maxLength));

        public override TestComplexTypePropertyBuilder<TProperty> HasSentinel(TProperty? sentinel)
            => Wrap(PropertyBuilder.HasSentinel(sentinel));

        public override TestComplexTypePropertyBuilder<TProperty> HasPrecision(int precision)
            => Wrap(PropertyBuilder.HasPrecision(precision));

        public override TestComplexTypePropertyBuilder<TProperty> HasPrecision(int precision, int scale)
            => Wrap(PropertyBuilder.HasPrecision(precision, scale));

        public override TestComplexTypePropertyBuilder<TProperty> IsUnicode(bool unicode = true)
            => Wrap(PropertyBuilder.IsUnicode(unicode));

        public override TestComplexTypePropertyBuilder<TProperty> IsRowVersion()
            => Wrap(PropertyBuilder.IsRowVersion());

        public override TestComplexTypePropertyBuilder<TProperty> IsConcurrencyToken(bool isConcurrencyToken = true)
            => Wrap(PropertyBuilder.IsConcurrencyToken(isConcurrencyToken));

        public override TestComplexTypePropertyBuilder<TProperty> ValueGeneratedNever()
            => Wrap(PropertyBuilder.ValueGeneratedNever());

        public override TestComplexTypePropertyBuilder<TProperty> ValueGeneratedOnAdd()
            => Wrap(PropertyBuilder.ValueGeneratedOnAdd());

        public override TestComplexTypePropertyBuilder<TProperty> ValueGeneratedOnAddOrUpdate()
            => Wrap(PropertyBuilder.ValueGeneratedOnAddOrUpdate());

        public override TestComplexTypePropertyBuilder<TProperty> ValueGeneratedOnUpdate()
            => Wrap(PropertyBuilder.ValueGeneratedOnUpdate());

        public override TestComplexTypePropertyBuilder<TProperty> HasValueGenerator<TGenerator>()
            => Wrap(PropertyBuilder.HasValueGenerator<TGenerator>());

        public override TestComplexTypePropertyBuilder<TProperty> HasValueGenerator(Type valueGeneratorType)
            => Wrap(PropertyBuilder.HasValueGenerator(valueGeneratorType));

        public override TestComplexTypePropertyBuilder<TProperty> HasValueGeneratorFactory<TFactory>()
            => Wrap(PropertyBuilder.HasValueGeneratorFactory<TFactory>());

        public override TestComplexTypePropertyBuilder<TProperty> HasValueGeneratorFactory(Type valueGeneratorFactoryType)
            => Wrap(PropertyBuilder.HasValueGeneratorFactory(valueGeneratorFactoryType));

        public override TestComplexTypePropertyBuilder<TProperty> HasField(string fieldName)
            => Wrap(PropertyBuilder.HasField(fieldName));

        public override TestComplexTypePropertyBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => Wrap(PropertyBuilder.UsePropertyAccessMode(propertyAccessMode));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TConversion>()
            => Wrap(PropertyBuilder.HasConversion(typeof(TConversion)));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TConversion>(ValueComparer? valueComparer)
            => Wrap(PropertyBuilder.HasConversion(typeof(TConversion), valueComparer));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TConversion>(
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType)
            => Wrap(PropertyBuilder.HasConversion(typeof(TConversion), valueComparer, providerComparerType));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression)
            => Wrap(
                PropertyBuilder.HasConversion(
                    new ValueConverter<TProperty, TProvider>(convertToProviderExpression, convertFromProviderExpression)));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            ValueComparer? valueComparer)
            => Wrap(
                PropertyBuilder.HasConversion(
                    new ValueConverter<TProperty, TProvider>(convertToProviderExpression, convertFromProviderExpression),
                    valueComparer));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType)
            => Wrap(
                PropertyBuilder.HasConversion(
                    new ValueConverter<TProperty, TProvider>(convertToProviderExpression, convertFromProviderExpression),
                    valueComparer,
                    providerComparerType));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TStore>(ValueConverter<TProperty, TStore> converter)
            => Wrap(PropertyBuilder.HasConversion(converter));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TStore>(
            ValueConverter<TProperty, TStore> converter,
            ValueComparer? valueComparer)
            => Wrap(PropertyBuilder.HasConversion(converter, valueComparer));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TStore>(
            ValueConverter<TProperty, TStore> converter,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType)
            => Wrap(PropertyBuilder.HasConversion(converter, valueComparer, providerComparerType));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion(ValueConverter? converter)
            => Wrap(PropertyBuilder.HasConversion(converter));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion(ValueConverter? converter, ValueComparer? valueComparer)
            => Wrap(PropertyBuilder.HasConversion(converter, valueComparer));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion(
            ValueConverter? converter,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType)
            => Wrap(PropertyBuilder.HasConversion(converter, valueComparer, providerComparerType));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TConverter, TComparer>()
            => Wrap(PropertyBuilder.HasConversion<TConverter, TComparer>());

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TConverter, TComparer, TProviderComparer>()
            => Wrap(PropertyBuilder.HasConversion<TConverter, TComparer, TProviderComparer>());

        ComplexTypePropertyBuilder IInfrastructure<ComplexTypePropertyBuilder>.Instance
            => PropertyBuilder;
    }

    protected class NonGenericTestComplexTypePrimitiveCollectionBuilder<TProperty>(ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder) :
        TestComplexTypePrimitiveCollectionBuilder<TProperty>,
        IInfrastructure<ComplexTypePrimitiveCollectionBuilder>
    {
        private ComplexTypePrimitiveCollectionBuilder PrimitiveCollectionBuilder { get; } = primitiveCollectionBuilder;

        public override IMutableProperty Metadata
            => PrimitiveCollectionBuilder.Metadata;

        protected virtual TestComplexTypePrimitiveCollectionBuilder<TProperty> Wrap(
            ComplexTypePrimitiveCollectionBuilder primitiveCollectionBuilder)
            => new NonGenericTestComplexTypePrimitiveCollectionBuilder<TProperty>(primitiveCollectionBuilder);

        public override TestElementTypeBuilder ElementType()
            => new(PrimitiveCollectionBuilder.ElementType());

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> ElementType(Action<TestElementTypeBuilder> builderAction)
            => Wrap(PrimitiveCollectionBuilder.ElementType(b => builderAction(new TestElementTypeBuilder(b))));

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> HasAnnotation(string annotation, object? value)
            => Wrap(PrimitiveCollectionBuilder.HasAnnotation(annotation, value));

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> IsRequired(bool isRequired = true)
            => Wrap(PrimitiveCollectionBuilder.IsRequired(isRequired));

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> HasMaxLength(int maxLength)
            => Wrap(PrimitiveCollectionBuilder.HasMaxLength(maxLength));

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> HasSentinel(TProperty? sentinel)
            => Wrap(PrimitiveCollectionBuilder.HasSentinel(sentinel));

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> IsUnicode(bool unicode = true)
            => Wrap(PrimitiveCollectionBuilder.IsUnicode(unicode));

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> IsConcurrencyToken(bool isConcurrencyToken = true)
            => Wrap(PrimitiveCollectionBuilder.IsConcurrencyToken(isConcurrencyToken));

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> ValueGeneratedNever()
            => Wrap(PrimitiveCollectionBuilder.ValueGeneratedNever());

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> ValueGeneratedOnAdd()
            => Wrap(PrimitiveCollectionBuilder.ValueGeneratedOnAdd());

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> ValueGeneratedOnAddOrUpdate()
            => Wrap(PrimitiveCollectionBuilder.ValueGeneratedOnAddOrUpdate());

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> ValueGeneratedOnUpdate()
            => Wrap(PrimitiveCollectionBuilder.ValueGeneratedOnUpdate());

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> HasValueGenerator<TGenerator>()
            => Wrap(PrimitiveCollectionBuilder.HasValueGenerator<TGenerator>());

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> HasValueGenerator(Type valueGeneratorType)
            => Wrap(PrimitiveCollectionBuilder.HasValueGenerator(valueGeneratorType));

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> HasValueGeneratorFactory<TFactory>()
            => Wrap(PrimitiveCollectionBuilder.HasValueGeneratorFactory<TFactory>());

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> HasValueGeneratorFactory(Type valueGeneratorFactoryType)
            => Wrap(PrimitiveCollectionBuilder.HasValueGeneratorFactory(valueGeneratorFactoryType));

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> HasField(string fieldName)
            => Wrap(PrimitiveCollectionBuilder.HasField(fieldName));

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => Wrap(PrimitiveCollectionBuilder.UsePropertyAccessMode(propertyAccessMode));

        ComplexTypePrimitiveCollectionBuilder IInfrastructure<ComplexTypePrimitiveCollectionBuilder>.Instance
            => PrimitiveCollectionBuilder;
    }

    protected class NonGenericTestNavigationBuilder(NavigationBuilder navigationBuilder) : TestNavigationBuilder
    {
        private NavigationBuilder NavigationBuilder { get; } = navigationBuilder;

        public override TestNavigationBuilder HasAnnotation(string annotation, object? value)
            => new NonGenericTestNavigationBuilder(NavigationBuilder.HasAnnotation(annotation, value));

        public override TestNavigationBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => new NonGenericTestNavigationBuilder(NavigationBuilder.UsePropertyAccessMode(propertyAccessMode));

        public override TestNavigationBuilder HasField(string fieldName)
            => new NonGenericTestNavigationBuilder(NavigationBuilder.HasField(fieldName));

        public override TestNavigationBuilder AutoInclude(bool autoInclude = true)
            => new NonGenericTestNavigationBuilder(NavigationBuilder.AutoInclude(autoInclude));

        public override TestNavigationBuilder EnableLazyLoading(bool lazyLoadingEnabled = true)
            => new NonGenericTestNavigationBuilder(NavigationBuilder.EnableLazyLoading(lazyLoadingEnabled));

        public override TestNavigationBuilder IsRequired(bool required = true)
            => new NonGenericTestNavigationBuilder(NavigationBuilder.IsRequired(required));
    }

    protected class NonGenericTestKeyBuilder<TEntity>(KeyBuilder keyBuilder) : TestKeyBuilder<TEntity>, IInfrastructure<KeyBuilder>
    {
        private KeyBuilder KeyBuilder { get; } = keyBuilder;

        public override IMutableKey Metadata
            => KeyBuilder.Metadata;

        public override TestKeyBuilder<TEntity> HasAnnotation(string annotation, object? value)
            => new NonGenericTestKeyBuilder<TEntity>(KeyBuilder.HasAnnotation(annotation, value));

        KeyBuilder IInfrastructure<KeyBuilder>.Instance
            => KeyBuilder;
    }

    protected class NonGenericTestIndexBuilder<TEntity>(IndexBuilder indexBuilder) : TestIndexBuilder<TEntity>, IInfrastructure<IndexBuilder>
    {
        private IndexBuilder IndexBuilder { get; } = indexBuilder;

        public override IMutableIndex Metadata
            => IndexBuilder.Metadata;

        public override TestIndexBuilder<TEntity> HasAnnotation(string annotation, object? value)
            => new NonGenericTestIndexBuilder<TEntity>(IndexBuilder.HasAnnotation(annotation, value));

        public override TestIndexBuilder<TEntity> IsUnique(bool isUnique = true)
            => new NonGenericTestIndexBuilder<TEntity>(IndexBuilder.IsUnique(isUnique));

        public override TestIndexBuilder<TEntity> IsDescending(params bool[] isDescending)
            => new NonGenericTestIndexBuilder<TEntity>(IndexBuilder.IsDescending(isDescending));

        IndexBuilder IInfrastructure<IndexBuilder>.Instance
            => IndexBuilder;
    }

    protected class
        NonGenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(ReferenceNavigationBuilder referenceNavigationBuilder) : TestReferenceNavigationBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        protected ReferenceNavigationBuilder ReferenceNavigationBuilder { get; } = referenceNavigationBuilder;

        public override TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(string? navigationName)
            => new NonGenericTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(
                ReferenceNavigationBuilder.WithMany(navigationName));

        public override TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(
            Expression<Func<TRelatedEntity, IEnumerable<TEntity>?>>? navigationExpression = null)
            => new NonGenericTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(
                ReferenceNavigationBuilder.WithMany(
                    navigationExpression?.GetMemberAccess().GetSimpleMemberName()));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(string? navigationName)
            => new NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                ReferenceNavigationBuilder.WithOne(navigationName));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
            Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression = null)
            => new NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                ReferenceNavigationBuilder.WithOne(
                    navigationExpression?.GetMemberAccess().GetSimpleMemberName()));
    }

    protected class NonGenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(CollectionNavigationBuilder collectionNavigationBuilder)
        : TestCollectionNavigationBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        private CollectionNavigationBuilder CollectionNavigationBuilder { get; } = collectionNavigationBuilder;

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(
            string? navigationName)
            => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                CollectionNavigationBuilder.WithOne(navigationName));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(
            Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression = null)
            => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                CollectionNavigationBuilder.WithOne(
                    navigationExpression?.GetMemberAccess().GetSimpleMemberName()));

        public override TestCollectionCollectionBuilder<TRelatedEntity, TEntity> WithMany(
            string? navigationName = null)
            => new NonGenericTestCollectionCollectionBuilder<TRelatedEntity, TEntity>(
                CollectionNavigationBuilder.WithMany(navigationName));

        public override TestCollectionCollectionBuilder<TRelatedEntity, TEntity> WithMany(
            Expression<Func<TRelatedEntity, IEnumerable<TEntity>?>> navigationExpression)
            => new NonGenericTestCollectionCollectionBuilder<TRelatedEntity, TEntity>(
                CollectionNavigationBuilder.WithMany(navigationExpression.GetMemberAccess().GetSimpleMemberName()));
    }

    protected class NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder referenceCollectionBuilder)
        : TestReferenceCollectionBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        public ReferenceCollectionBuilder ReferenceCollectionBuilder { get; } = referenceCollectionBuilder;

        public override IMutableForeignKey Metadata
            => ReferenceCollectionBuilder.Metadata;

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(
            Expression<Func<TRelatedEntity, object?>> foreignKeyExpression)
            => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                ReferenceCollectionBuilder.HasForeignKey(
                    foreignKeyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
            Expression<Func<TEntity, object?>> keyExpression)
            => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                ReferenceCollectionBuilder.HasPrincipalKey(
                    keyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(params string[] foreignKeyPropertyNames)
            => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                ReferenceCollectionBuilder.HasForeignKey(foreignKeyPropertyNames));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(params string[] keyPropertyNames)
            => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                ReferenceCollectionBuilder.HasPrincipalKey(keyPropertyNames));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasAnnotation(string annotation, object? value)
            => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                ReferenceCollectionBuilder.HasAnnotation(annotation, value));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> IsRequired(bool isRequired = true)
            => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.IsRequired(isRequired));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior)
            => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                ReferenceCollectionBuilder.OnDelete(deleteBehavior));
    }

    protected class
        NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(ReferenceReferenceBuilder referenceReferenceBuilder) : TestReferenceReferenceBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        protected ReferenceReferenceBuilder ReferenceReferenceBuilder { get; } = referenceReferenceBuilder;

        public override IMutableForeignKey Metadata
            => ReferenceReferenceBuilder.Metadata;

        protected virtual NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(
            ReferenceReferenceBuilder referenceReferenceBuilder)
            => new(referenceReferenceBuilder);

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasAnnotation(string annotation, object? value)
            => Wrap(ReferenceReferenceBuilder.HasAnnotation(annotation, value));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
            Expression<Func<TDependentEntity, object?>> foreignKeyExpression)
            => Wrap(
                ReferenceReferenceBuilder.HasForeignKey(
                    typeof(TDependentEntity),
                    foreignKeyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
            Expression<Func<TPrincipalEntity, object?>> keyExpression)
            => Wrap(
                ReferenceReferenceBuilder.HasPrincipalKey(
                    typeof(TPrincipalEntity), keyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
            params string[] foreignKeyPropertyNames)
            => Wrap(ReferenceReferenceBuilder.HasForeignKey(typeof(TDependentEntity), foreignKeyPropertyNames));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
            params string[] keyPropertyNames)
            => Wrap(ReferenceReferenceBuilder.HasPrincipalKey(typeof(TPrincipalEntity), keyPropertyNames));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> IsRequired(bool isRequired = true)
            => Wrap(ReferenceReferenceBuilder.IsRequired(isRequired));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior)
            => Wrap(ReferenceReferenceBuilder.OnDelete(deleteBehavior));
    }

    protected class NonGenericTestCollectionCollectionBuilder<TLeftEntity, TRightEntity>(CollectionCollectionBuilder collectionCollectionBuilder) :
        TestCollectionCollectionBuilder<TLeftEntity, TRightEntity>
        where TLeftEntity : class
        where TRightEntity : class
    {
        protected CollectionCollectionBuilder CollectionCollectionBuilder { get; } = collectionCollectionBuilder;

        public override TestEntityTypeBuilder<Dictionary<string, object>> UsingEntity(
            string joinEntityName)
            => new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(
                CollectionCollectionBuilder.UsingEntity(joinEntityName));

        public override TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>()
            => new NonGenericTestEntityTypeBuilder<TJoinEntity>(
                CollectionCollectionBuilder.UsingEntity(typeof(TJoinEntity)));

        public override TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            string joinEntityName)
            => new NonGenericTestEntityTypeBuilder<TJoinEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    joinEntityName,
                    typeof(TJoinEntity)));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity(
            Action<TestEntityTypeBuilder<Dictionary<string, object>>> configureJoinEntityType)
            => new NonGenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    e => configureJoinEntityType(new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(e))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity(
            string joinEntityName,
            Action<TestEntityTypeBuilder<Dictionary<string, object>>> configureJoinEntityType)
            => new NonGenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    joinEntityName,
                    e => configureJoinEntityType(new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(e))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            => new NonGenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    typeof(TJoinEntity),
                    e => configureJoinEntityType(new NonGenericTestEntityTypeBuilder<TJoinEntity>(e))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            string joinEntityName,
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            => new NonGenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    joinEntityName,
                    typeof(TJoinEntity),
                    e => configureJoinEntityType(new NonGenericTestEntityTypeBuilder<TJoinEntity>(e))));

        public override TestEntityTypeBuilder<Dictionary<string, object>> UsingEntity(
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>> configureRight,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>> configureLeft)
            => new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(
                CollectionCollectionBuilder.UsingEntity(
                    l => ((NonGenericTestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>)configureRight(
                        new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(l))).ReferenceCollectionBuilder,
                    r => ((NonGenericTestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>)configureLeft(
                        new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(r))).ReferenceCollectionBuilder));

        public override TestEntityTypeBuilder<Dictionary<string, object>> UsingEntity(
            string joinEntityName,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>> configureRight,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>> configureLeft)
            => new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(
                CollectionCollectionBuilder.UsingEntity(
                    joinEntityName,
                    l => ((NonGenericTestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>)configureRight(
                        new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(l))).ReferenceCollectionBuilder,
                    r => ((NonGenericTestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>)configureLeft(
                        new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(r))).ReferenceCollectionBuilder));

        public override TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft)
            => new NonGenericTestEntityTypeBuilder<TJoinEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    typeof(TJoinEntity),
                    l => ((NonGenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                        new NonGenericTestEntityTypeBuilder<TJoinEntity>(l))).ReferenceCollectionBuilder,
                    r => ((NonGenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                        new NonGenericTestEntityTypeBuilder<TJoinEntity>(r))).ReferenceCollectionBuilder));

        public override TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            string joinEntityName,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft)
            => new NonGenericTestEntityTypeBuilder<TJoinEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    joinEntityName,
                    typeof(TJoinEntity),
                    l => ((NonGenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                        new NonGenericTestEntityTypeBuilder<TJoinEntity>(l))).ReferenceCollectionBuilder,
                    r => ((NonGenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                        new NonGenericTestEntityTypeBuilder<TJoinEntity>(r))).ReferenceCollectionBuilder));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity(
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>> configureRight,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>> configureLeft,
            Action<TestEntityTypeBuilder<Dictionary<string, object>>> configureJoinEntityType)
            => new NonGenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    l => ((NonGenericTestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>)configureRight(
                        new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(l))).ReferenceCollectionBuilder,
                    r => ((NonGenericTestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>)configureLeft(
                        new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(r))).ReferenceCollectionBuilder,
                    e => configureJoinEntityType(new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(e))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity(
            string joinEntityName,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>> configureRight,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>> configureLeft,
            Action<TestEntityTypeBuilder<Dictionary<string, object>>> configureJoinEntityType)
            => new NonGenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    joinEntityName,
                    l => ((NonGenericTestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>)configureRight(
                        new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(l))).ReferenceCollectionBuilder,
                    r => ((NonGenericTestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>)configureLeft(
                        new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(r))).ReferenceCollectionBuilder,
                    e => configureJoinEntityType(new NonGenericTestEntityTypeBuilder<Dictionary<string, object>>(e))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            where TJoinEntity : class
            => new NonGenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    typeof(TJoinEntity),
                    l => ((NonGenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                        new NonGenericTestEntityTypeBuilder<TJoinEntity>(l))).ReferenceCollectionBuilder,
                    r => ((NonGenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                        new NonGenericTestEntityTypeBuilder<TJoinEntity>(r))).ReferenceCollectionBuilder,
                    e => configureJoinEntityType(new NonGenericTestEntityTypeBuilder<TJoinEntity>(e))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            string joinEntityName,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            where TJoinEntity : class
            => new NonGenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    joinEntityName,
                    typeof(TJoinEntity),
                    l => ((NonGenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                        new NonGenericTestEntityTypeBuilder<TJoinEntity>(l))).ReferenceCollectionBuilder,
                    r => ((NonGenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                        new NonGenericTestEntityTypeBuilder<TJoinEntity>(r))).ReferenceCollectionBuilder,
                    e => configureJoinEntityType(new NonGenericTestEntityTypeBuilder<TJoinEntity>(e))));
    }

    protected class NonGenericTestOwnershipBuilder<TEntity, TRelatedEntity>(OwnershipBuilder ownershipBuilder)
        : TestOwnershipBuilder<TEntity, TRelatedEntity>, IInfrastructure<OwnershipBuilder>
        where TEntity : class
        where TRelatedEntity : class
    {
        protected OwnershipBuilder OwnershipBuilder { get; } = ownershipBuilder;

        public override IMutableForeignKey Metadata
            => OwnershipBuilder.Metadata;

        protected virtual NonGenericTestOwnershipBuilder<TNewEntity, TNewRelatedEntity> Wrap<TNewEntity, TNewRelatedEntity>(
            OwnershipBuilder ownershipBuilder)
            where TNewEntity : class
            where TNewRelatedEntity : class
            => new(ownershipBuilder);

        public override TestOwnershipBuilder<TEntity, TRelatedEntity> HasAnnotation(string annotation, object? value)
            => Wrap<TEntity, TRelatedEntity>(OwnershipBuilder.HasAnnotation(annotation, value));

        public override TestOwnershipBuilder<TEntity, TRelatedEntity> HasForeignKey(
            params string[] foreignKeyPropertyNames)
            => Wrap<TEntity, TRelatedEntity>(OwnershipBuilder.HasForeignKey(foreignKeyPropertyNames));

        public override TestOwnershipBuilder<TEntity, TRelatedEntity> HasForeignKey(
            Expression<Func<TRelatedEntity, object?>> foreignKeyExpression)
            => Wrap<TEntity, TRelatedEntity>(
                OwnershipBuilder.HasForeignKey(
                    foreignKeyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestOwnershipBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
            params string[] keyPropertyNames)
            => Wrap<TEntity, TRelatedEntity>(OwnershipBuilder.HasPrincipalKey(keyPropertyNames));

        public override TestOwnershipBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
            Expression<Func<TEntity, object?>> keyExpression)
            => Wrap<TEntity, TRelatedEntity>(
                OwnershipBuilder.HasPrincipalKey(
                    keyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        OwnershipBuilder IInfrastructure<OwnershipBuilder>.Instance
            => OwnershipBuilder;
    }

    protected class NonGenericTestOwnedNavigationBuilder<TEntity, TDependentEntity>(OwnedNavigationBuilder ownedNavigationBuilder)
        : TestOwnedNavigationBuilder<TEntity, TDependentEntity>, IInfrastructure<OwnedNavigationBuilder>
        where TEntity : class
        where TDependentEntity : class
    {
        protected OwnedNavigationBuilder OwnedNavigationBuilder { get; } = ownedNavigationBuilder;

        public override IMutableForeignKey Metadata
            => OwnedNavigationBuilder.Metadata;

        public override IMutableEntityType OwnedEntityType
            => OwnedNavigationBuilder.OwnedEntityType;

        protected virtual NonGenericTestOwnedNavigationBuilder<TNewEntity, TNewDependentEntity> Wrap<TNewEntity, TNewDependentEntity>(
            OwnedNavigationBuilder ownedNavigationBuilder)
            where TNewEntity : class
            where TNewDependentEntity : class
            => new(ownedNavigationBuilder);

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> HasAnnotation(
            string annotation,
            object? value)
            => Wrap<TEntity, TDependentEntity>(OwnedNavigationBuilder.HasAnnotation(annotation, value));

        public override TestKeyBuilder<TDependentEntity> HasKey(Expression<Func<TDependentEntity, object?>> keyExpression)
            => new NonGenericTestKeyBuilder<TDependentEntity>(
                OwnedNavigationBuilder.HasKey(
                    keyExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestKeyBuilder<TDependentEntity> HasKey(params string[] propertyNames)
            => new NonGenericTestKeyBuilder<TDependentEntity>(OwnedNavigationBuilder.HasKey(propertyNames));

        public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
            => new NonGenericTestPropertyBuilder<TProperty>(OwnedNavigationBuilder.Property<TProperty>(propertyName));

        public override TestPropertyBuilder<TProperty> IndexerProperty<TProperty>(string propertyName)
            => new NonGenericTestPropertyBuilder<TProperty>(OwnedNavigationBuilder.IndexerProperty<TProperty>(propertyName));

        public override TestPropertyBuilder<TProperty> Property<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            return new NonGenericTestPropertyBuilder<TProperty>(
                OwnedNavigationBuilder.Property(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName()));
        }

        public override TestPrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(string propertyName)
            => new NonGenericTestPrimitiveCollectionBuilder<TProperty>(OwnedNavigationBuilder.PrimitiveCollection<TProperty>(propertyName));

        public override TestPrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
        {
            var memberInfo = propertyExpression.GetMemberAccess();
            return new NonGenericTestPrimitiveCollectionBuilder<TProperty>(
                OwnedNavigationBuilder.PrimitiveCollection(memberInfo.GetMemberType(), memberInfo.GetSimpleMemberName()));
        }

        public override TestNavigationBuilder Navigation<TNavigation>(
            Expression<Func<TDependentEntity, TNavigation?>> navigationExpression)
            where TNavigation : class
            => new NonGenericTestNavigationBuilder(
                OwnedNavigationBuilder.Navigation(navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestNavigationBuilder Navigation<TNavigation>(
            Expression<Func<TDependentEntity, IEnumerable<TNavigation>?>> navigationExpression)
            where TNavigation : class
            => new NonGenericTestNavigationBuilder(
                OwnedNavigationBuilder.Navigation(navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> Ignore(string propertyName)
            => Wrap<TEntity, TDependentEntity>(OwnedNavigationBuilder.Ignore(propertyName));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> Ignore(
            Expression<Func<TDependentEntity, object?>> propertyExpression)
            => Wrap<TEntity, TDependentEntity>(
                OwnedNavigationBuilder.Ignore(propertyExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestIndexBuilder<TDependentEntity> HasIndex(params string[] propertyNames)
            => new NonGenericTestIndexBuilder<TDependentEntity>(OwnedNavigationBuilder.HasIndex(propertyNames));

        public override TestIndexBuilder<TDependentEntity> HasIndex(Expression<Func<TDependentEntity, object?>> indexExpression)
            => new NonGenericTestIndexBuilder<TDependentEntity>(
                OwnedNavigationBuilder.HasIndex(
                    indexExpression.GetMemberAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

        public override TestOwnershipBuilder<TEntity, TDependentEntity> WithOwner(string? ownerReference)
            => new NonGenericTestOwnershipBuilder<TEntity, TDependentEntity>(
                OwnedNavigationBuilder.WithOwner(ownerReference));

        public override TestOwnershipBuilder<TEntity, TDependentEntity> WithOwner(
            Expression<Func<TDependentEntity, TEntity?>>? referenceExpression = null)
            => new NonGenericTestOwnershipBuilder<TEntity, TDependentEntity>(
                OwnedNavigationBuilder.WithOwner(referenceExpression?.GetMemberAccess().GetSimpleMemberName()));

        public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
            Expression<Func<TDependentEntity, TNewDependentEntity?>> navigationExpression)
            where TNewDependentEntity : class
            => Wrap<TDependentEntity, TNewDependentEntity>(
                OwnedNavigationBuilder.OwnsOne(
                    typeof(TNewDependentEntity), navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
            string entityTypeName,
            Expression<Func<TDependentEntity, TNewDependentEntity?>> navigationExpression)
            where TNewDependentEntity : class
            => Wrap<TDependentEntity, TNewDependentEntity>(
                OwnedNavigationBuilder.OwnsOne(
                    entityTypeName, typeof(TNewDependentEntity), navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
            Expression<Func<TDependentEntity, TNewDependentEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
            => Wrap<TEntity, TDependentEntity>(
                OwnedNavigationBuilder.OwnsOne(
                    typeof(TNewDependentEntity),
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericTestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>(r))));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
            string entityTypeName,
            Expression<Func<TDependentEntity, TNewDependentEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
            => Wrap<TEntity, TDependentEntity>(
                OwnedNavigationBuilder.OwnsOne(
                    entityTypeName,
                    typeof(TNewDependentEntity),
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericTestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>(r))));

        public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression)
            where TNewDependentEntity : class
            => Wrap<TDependentEntity, TNewDependentEntity>(
                OwnedNavigationBuilder.OwnsMany(
                    typeof(TNewDependentEntity), navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
            string entityTypeName,
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression)
            where TNewDependentEntity : class
            => Wrap<TDependentEntity, TNewDependentEntity>(
                OwnedNavigationBuilder.OwnsMany(
                    entityTypeName, typeof(TNewDependentEntity), navigationExpression.GetMemberAccess().GetSimpleMemberName()));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            => Wrap<TEntity, TDependentEntity>(
                OwnedNavigationBuilder.OwnsMany(
                    typeof(TNewDependentEntity),
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(Wrap<TDependentEntity, TNewDependentEntity>(r))));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
            string entityTypeName,
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            => Wrap<TEntity, TDependentEntity>(
                OwnedNavigationBuilder.OwnsMany(
                    entityTypeName,
                    typeof(TNewDependentEntity),
                    navigationExpression.GetMemberAccess().GetSimpleMemberName(),
                    r => buildAction(Wrap<TDependentEntity, TNewDependentEntity>(r))));

        public override TestReferenceNavigationBuilder<TDependentEntity, TNewDependentEntity> HasOne<TNewDependentEntity>(
            Expression<Func<TDependentEntity, TNewDependentEntity?>>? navigationExpression = null)
            where TNewDependentEntity : class
            => new NonGenericTestReferenceNavigationBuilder<TDependentEntity, TNewDependentEntity>(
                OwnedNavigationBuilder.HasOne(
                    typeof(TNewDependentEntity), navigationExpression?.GetMemberAccess().GetSimpleMemberName()));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> HasChangeTrackingStrategy(
            ChangeTrackingStrategy changeTrackingStrategy)
            => Wrap<TEntity, TDependentEntity>(OwnedNavigationBuilder.HasChangeTrackingStrategy(changeTrackingStrategy));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> UsePropertyAccessMode(
            PropertyAccessMode propertyAccessMode)
            => Wrap<TEntity, TDependentEntity>(OwnedNavigationBuilder.UsePropertyAccessMode(propertyAccessMode));

        public override DataBuilder<TDependentEntity> HasData(params TDependentEntity[] data)
        {
            OwnedNavigationBuilder.HasData(data);
            return new DataBuilder<TDependentEntity>();
        }

        public override DataBuilder<TDependentEntity> HasData(params object[] data)
        {
            OwnedNavigationBuilder.HasData(data);
            return new DataBuilder<TDependentEntity>();
        }

        public override DataBuilder<TDependentEntity> HasData(IEnumerable<TDependentEntity> data)
        {
            OwnedNavigationBuilder.HasData(data);
            return new DataBuilder<TDependentEntity>();
        }

        public override DataBuilder<TDependentEntity> HasData(IEnumerable<object> data)
        {
            OwnedNavigationBuilder.HasData(data);
            return new DataBuilder<TDependentEntity>();
        }

        OwnedNavigationBuilder IInfrastructure<OwnedNavigationBuilder>.Instance
            => OwnedNavigationBuilder;
    }
}
