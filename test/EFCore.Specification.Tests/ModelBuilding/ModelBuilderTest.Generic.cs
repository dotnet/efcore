// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public abstract partial class ModelBuilderTest
{
    public class GenericTestModelBuilder(ModelBuilderFixtureBase fixture, Action<ModelConfigurationBuilder>? configure) : TestModelBuilder(fixture, configure)
    {
        public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
            => new GenericTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity<TEntity>());

        public override TestEntityTypeBuilder<TEntity> SharedTypeEntity<TEntity>(string name)
            => new GenericTestEntityTypeBuilder<TEntity>(ModelBuilder.SharedTypeEntity<TEntity>(name));

        public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
        {
            ModelBuilder.Entity<TEntity>(
                entityTypeBuilder =>
                    buildAction(new GenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
            return this;
        }

        public override TestModelBuilder SharedTypeEntity<TEntity>(string name, Action<TestEntityTypeBuilder<TEntity>> buildAction)
        {
            ModelBuilder.SharedTypeEntity<TEntity>(
                name,
                entityTypeBuilder =>
                    buildAction(new GenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
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

    protected class GenericTestEntityTypeBuilder<TEntity>(EntityTypeBuilder<TEntity> entityTypeBuilder) : TestEntityTypeBuilder<TEntity>, IInfrastructure<EntityTypeBuilder<TEntity>>
        where TEntity : class
    {
        protected EntityTypeBuilder<TEntity> EntityTypeBuilder { get; } = entityTypeBuilder;

        public override IMutableEntityType Metadata
            => EntityTypeBuilder.Metadata;

        protected virtual TestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder<TEntity> entityTypeBuilder)
            => new GenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder);

        protected virtual TestPropertyBuilder<TProperty> Wrap<TProperty>(PropertyBuilder<TProperty> propertyBuilder)
            => new GenericTestPropertyBuilder<TProperty>(propertyBuilder);

        protected virtual TestPrimitiveCollectionBuilder<TProperty> Wrap<TProperty>(PrimitiveCollectionBuilder<TProperty> propertyBuilder)
            => new GenericTestPrimitiveCollectionBuilder<TProperty>(propertyBuilder);

        public override TestEntityTypeBuilder<TEntity> HasAnnotation(string annotation, object? value)
            => Wrap(EntityTypeBuilder.HasAnnotation(annotation, value));

        public override TestEntityTypeBuilder<TEntity> HasBaseType<TBaseEntity>()
            => Wrap(EntityTypeBuilder.HasBaseType<TBaseEntity>());

        public override TestEntityTypeBuilder<TEntity> HasBaseType(string? baseEntityTypeName)
            => Wrap(EntityTypeBuilder.HasBaseType(baseEntityTypeName));

        public override TestKeyBuilder<TEntity> HasKey(Expression<Func<TEntity, object?>> keyExpression)
            => new GenericTestKeyBuilder<TEntity>((KeyBuilder<TEntity>)EntityTypeBuilder.HasKey(keyExpression));

        public override TestKeyBuilder<TEntity> HasKey(params string[] propertyNames)
            => new GenericTestKeyBuilder<TEntity>(EntityTypeBuilder.HasKey(propertyNames));

        public override TestKeyBuilder<TEntity> HasAlternateKey(Expression<Func<TEntity, object?>> keyExpression)
            => new GenericTestKeyBuilder<TEntity>(EntityTypeBuilder.HasAlternateKey(keyExpression));

        public override TestKeyBuilder<TEntity> HasAlternateKey(params string[] propertyNames)
            => new GenericTestKeyBuilder<TEntity>(EntityTypeBuilder.HasAlternateKey(propertyNames));

        public override TestEntityTypeBuilder<TEntity> HasNoKey()
            => Wrap(EntityTypeBuilder.HasNoKey());

        public override TestPropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            where TProperty : default
            => Wrap(EntityTypeBuilder.Property(propertyExpression));

        public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
            => Wrap(EntityTypeBuilder.Property<TProperty>(propertyName));

        public override TestPrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression)
            where TProperty : default
            => Wrap(EntityTypeBuilder.PrimitiveCollection(propertyExpression));

        public override TestPrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(string propertyName)
            => Wrap(EntityTypeBuilder.PrimitiveCollection<TProperty>(propertyName));

        public override TestPropertyBuilder<TProperty> IndexerProperty<TProperty>(string propertyName)
            => Wrap(EntityTypeBuilder.IndexerProperty<TProperty>(propertyName));

        public override TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(string propertyName)
            => new GenericTestComplexPropertyBuilder<TProperty>(EntityTypeBuilder.ComplexProperty<TProperty>(propertyName));

        public override TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
            Expression<Func<TEntity, TProperty?>> propertyExpression)
            where TProperty : default
            => new GenericTestComplexPropertyBuilder<TProperty>(EntityTypeBuilder.ComplexProperty(propertyExpression));

        public override TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
            Expression<Func<TEntity, TProperty?>> propertyExpression,
            string complexTypeName)
            where TProperty : default
            => new GenericTestComplexPropertyBuilder<TProperty>(
                EntityTypeBuilder.ComplexProperty(propertyExpression, complexTypeName));

        public override TestEntityTypeBuilder<TEntity> ComplexProperty<TProperty>(
            string propertyName,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction)
        {
            buildAction(new GenericTestComplexPropertyBuilder<TProperty>(EntityTypeBuilder.ComplexProperty<TProperty>(propertyName)));

            return this;
        }

        public override TestEntityTypeBuilder<TEntity> ComplexProperty<TProperty>(
            Expression<Func<TEntity, TProperty?>> propertyExpression,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction)
            where TProperty : default
        {
            buildAction(new GenericTestComplexPropertyBuilder<TProperty>(EntityTypeBuilder.ComplexProperty(propertyExpression)));

            return this;
        }

        public override TestEntityTypeBuilder<TEntity> ComplexProperty<TProperty>(
            Expression<Func<TEntity, TProperty?>> propertyExpression,
            string complexTypeName,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction)
            where TProperty : default
        {
            buildAction(
                new GenericTestComplexPropertyBuilder<TProperty>(
                    EntityTypeBuilder.ComplexProperty(propertyExpression, complexTypeName)));

            return this;
        }

        public override TestNavigationBuilder Navigation<TNavigation>(
            Expression<Func<TEntity, TNavigation?>> navigationExpression)
            where TNavigation : class
            => new GenericTestNavigationBuilder<TEntity, TNavigation>(EntityTypeBuilder.Navigation(navigationExpression));

        public override TestNavigationBuilder Navigation<TNavigation>(
            Expression<Func<TEntity, IEnumerable<TNavigation>?>> navigationExpression)
            where TNavigation : class
            => new GenericTestNavigationBuilder<TEntity, TNavigation>(EntityTypeBuilder.Navigation(navigationExpression));

        public override TestEntityTypeBuilder<TEntity> Ignore(Expression<Func<TEntity, object?>> propertyExpression)
            => Wrap(EntityTypeBuilder.Ignore(propertyExpression));

        public override TestEntityTypeBuilder<TEntity> Ignore(string propertyName)
            => Wrap(EntityTypeBuilder.Ignore(propertyName));

        public override TestIndexBuilder<TEntity> HasIndex(Expression<Func<TEntity, object?>> indexExpression)
            => new GenericTestIndexBuilder<TEntity>(EntityTypeBuilder.HasIndex(indexExpression));

        public override TestIndexBuilder<TEntity> HasIndex(Expression<Func<TEntity, object?>> indexExpression, string name)
            => new GenericTestIndexBuilder<TEntity>(EntityTypeBuilder.HasIndex(indexExpression, name));

        public override TestIndexBuilder<TEntity> HasIndex(params string[] propertyNames)
            => new GenericTestIndexBuilder<TEntity>(EntityTypeBuilder.HasIndex(propertyNames));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(string navigationName)
            => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsOne<TRelatedEntity>(navigationName));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
            string entityTypeName,
            string navigationName)
            => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsOne<TRelatedEntity>(entityTypeName, navigationName));

        public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            string navigationName,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            => Wrap(
                EntityTypeBuilder.OwnsOne<TRelatedEntity>(
                    navigationName,
                    r => buildAction(new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            string entityTypeName,
            string navigationName,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            => Wrap(
                EntityTypeBuilder.OwnsOne<TRelatedEntity>(
                    entityTypeName, navigationName,
                    r => buildAction(new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression)
            where TRelatedEntity : class
            => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.OwnsOne(navigationExpression));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
            string entityTypeName,
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression)
            where TRelatedEntity : class
            => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsOne(
                    entityTypeName, navigationExpression));

        public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
            => Wrap(
                EntityTypeBuilder.OwnsOne(
                    navigationExpression,
                    r => buildAction(new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            string entityTypeName,
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
            => Wrap(
                EntityTypeBuilder.OwnsOne(
                    entityTypeName, navigationExpression,
                    r => buildAction(new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(string navigationName)
            => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsMany<TRelatedEntity>(navigationName));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
            string entityTypeName,
            string navigationName)
            => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsMany<TRelatedEntity>(entityTypeName, navigationName));

        public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            string navigationName,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            => Wrap(
                EntityTypeBuilder.OwnsMany<TRelatedEntity>(
                    navigationName,
                    r => buildAction(new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            string entityTypeName,
            string navigationName,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            => Wrap(
                EntityTypeBuilder.OwnsMany<TRelatedEntity>(
                    entityTypeName, navigationName,
                    r => buildAction(new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression)
            where TRelatedEntity : class
            => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsMany(navigationExpression));

        public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
            string entityTypeName,
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression)
            where TRelatedEntity : class
            => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.OwnsMany(entityTypeName, navigationExpression));

        public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
            => Wrap(
                EntityTypeBuilder.OwnsMany(
                    navigationExpression,
                    r => buildAction(new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            string entityTypeName,
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class
            => Wrap(
                EntityTypeBuilder.OwnsMany(
                    entityTypeName, navigationExpression,
                    r => buildAction(new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

        public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(string? navigationName)
            => new GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.HasOne<TRelatedEntity>(navigationName));

        public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>>? navigationExpression = null)
            where TRelatedEntity : class
            => new GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasOne(navigationExpression));

        public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(string? navigationName)
            => new GenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(
                EntityTypeBuilder.HasMany<TRelatedEntity>(navigationName));

        public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>>? navigationExpression = null)
            where TRelatedEntity : class
            => new GenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasMany(navigationExpression));

        public override TestEntityTypeBuilder<TEntity> HasQueryFilter(Expression<Func<TEntity, bool>> filter)
            => Wrap(EntityTypeBuilder.HasQueryFilter(filter));

        public override TestEntityTypeBuilder<TEntity> HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
            => Wrap(EntityTypeBuilder.HasChangeTrackingStrategy(changeTrackingStrategy));

        public override TestEntityTypeBuilder<TEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => Wrap(EntityTypeBuilder.UsePropertyAccessMode(propertyAccessMode));

        public override DataBuilder<TEntity> HasData(params TEntity[] data)
            => EntityTypeBuilder.HasData(data);

        public override DataBuilder<TEntity> HasData(params object[] data)
            => EntityTypeBuilder.HasData(data);

        public override DataBuilder<TEntity> HasData(IEnumerable<TEntity> data)
            => EntityTypeBuilder.HasData(data);

        public override DataBuilder<TEntity> HasData(IEnumerable<object> data)
            => EntityTypeBuilder.HasData(data);

        public override TestDiscriminatorBuilder<TDiscriminator> HasDiscriminator<TDiscriminator>(
            Expression<Func<TEntity, TDiscriminator>> propertyExpression)
            where TDiscriminator : default
            => new GenericTestDiscriminatorBuilder<TDiscriminator>(EntityTypeBuilder.HasDiscriminator(propertyExpression));

        public override TestDiscriminatorBuilder<TDiscriminator> HasDiscriminator<TDiscriminator>(string propertyName)
            => new GenericTestDiscriminatorBuilder<TDiscriminator>(EntityTypeBuilder.HasDiscriminator<TDiscriminator>(propertyName));

        public override TestEntityTypeBuilder<TEntity> HasNoDiscriminator()
            => Wrap(EntityTypeBuilder.HasNoDiscriminator());

        public EntityTypeBuilder<TEntity> Instance
            => EntityTypeBuilder;
    }

    protected class GenericTestComplexPropertyBuilder<TComplex>(ComplexPropertyBuilder<TComplex> complexPropertyBuilder) :
        TestComplexPropertyBuilder<TComplex>,
        IInfrastructure<ComplexPropertyBuilder<TComplex>>
    {
        protected ComplexPropertyBuilder<TComplex> PropertyBuilder { get; } = complexPropertyBuilder;

        public override IMutableComplexProperty Metadata
            => PropertyBuilder.Metadata;

        protected virtual TestComplexPropertyBuilder<T> Wrap<T>(ComplexPropertyBuilder<T> complexPropertyBuilder)
            => new GenericTestComplexPropertyBuilder<T>(complexPropertyBuilder);

        protected virtual TestComplexTypePropertyBuilder<TProperty> Wrap<TProperty>(ComplexTypePropertyBuilder<TProperty> propertyBuilder)
            => new GenericTestComplexTypePropertyBuilder<TProperty>(propertyBuilder);

        protected virtual TestComplexTypePrimitiveCollectionBuilder<TProperty> Wrap<TProperty>(
            ComplexTypePrimitiveCollectionBuilder<TProperty> propertyBuilder)
            => new GenericTestComplexTypePrimitiveCollectionBuilder<TProperty>(propertyBuilder);

        public override TestComplexPropertyBuilder<TComplex> HasPropertyAnnotation(string annotation, object? value)
            => Wrap(PropertyBuilder.HasPropertyAnnotation(annotation, value));

        public override TestComplexPropertyBuilder<TComplex> HasTypeAnnotation(string annotation, object? value)
            => Wrap(PropertyBuilder.HasTypeAnnotation(annotation, value));

        public override TestComplexTypePropertyBuilder<TProperty> Property<TProperty>(
            Expression<Func<TComplex, TProperty>> propertyExpression)
            where TProperty : default
            => Wrap(PropertyBuilder.Property(propertyExpression));

        public override TestComplexTypePropertyBuilder<TProperty> Property<TProperty>(string propertyName)
            => Wrap(PropertyBuilder.Property<TProperty>(propertyName));

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(
            Expression<Func<TComplex, TProperty>> propertyExpression)
            where TProperty : default
            => Wrap(PropertyBuilder.PrimitiveCollection(propertyExpression));

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(string propertyName)
            => Wrap(PropertyBuilder.PrimitiveCollection<TProperty>(propertyName));

        public override TestComplexTypePropertyBuilder<TProperty> IndexerProperty<TProperty>(string propertyName)
            => Wrap(PropertyBuilder.IndexerProperty<TProperty>(propertyName));

        public override TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(string propertyName)
            => Wrap(PropertyBuilder.ComplexProperty<TProperty>(propertyName));

        public override TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
            Expression<Func<TComplex, TProperty?>> propertyExpression)
            where TProperty : default
            => Wrap(PropertyBuilder.ComplexProperty(propertyExpression));

        public override TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
            Expression<Func<TComplex, TProperty?>> propertyExpression,
            string complexTypeName)
            where TProperty : default
            => Wrap(PropertyBuilder.ComplexProperty(propertyExpression, complexTypeName));

        public override TestComplexPropertyBuilder<TComplex> ComplexProperty<TProperty>(
            string propertyName,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction)
        {
            buildAction(Wrap(PropertyBuilder.ComplexProperty<TProperty>(propertyName)));

            return this;
        }

        public override TestComplexPropertyBuilder<TComplex> ComplexProperty<TProperty>(
            Expression<Func<TComplex, TProperty?>> propertyExpression,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction)
            where TProperty : default
        {
            buildAction(Wrap(PropertyBuilder.ComplexProperty(propertyExpression)));

            return this;
        }

        public override TestComplexPropertyBuilder<TComplex> ComplexProperty<TProperty>(
            Expression<Func<TComplex, TProperty?>> propertyExpression,
            string complexTypeName,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction)
            where TProperty : default
        {
            buildAction(Wrap(PropertyBuilder.ComplexProperty(propertyExpression, complexTypeName)));

            return this;
        }

        public override TestComplexPropertyBuilder<TComplex> Ignore(Expression<Func<TComplex, object?>> propertyExpression)
            => Wrap(PropertyBuilder.Ignore(propertyExpression));

        public override TestComplexPropertyBuilder<TComplex> Ignore(string propertyName)
            => Wrap(PropertyBuilder.Ignore(propertyName));

        public override TestComplexPropertyBuilder<TComplex> IsRequired(bool isRequired = true)
            => Wrap(PropertyBuilder.IsRequired(isRequired));

        public override TestComplexPropertyBuilder<TComplex> HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
            => Wrap(PropertyBuilder.HasChangeTrackingStrategy(changeTrackingStrategy));

        public override TestComplexPropertyBuilder<TComplex> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => Wrap(PropertyBuilder.UsePropertyAccessMode(propertyAccessMode));

        public override TestComplexPropertyBuilder<TComplex> UseDefaultPropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => Wrap(PropertyBuilder.UseDefaultPropertyAccessMode(propertyAccessMode));

        public ComplexPropertyBuilder<TComplex> Instance
            => PropertyBuilder;
    }

    protected class GenericTestDiscriminatorBuilder<TDiscriminator>(DiscriminatorBuilder<TDiscriminator> discriminatorBuilder) : TestDiscriminatorBuilder<TDiscriminator>
    {
        protected DiscriminatorBuilder<TDiscriminator> DiscriminatorBuilder { get; } = discriminatorBuilder;

        protected virtual TestDiscriminatorBuilder<TDiscriminator> Wrap(DiscriminatorBuilder<TDiscriminator> discriminatorBuilder)
            => new GenericTestDiscriminatorBuilder<TDiscriminator>(discriminatorBuilder);

        public override TestDiscriminatorBuilder<TDiscriminator> IsComplete(bool complete)
            => Wrap(DiscriminatorBuilder.IsComplete(complete));

        public override TestDiscriminatorBuilder<TDiscriminator> HasValue(TDiscriminator value)
            => Wrap(DiscriminatorBuilder.HasValue(value));

        public override TestDiscriminatorBuilder<TDiscriminator> HasValue<TEntity>(TDiscriminator value)
            => Wrap(DiscriminatorBuilder.HasValue<TEntity>(value));

        public override TestDiscriminatorBuilder<TDiscriminator> HasValue(Type entityType, TDiscriminator value)
            => Wrap(DiscriminatorBuilder.HasValue(entityType, value));

        public override TestDiscriminatorBuilder<TDiscriminator> HasValue(string entityTypeName, TDiscriminator value)
            => Wrap(DiscriminatorBuilder.HasValue(entityTypeName, value));
    }

    protected class GenericTestOwnedEntityTypeBuilder<TEntity>(OwnedEntityTypeBuilder<TEntity> ownedEntityTypeBuilder) : TestOwnedEntityTypeBuilder<TEntity>,
        IInfrastructure<OwnedEntityTypeBuilder<TEntity>>
        where TEntity : class
    {
        protected OwnedEntityTypeBuilder<TEntity> OwnedEntityTypeBuilder { get; } = ownedEntityTypeBuilder;

        public OwnedEntityTypeBuilder<TEntity> Instance
            => OwnedEntityTypeBuilder;
    }

    protected class GenericTestPropertyBuilder<TProperty>(PropertyBuilder<TProperty> propertyBuilder) : TestPropertyBuilder<TProperty>, IInfrastructure<PropertyBuilder<TProperty>>
    {
        protected PropertyBuilder<TProperty> PropertyBuilder { get; } = propertyBuilder;

        public override IMutableProperty Metadata
            => PropertyBuilder.Metadata;

        protected virtual TestPropertyBuilder<TProperty> Wrap(PropertyBuilder<TProperty> propertyBuilder)
            => new GenericTestPropertyBuilder<TProperty>(propertyBuilder);

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
            => Wrap(PropertyBuilder.HasConversion<TConversion>());

        public override TestPropertyBuilder<TProperty> HasConversion<TConversion>(ValueComparer? valueComparer)
            => Wrap(PropertyBuilder.HasConversion<TConversion>(valueComparer));

        public override TestPropertyBuilder<TProperty> HasConversion<TConversion>(
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType)
            => Wrap(PropertyBuilder.HasConversion<TConversion>(valueComparer, providerComparerType));

        public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression)
            => Wrap(
                PropertyBuilder.HasConversion(
                    convertToProviderExpression,
                    convertFromProviderExpression));

        public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            ValueComparer? valueComparer)
            => Wrap(
                PropertyBuilder.HasConversion(
                    convertToProviderExpression,
                    convertFromProviderExpression,
                    valueComparer));

        public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType)
            => Wrap(
                PropertyBuilder.HasConversion(
                    convertToProviderExpression,
                    convertFromProviderExpression,
                    valueComparer,
                    providerComparerType));

        public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(ValueConverter<TProperty, TProvider> converter)
            => Wrap(PropertyBuilder.HasConversion(converter));

        public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            ValueConverter<TProperty, TProvider> converter,
            ValueComparer? valueComparer)
            => Wrap(PropertyBuilder.HasConversion(converter, valueComparer));

        public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            ValueConverter<TProperty, TProvider> converter,
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

        PropertyBuilder<TProperty> IInfrastructure<PropertyBuilder<TProperty>>.Instance
            => PropertyBuilder;
    }

    protected class GenericTestPrimitiveCollectionBuilder<TProperty>(PrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder)
        : TestPrimitiveCollectionBuilder<TProperty>, IInfrastructure<PrimitiveCollectionBuilder<TProperty>>
    {
        protected PrimitiveCollectionBuilder<TProperty> PrimitiveCollectionBuilder { get; } = primitiveCollectionBuilder;

        public override IMutableProperty Metadata
            => PrimitiveCollectionBuilder.Metadata;

        public override TestElementTypeBuilder ElementType()
            => new(PrimitiveCollectionBuilder.ElementType());

        public override TestPrimitiveCollectionBuilder<TProperty> ElementType(Action<TestElementTypeBuilder> builderAction)
            => Wrap(PrimitiveCollectionBuilder.ElementType(b => builderAction(new TestElementTypeBuilder(b))));

        protected virtual TestPrimitiveCollectionBuilder<TProperty> Wrap(PrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder)
            => new GenericTestPrimitiveCollectionBuilder<TProperty>(primitiveCollectionBuilder);

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

        PrimitiveCollectionBuilder<TProperty> IInfrastructure<PrimitiveCollectionBuilder<TProperty>>.Instance
            => PrimitiveCollectionBuilder;
    }

    protected class GenericTestComplexTypePropertyBuilder<TProperty>(ComplexTypePropertyBuilder<TProperty> propertyBuilder) :
        TestComplexTypePropertyBuilder<TProperty>,
        IInfrastructure<ComplexTypePropertyBuilder<TProperty>>
    {
        protected ComplexTypePropertyBuilder<TProperty> PropertyBuilder { get; } = propertyBuilder;

        public override IMutableProperty Metadata
            => PropertyBuilder.Metadata;

        protected virtual TestComplexTypePropertyBuilder<TProperty> Wrap(ComplexTypePropertyBuilder<TProperty> propertyBuilder)
            => new GenericTestComplexTypePropertyBuilder<TProperty>(propertyBuilder);

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
            => Wrap(PropertyBuilder.HasConversion<TConversion>());

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TConversion>(ValueComparer? valueComparer)
            => Wrap(PropertyBuilder.HasConversion<TConversion>(valueComparer));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TConversion>(
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType)
            => Wrap(PropertyBuilder.HasConversion<TConversion>(valueComparer, providerComparerType));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression)
            => Wrap(
                PropertyBuilder.HasConversion(
                    convertToProviderExpression,
                    convertFromProviderExpression));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            ValueComparer? valueComparer)
            => Wrap(
                PropertyBuilder.HasConversion(
                    convertToProviderExpression,
                    convertFromProviderExpression,
                    valueComparer));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType)
            => Wrap(
                PropertyBuilder.HasConversion(
                    convertToProviderExpression,
                    convertFromProviderExpression,
                    valueComparer,
                    providerComparerType));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(ValueConverter<TProperty, TProvider> converter)
            => Wrap(PropertyBuilder.HasConversion(converter));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(
            ValueConverter<TProperty, TProvider> converter,
            ValueComparer? valueComparer)
            => Wrap(PropertyBuilder.HasConversion(converter, valueComparer));

        public override TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(
            ValueConverter<TProperty, TProvider> converter,
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

        ComplexTypePropertyBuilder<TProperty> IInfrastructure<ComplexTypePropertyBuilder<TProperty>>.Instance
            => PropertyBuilder;
    }

    protected class GenericTestComplexTypePrimitiveCollectionBuilder<TProperty>(ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder) :
        TestComplexTypePrimitiveCollectionBuilder<TProperty>,
        IInfrastructure<ComplexTypePrimitiveCollectionBuilder<TProperty>>
    {
        protected ComplexTypePrimitiveCollectionBuilder<TProperty> PrimitiveCollectionBuilder { get; } = primitiveCollectionBuilder;

        public override IMutableProperty Metadata
            => PrimitiveCollectionBuilder.Metadata;

        public override TestElementTypeBuilder ElementType()
            => new(PrimitiveCollectionBuilder.ElementType());

        public override TestComplexTypePrimitiveCollectionBuilder<TProperty> ElementType(Action<TestElementTypeBuilder> builderAction)
            => Wrap(PrimitiveCollectionBuilder.ElementType(b => builderAction(new TestElementTypeBuilder(b))));

        protected virtual TestComplexTypePrimitiveCollectionBuilder<TProperty> Wrap(
            ComplexTypePrimitiveCollectionBuilder<TProperty> primitiveCollectionBuilder)
            => new GenericTestComplexTypePrimitiveCollectionBuilder<TProperty>(primitiveCollectionBuilder);

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

        ComplexTypePrimitiveCollectionBuilder<TProperty> IInfrastructure<ComplexTypePrimitiveCollectionBuilder<TProperty>>.Instance
            => PrimitiveCollectionBuilder;
    }

    protected class GenericTestKeyBuilder<TEntity>(KeyBuilder<TEntity> keyBuilder) : TestKeyBuilder<TEntity>, IInfrastructure<KeyBuilder<TEntity>>
    {
        private KeyBuilder<TEntity> KeyBuilder { get; } = keyBuilder;

        public override IMutableKey Metadata
            => KeyBuilder.Metadata;

        public override TestKeyBuilder<TEntity> HasAnnotation(string annotation, object? value)
            => new GenericTestKeyBuilder<TEntity>(KeyBuilder.HasAnnotation(annotation, value));

        KeyBuilder<TEntity> IInfrastructure<KeyBuilder<TEntity>>.Instance
            => KeyBuilder;
    }

    protected class GenericTestIndexBuilder<TEntity>(IndexBuilder<TEntity> indexBuilder) : TestIndexBuilder<TEntity>, IInfrastructure<IndexBuilder<TEntity>>
    {
        private IndexBuilder<TEntity> IndexBuilder { get; } = indexBuilder;

        public override IMutableIndex Metadata
            => IndexBuilder.Metadata;

        public override TestIndexBuilder<TEntity> HasAnnotation(string annotation, object? value)
            => new GenericTestIndexBuilder<TEntity>(IndexBuilder.HasAnnotation(annotation, value));

        public override TestIndexBuilder<TEntity> IsUnique(bool isUnique = true)
            => new GenericTestIndexBuilder<TEntity>(IndexBuilder.IsUnique(isUnique));

        public override TestIndexBuilder<TEntity> IsDescending(params bool[] isDescending)
            => new GenericTestIndexBuilder<TEntity>(IndexBuilder.IsDescending(isDescending));

        IndexBuilder<TEntity> IInfrastructure<IndexBuilder<TEntity>>.Instance
            => IndexBuilder;
    }

    protected class GenericTestNavigationBuilder<TSource, TTarget>(NavigationBuilder<TSource, TTarget> navigationBuilder) : TestNavigationBuilder
        where TSource : class
        where TTarget : class
    {
        private NavigationBuilder<TSource, TTarget> NavigationBuilder { get; } = navigationBuilder;

        public override TestNavigationBuilder HasAnnotation(string annotation, object? value)
            => new GenericTestNavigationBuilder<TSource, TTarget>(NavigationBuilder.HasAnnotation(annotation, value));

        public override TestNavigationBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            => new GenericTestNavigationBuilder<TSource, TTarget>(NavigationBuilder.UsePropertyAccessMode(propertyAccessMode));

        public override TestNavigationBuilder HasField(string fieldName)
            => new GenericTestNavigationBuilder<TSource, TTarget>(NavigationBuilder.HasField(fieldName));

        public override TestNavigationBuilder AutoInclude(bool autoInclude = true)
            => new GenericTestNavigationBuilder<TSource, TTarget>(NavigationBuilder.AutoInclude(autoInclude));

        public override TestNavigationBuilder EnableLazyLoading(bool lazyLoadingEnabled = true)
            => new GenericTestNavigationBuilder<TSource, TTarget>(NavigationBuilder.EnableLazyLoading(lazyLoadingEnabled));

        public override TestNavigationBuilder IsRequired(bool required = true)
            => new GenericTestNavigationBuilder<TSource, TTarget>(NavigationBuilder.IsRequired(required));
    }

    protected class
        GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(ReferenceNavigationBuilder<TEntity, TRelatedEntity> referenceNavigationBuilder) : TestReferenceNavigationBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        protected ReferenceNavigationBuilder<TEntity, TRelatedEntity> ReferenceNavigationBuilder { get; } = referenceNavigationBuilder;

        public override TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(string? navigationName)
            => new GenericTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(
                ReferenceNavigationBuilder.WithMany(navigationName));

        public override TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(
            Expression<Func<TRelatedEntity, IEnumerable<TEntity>?>>? navigationExpression = null)
            => new GenericTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(
                ReferenceNavigationBuilder.WithMany(navigationExpression));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(string? navigationName)
            => new GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                ReferenceNavigationBuilder.WithOne(navigationName));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
            Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression = null)
            => new GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                ReferenceNavigationBuilder.WithOne(navigationExpression));
    }

    protected class GenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(CollectionNavigationBuilder<TEntity, TRelatedEntity> collectionNavigationBuilder) :
        TestCollectionNavigationBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        protected CollectionNavigationBuilder<TEntity, TRelatedEntity> CollectionNavigationBuilder { get; } = collectionNavigationBuilder;

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(string? navigationName)
            => new GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                CollectionNavigationBuilder.WithOne(navigationName));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(
            Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression = null)
            => new GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                CollectionNavigationBuilder.WithOne(navigationExpression));

        public override TestCollectionCollectionBuilder<TRelatedEntity, TEntity> WithMany(
            string? navigationName = null)
            => new GenericTestCollectionCollectionBuilder<TRelatedEntity, TEntity>(
                CollectionNavigationBuilder.WithMany(navigationName));

        public override TestCollectionCollectionBuilder<TRelatedEntity, TEntity> WithMany(
            Expression<Func<TRelatedEntity, IEnumerable<TEntity>?>> navigationExpression)
            => new GenericTestCollectionCollectionBuilder<TRelatedEntity, TEntity>(
                CollectionNavigationBuilder.WithMany(navigationExpression));
    }

    protected class GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder)
        : TestReferenceCollectionBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        public ReferenceCollectionBuilder<TEntity, TRelatedEntity> ReferenceCollectionBuilder { get; } = referenceCollectionBuilder;

        public override IMutableForeignKey Metadata
            => ReferenceCollectionBuilder.Metadata;

        protected virtual GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity> Wrap(
            ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder)
            => new(referenceCollectionBuilder);

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(
            Expression<Func<TRelatedEntity, object?>> foreignKeyExpression)
            => Wrap(ReferenceCollectionBuilder.HasForeignKey(foreignKeyExpression));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
            Expression<Func<TEntity, object?>> keyExpression)
            => Wrap(ReferenceCollectionBuilder.HasPrincipalKey(keyExpression));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(params string[] foreignKeyPropertyNames)
            => Wrap(ReferenceCollectionBuilder.HasForeignKey(foreignKeyPropertyNames));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(params string[] keyPropertyNames)
            => Wrap(ReferenceCollectionBuilder.HasPrincipalKey(keyPropertyNames));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasAnnotation(string annotation, object? value)
            => Wrap(ReferenceCollectionBuilder.HasAnnotation(annotation, value));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> IsRequired(bool isRequired = true)
            => Wrap(ReferenceCollectionBuilder.IsRequired(isRequired));

        public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior)
            => Wrap(ReferenceCollectionBuilder.OnDelete(deleteBehavior));
    }

    protected class GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder) :
        TestReferenceReferenceBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        protected ReferenceReferenceBuilder<TEntity, TRelatedEntity> ReferenceReferenceBuilder { get; } = referenceReferenceBuilder;

        public override IMutableForeignKey Metadata
            => ReferenceReferenceBuilder.Metadata;

        protected virtual GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(
            ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder)
            => new(referenceReferenceBuilder);

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasAnnotation(string annotation, object? value)
            => Wrap(ReferenceReferenceBuilder.HasAnnotation(annotation, value));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
            Expression<Func<TDependentEntity, object?>> foreignKeyExpression)
            => Wrap(ReferenceReferenceBuilder.HasForeignKey(foreignKeyExpression));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
            Expression<Func<TPrincipalEntity, object?>> keyExpression)
            => Wrap(ReferenceReferenceBuilder.HasPrincipalKey(keyExpression));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
            params string[] foreignKeyPropertyNames)
            => Wrap(ReferenceReferenceBuilder.HasForeignKey<TDependentEntity>(foreignKeyPropertyNames));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
            params string[] keyPropertyNames)
            => Wrap(ReferenceReferenceBuilder.HasPrincipalKey<TPrincipalEntity>(keyPropertyNames));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> IsRequired(bool isRequired = true)
            => Wrap(ReferenceReferenceBuilder.IsRequired(isRequired));

        public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior)
            => Wrap(ReferenceReferenceBuilder.OnDelete(deleteBehavior));
    }

    protected class GenericTestCollectionCollectionBuilder<TLeftEntity, TRightEntity>(
        CollectionCollectionBuilder<TLeftEntity, TRightEntity> collectionCollectionBuilder) :
        TestCollectionCollectionBuilder<TLeftEntity, TRightEntity>
        where TLeftEntity : class
        where TRightEntity : class
    {
        protected CollectionCollectionBuilder<TLeftEntity, TRightEntity> CollectionCollectionBuilder { get; } = collectionCollectionBuilder;

        public override TestEntityTypeBuilder<Dictionary<string, object>> UsingEntity(string joinEntityName)
            => new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                new EntityTypeBuilder<Dictionary<string, object>>(
                    CollectionCollectionBuilder.UsingEntity(joinEntityName)
                        .Metadata));

        public override TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>()
            => new GenericTestEntityTypeBuilder<TJoinEntity>(
                CollectionCollectionBuilder.UsingEntity<TJoinEntity>());

        public override TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            string joinEntityName)
            => new GenericTestEntityTypeBuilder<TJoinEntity>(
                CollectionCollectionBuilder.UsingEntity<TJoinEntity>(joinEntityName));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity(
            Action<TestEntityTypeBuilder<Dictionary<string, object>>> configureJoinEntityType)
            => new GenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    e => configureJoinEntityType(
                        new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                            new EntityTypeBuilder<Dictionary<string, object>>(e.Metadata)))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity(
            string joinEntityName,
            Action<TestEntityTypeBuilder<Dictionary<string, object>>> configureJoinEntityType)
            => new GenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    joinEntityName,
                    e => configureJoinEntityType(
                        new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                            new EntityTypeBuilder<Dictionary<string, object>>(e.Metadata)))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            => new GenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity<TJoinEntity>(
                    e => configureJoinEntityType(new GenericTestEntityTypeBuilder<TJoinEntity>(e))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            string joinEntityName,
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            => new GenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity<TJoinEntity>(
                    joinEntityName,
                    e => configureJoinEntityType(new GenericTestEntityTypeBuilder<TJoinEntity>(e))));

        public override TestEntityTypeBuilder<Dictionary<string, object>> UsingEntity(
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>> configureRight,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>> configureLeft)
            => new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                new EntityTypeBuilder<Dictionary<string, object>>(
                    CollectionCollectionBuilder.UsingEntity(
                            l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>)configureRight(
                                new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                                    new EntityTypeBuilder<Dictionary<string, object>>(l.Metadata)))).ReferenceCollectionBuilder,
                            r => ((GenericTestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>)configureLeft(
                                new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                                    new EntityTypeBuilder<Dictionary<string, object>>(r.Metadata)))).ReferenceCollectionBuilder)
                        .Metadata));

        public override TestEntityTypeBuilder<Dictionary<string, object>> UsingEntity(
            string joinEntityName,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>> configureRight,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>> configureLeft)
            => new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                new EntityTypeBuilder<Dictionary<string, object>>(
                    CollectionCollectionBuilder.UsingEntity(
                            joinEntityName,
                            l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>)configureRight(
                                new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                                    new EntityTypeBuilder<Dictionary<string, object>>(l.Metadata)))).ReferenceCollectionBuilder,
                            r => ((GenericTestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>)configureLeft(
                                new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                                    new EntityTypeBuilder<Dictionary<string, object>>(r.Metadata)))).ReferenceCollectionBuilder)
                        .Metadata));

        public override TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft)
            => new GenericTestEntityTypeBuilder<TJoinEntity>(
                CollectionCollectionBuilder.UsingEntity<TJoinEntity>(
                    l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                        new GenericTestEntityTypeBuilder<TJoinEntity>(l))).ReferenceCollectionBuilder,
                    r => ((GenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                        new GenericTestEntityTypeBuilder<TJoinEntity>(r))).ReferenceCollectionBuilder));

        public override TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            string joinEntityName,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft)
            => new GenericTestEntityTypeBuilder<TJoinEntity>(
                CollectionCollectionBuilder.UsingEntity<TJoinEntity>(
                    joinEntityName,
                    l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                        new GenericTestEntityTypeBuilder<TJoinEntity>(l))).ReferenceCollectionBuilder,
                    r => ((GenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                        new GenericTestEntityTypeBuilder<TJoinEntity>(r))).ReferenceCollectionBuilder));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity(
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>> configureRight,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>> configureLeft,
            Action<TestEntityTypeBuilder<Dictionary<string, object>>> configureJoinEntityType)
            => new GenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>)configureRight(
                        new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                            new EntityTypeBuilder<Dictionary<string, object>>(l.Metadata)))).ReferenceCollectionBuilder,
                    r => ((GenericTestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>)configureLeft(
                        new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                            new EntityTypeBuilder<Dictionary<string, object>>(r.Metadata)))).ReferenceCollectionBuilder,
                    e => configureJoinEntityType(
                        new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                            new EntityTypeBuilder<Dictionary<string, object>>(e.Metadata)))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity(
            string joinEntityName,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>> configureRight,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>> configureLeft,
            Action<TestEntityTypeBuilder<Dictionary<string, object>>> configureJoinEntityType)
            => new GenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity(
                    joinEntityName,
                    l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>)configureRight(
                        new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                            new EntityTypeBuilder<Dictionary<string, object>>(l.Metadata)))).ReferenceCollectionBuilder,
                    r => ((GenericTestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>)configureLeft(
                        new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                            new EntityTypeBuilder<Dictionary<string, object>>(r.Metadata)))).ReferenceCollectionBuilder,
                    e => configureJoinEntityType(
                        new GenericTestEntityTypeBuilder<Dictionary<string, object>>(
                            new EntityTypeBuilder<Dictionary<string, object>>(e.Metadata)))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            where TJoinEntity : class
            => new GenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity<TJoinEntity>(
                    l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                        new GenericTestEntityTypeBuilder<TJoinEntity>(l))).ReferenceCollectionBuilder,
                    r => ((GenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                        new GenericTestEntityTypeBuilder<TJoinEntity>(r))).ReferenceCollectionBuilder,
                    e => configureJoinEntityType(new GenericTestEntityTypeBuilder<TJoinEntity>(e))));

        public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            string joinEntityName,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            where TJoinEntity : class
            => new GenericTestEntityTypeBuilder<TRightEntity>(
                CollectionCollectionBuilder.UsingEntity<TJoinEntity>(
                    joinEntityName,
                    l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                        new GenericTestEntityTypeBuilder<TJoinEntity>(l))).ReferenceCollectionBuilder,
                    r => ((GenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                        new GenericTestEntityTypeBuilder<TJoinEntity>(r))).ReferenceCollectionBuilder,
                    e => configureJoinEntityType(new GenericTestEntityTypeBuilder<TJoinEntity>(e))));
    }

    protected class GenericTestOwnershipBuilder<TEntity, TDependentEntity>(OwnershipBuilder<TEntity, TDependentEntity> ownershipBuilder)
        : TestOwnershipBuilder<TEntity, TDependentEntity>, IInfrastructure<OwnershipBuilder<TEntity, TDependentEntity>>
        where TEntity : class
        where TDependentEntity : class
    {
        protected OwnershipBuilder<TEntity, TDependentEntity> OwnershipBuilder { get; } = ownershipBuilder;

        public override IMutableForeignKey Metadata
            => OwnershipBuilder.Metadata;

        protected virtual GenericTestOwnershipBuilder<TNewEntity, TNewDependentEntity> Wrap<TNewEntity, TNewDependentEntity>(
            OwnershipBuilder<TNewEntity, TNewDependentEntity> ownershipBuilder)
            where TNewEntity : class
            where TNewDependentEntity : class
            => new(ownershipBuilder);

        public override TestOwnershipBuilder<TEntity, TDependentEntity> HasAnnotation(string annotation, object? value)
            => Wrap(OwnershipBuilder.HasAnnotation(annotation, value));

        public override TestOwnershipBuilder<TEntity, TDependentEntity> HasForeignKey(
            params string[] foreignKeyPropertyNames)
            => Wrap(OwnershipBuilder.HasForeignKey(foreignKeyPropertyNames));

        public override TestOwnershipBuilder<TEntity, TDependentEntity> HasForeignKey(
            Expression<Func<TDependentEntity, object?>> foreignKeyExpression)
            => Wrap(OwnershipBuilder.HasForeignKey(foreignKeyExpression));

        public override TestOwnershipBuilder<TEntity, TDependentEntity> HasPrincipalKey(
            params string[] keyPropertyNames)
            => Wrap(OwnershipBuilder.HasPrincipalKey(keyPropertyNames));

        public override TestOwnershipBuilder<TEntity, TDependentEntity> HasPrincipalKey(
            Expression<Func<TEntity, object?>> keyExpression)
            => Wrap(OwnershipBuilder.HasPrincipalKey(keyExpression));

        OwnershipBuilder<TEntity, TDependentEntity> IInfrastructure<OwnershipBuilder<TEntity, TDependentEntity>>.Instance
            => OwnershipBuilder;
    }

    protected class GenericTestOwnedNavigationBuilder<TEntity, TDependentEntity>(OwnedNavigationBuilder<TEntity, TDependentEntity> ownedNavigationBuilder)
        : TestOwnedNavigationBuilder<TEntity, TDependentEntity>,
            IInfrastructure<OwnedNavigationBuilder<TEntity, TDependentEntity>>
        where TEntity : class
        where TDependentEntity : class
    {
        protected OwnedNavigationBuilder<TEntity, TDependentEntity> OwnedNavigationBuilder { get; } = ownedNavigationBuilder;

        public override IMutableForeignKey Metadata
            => OwnedNavigationBuilder.Metadata;

        public override IMutableEntityType OwnedEntityType
            => OwnedNavigationBuilder.OwnedEntityType;

        protected virtual GenericTestOwnedNavigationBuilder<TNewEntity, TNewDependentEntity> Wrap<TNewEntity, TNewDependentEntity>(
            OwnedNavigationBuilder<TNewEntity, TNewDependentEntity> ownershipBuilder)
            where TNewEntity : class
            where TNewDependentEntity : class
            => new(ownershipBuilder);

        protected virtual TestPropertyBuilder<TProperty> Wrap<TProperty>(PropertyBuilder<TProperty> propertyBuilder)
            => new GenericTestPropertyBuilder<TProperty>(propertyBuilder);

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> HasAnnotation(
            string annotation,
            object? value)
            => Wrap(OwnedNavigationBuilder.HasAnnotation(annotation, value));

        public override TestKeyBuilder<TDependentEntity> HasKey(Expression<Func<TDependentEntity, object?>> keyExpression)
            => new GenericTestKeyBuilder<TDependentEntity>(OwnedNavigationBuilder.HasKey(keyExpression));

        public override TestKeyBuilder<TDependentEntity> HasKey(params string[] propertyNames)
            => new GenericTestKeyBuilder<TDependentEntity>(OwnedNavigationBuilder.HasKey(propertyNames));

        public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
            => Wrap(OwnedNavigationBuilder.Property<TProperty>(propertyName));

        public override TestPropertyBuilder<TProperty> IndexerProperty<TProperty>(string propertyName)
            => Wrap(OwnedNavigationBuilder.IndexerProperty<TProperty>(propertyName));

        public override TestPropertyBuilder<TProperty> Property<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => Wrap(OwnedNavigationBuilder.Property(propertyExpression));

        public override TestPrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(string propertyName)
            => new GenericTestPrimitiveCollectionBuilder<TProperty>(OwnedNavigationBuilder.PrimitiveCollection<TProperty>(propertyName));

        public override TestPrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            => new GenericTestPrimitiveCollectionBuilder<TProperty>(OwnedNavigationBuilder.PrimitiveCollection(propertyExpression));

        public override TestNavigationBuilder Navigation<TNavigation>(
            Expression<Func<TDependentEntity, TNavigation?>> navigationExpression)
            where TNavigation : class
            => new GenericTestNavigationBuilder<TDependentEntity, TNavigation>(OwnedNavigationBuilder.Navigation(navigationExpression));

        public override TestNavigationBuilder Navigation<TNavigation>(
            Expression<Func<TDependentEntity, IEnumerable<TNavigation>?>> navigationExpression)
            where TNavigation : class
            => new GenericTestNavigationBuilder<TDependentEntity, TNavigation>(OwnedNavigationBuilder.Navigation(navigationExpression));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> Ignore(string propertyName)
            => Wrap(OwnedNavigationBuilder.Ignore(propertyName));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> Ignore(
            Expression<Func<TDependentEntity, object?>> propertyExpression)
            => Wrap(OwnedNavigationBuilder.Ignore(propertyExpression));

        public override TestIndexBuilder<TDependentEntity> HasIndex(params string[] propertyNames)
            => new GenericTestIndexBuilder<TDependentEntity>(OwnedNavigationBuilder.HasIndex(propertyNames));

        public override TestIndexBuilder<TDependentEntity> HasIndex(Expression<Func<TDependentEntity, object?>> indexExpression)
            => new GenericTestIndexBuilder<TDependentEntity>(OwnedNavigationBuilder.HasIndex(indexExpression));

        public override TestOwnershipBuilder<TEntity, TDependentEntity> WithOwner(string? ownerReference)
            => new GenericTestOwnershipBuilder<TEntity, TDependentEntity>(
                OwnedNavigationBuilder.WithOwner(ownerReference));

        public override TestOwnershipBuilder<TEntity, TDependentEntity> WithOwner(
            Expression<Func<TDependentEntity, TEntity?>>? referenceExpression = null)
            => new GenericTestOwnershipBuilder<TEntity, TDependentEntity>(
                OwnedNavigationBuilder.WithOwner(referenceExpression));

        public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
            Expression<Func<TDependentEntity, TNewDependentEntity?>> navigationExpression)
            where TNewDependentEntity : class
            => Wrap(OwnedNavigationBuilder.OwnsOne(navigationExpression));

        public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
            string entityTypeName,
            Expression<Func<TDependentEntity, TNewDependentEntity?>> navigationExpression)
            where TNewDependentEntity : class
            => Wrap(OwnedNavigationBuilder.OwnsOne(entityTypeName, navigationExpression));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
            Expression<Func<TDependentEntity, TNewDependentEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
            => Wrap(OwnedNavigationBuilder.OwnsOne(navigationExpression, r => buildAction(Wrap(r))));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
            string entityTypeName,
            Expression<Func<TDependentEntity, TNewDependentEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
            => Wrap(OwnedNavigationBuilder.OwnsOne(entityTypeName, navigationExpression, r => buildAction(Wrap(r))));

        public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression)
            where TNewDependentEntity : class
            => Wrap(OwnedNavigationBuilder.OwnsMany(navigationExpression));

        public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
            string entityTypeName,
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression)
            => Wrap(OwnedNavigationBuilder.OwnsMany(entityTypeName, navigationExpression));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class
            => Wrap(OwnedNavigationBuilder.OwnsMany(navigationExpression, r => buildAction(Wrap(r))));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
            string entityTypeName,
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            => Wrap(OwnedNavigationBuilder.OwnsMany(entityTypeName, navigationExpression, r => buildAction(Wrap(r))));

        public override TestReferenceNavigationBuilder<TDependentEntity, TRelatedEntity> HasOne<TRelatedEntity>(
            Expression<Func<TDependentEntity, TRelatedEntity?>>? navigationExpression = null)
            where TRelatedEntity : class
            => new GenericTestReferenceNavigationBuilder<TDependentEntity, TRelatedEntity>(
                OwnedNavigationBuilder.HasOne(navigationExpression));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> HasChangeTrackingStrategy(
            ChangeTrackingStrategy changeTrackingStrategy)
            => Wrap(OwnedNavigationBuilder.HasChangeTrackingStrategy(changeTrackingStrategy));

        public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> UsePropertyAccessMode(
            PropertyAccessMode propertyAccessMode)
            => Wrap(OwnedNavigationBuilder.UsePropertyAccessMode(propertyAccessMode));

        public override DataBuilder<TDependentEntity> HasData(params TDependentEntity[] data)
            => OwnedNavigationBuilder.HasData(data);

        public override DataBuilder<TDependentEntity> HasData(params object[] data)
            => OwnedNavigationBuilder.HasData(data);

        public override DataBuilder<TDependentEntity> HasData(IEnumerable<TDependentEntity> data)
            => OwnedNavigationBuilder.HasData(data);

        public override DataBuilder<TDependentEntity> HasData(IEnumerable<object> data)
            => OwnedNavigationBuilder.HasData(data);

        OwnedNavigationBuilder<TEntity, TDependentEntity> IInfrastructure<OwnedNavigationBuilder<TEntity, TDependentEntity>>.Instance
            => OwnedNavigationBuilder;
    }
}
