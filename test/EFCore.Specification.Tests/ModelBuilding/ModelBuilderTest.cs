// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

public abstract partial class ModelBuilderTest
{
    public abstract class ModelBuilderTestBase
    {
        protected ModelBuilderTestBase(ModelBuilderFixtureBase fixture)
        {
            Fixture = fixture;
        }

        protected virtual ModelBuilderFixtureBase Fixture { get; }

        protected abstract TestModelBuilder CreateModelBuilder(Action<ModelConfigurationBuilder>? configure = null);

        public virtual void AssertEqual(
            IReadOnlyModel expected,
            IReadOnlyModel actual,
            bool compareAnnotations = false)
            => Fixture.TestHelpers.ModelAsserter.AssertEqual(expected, actual, compareAnnotations);

        public virtual void AssertEqual(
            IEnumerable<IReadOnlyProperty> expectedProperties,
            IEnumerable<IReadOnlyProperty> actualProperties,
            bool assertOrder = false,
            bool compareAnnotations = false)
            => Fixture.TestHelpers.ModelAsserter.AssertEqual(expectedProperties, actualProperties, assertOrder, compareAnnotations);

        public virtual IReadOnlyModel Clone(IReadOnlyModel model)
            => Fixture.TestHelpers.ModelAsserter.Clone(model);

        protected TestModelBuilder HobNobBuilder()
        {
            var builder = CreateModelBuilder();

            builder.Entity<Hob>().HasKey(e => new { e.Id1, e.Id2 });
            builder.Entity<Nob>().HasKey(e => new { e.Id1, e.Id2 });

            return builder;
        }
    }

    public abstract class ModelBuilderFixtureBase
    {
        public abstract TestHelpers TestHelpers { get; }
        public virtual DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder) => builder;
        public virtual IServiceCollection AddServices(IServiceCollection services) => services;
    }

    public abstract class TestModelBuilder : IInfrastructure<ModelBuilder>
    {
        protected TestModelBuilder(ModelBuilderFixtureBase fixture, Action<ModelConfigurationBuilder>? configure)
        {
            var testHelpers = fixture.TestHelpers;
            var options = new LoggingOptions();
            options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(false).Options);
            ValidationLoggerFactory = new ListLoggerFactory(l => l == DbLoggerCategory.Model.Validation.Name);
            ValidationLogger = new DiagnosticsLogger<DbLoggerCategory.Model.Validation>(
                ValidationLoggerFactory,
                options,
                new DiagnosticListener("Fake"),
                testHelpers.LoggingDefinitions,
                new NullDbContextLogger());

            ModelLoggerFactory = new ListLoggerFactory(l => l == DbLoggerCategory.Model.Name);
            var modelLogger = new DiagnosticsLogger<DbLoggerCategory.Model>(
                ModelLoggerFactory,
                options,
                new DiagnosticListener("Fake"),
                testHelpers.LoggingDefinitions,
                new NullDbContextLogger());

            ModelBuilder = testHelpers.CreateConventionBuilder(
                modelLogger,
                ValidationLogger,
                configure,
                fixture.AddOptions,
                fixture.AddServices);
        }

        public virtual IMutableModel Model
            => ModelBuilder.Model;

        protected TestHelpers.TestModelBuilder ModelBuilder { get; }
        public ListLoggerFactory ValidationLoggerFactory { get; }
        public ListLoggerFactory ModelLoggerFactory { get; }
        protected virtual DiagnosticsLogger<DbLoggerCategory.Model.Validation> ValidationLogger { get; }

        public TestModelBuilder HasAnnotation(string annotation, object? value)
        {
            ModelBuilder.HasAnnotation(annotation, value);
            return this;
        }

        public abstract TestEntityTypeBuilder<TEntity> Entity<TEntity>()
            where TEntity : class;

        public abstract TestEntityTypeBuilder<TEntity> SharedTypeEntity<TEntity>(string name)
            where TEntity : class;

        public abstract TestOwnedEntityTypeBuilder<TEntity> Owned<TEntity>()
            where TEntity : class;

        public abstract TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
            where TEntity : class;

        public abstract TestModelBuilder SharedTypeEntity<TEntity>(string name, Action<TestEntityTypeBuilder<TEntity>> buildAction)
            where TEntity : class;

        public abstract TestModelBuilder Ignore<TEntity>()
            where TEntity : class;

        public virtual IModel FinalizeModel()
            => ModelBuilder.FinalizeModel(designTime: true);

        public virtual string GetDisplayName(Type entityType)
            => entityType.Name;

        public virtual TestModelBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
        {
            ModelBuilder.UsePropertyAccessMode(propertyAccessMode);

            return this;
        }

        ModelBuilder IInfrastructure<ModelBuilder>.Instance
            => ModelBuilder;
    }

    public abstract class TestEntityTypeBuilder<TEntity>
        where TEntity : class
    {
        public abstract IMutableEntityType Metadata { get; }
        public abstract TestEntityTypeBuilder<TEntity> HasAnnotation(string annotation, object? value);

        public abstract TestEntityTypeBuilder<TEntity> HasBaseType<TBaseEntity>()
            where TBaseEntity : class;

        public abstract TestEntityTypeBuilder<TEntity> HasBaseType(string? baseEntityTypeName);
        public abstract TestKeyBuilder<TEntity> HasKey(Expression<Func<TEntity, object?>> keyExpression);
        public abstract TestKeyBuilder<TEntity> HasKey(params string[] propertyNames);
        public abstract TestKeyBuilder<TEntity> HasAlternateKey(Expression<Func<TEntity, object?>> keyExpression);
        public abstract TestKeyBuilder<TEntity> HasAlternateKey(params string[] propertyNames);
        public abstract TestEntityTypeBuilder<TEntity> HasNoKey();

        public abstract TestPropertyBuilder<TProperty> Property<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression);

        public abstract TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName);

        public abstract TestPrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(
            Expression<Func<TEntity, TProperty>> propertyExpression);

        public abstract TestPrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(string propertyName);

        public abstract TestPropertyBuilder<TProperty> IndexerProperty<TProperty>(string propertyName);

        public abstract TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(string propertyName);

        public abstract TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
            Expression<Func<TEntity, TProperty?>> propertyExpression);

        public abstract TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
            Expression<Func<TEntity, TProperty?>> propertyExpression,
            string complexTypeName);

        public abstract TestEntityTypeBuilder<TEntity> ComplexProperty<TProperty>(
            Expression<Func<TEntity, TProperty?>> propertyExpression,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction);

        public abstract TestEntityTypeBuilder<TEntity> ComplexProperty<TProperty>(
            Expression<Func<TEntity, TProperty?>> propertyExpression,
            string complexTypeName,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction);

        public abstract TestEntityTypeBuilder<TEntity> ComplexProperty<TProperty>(
            string propertyName,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction);

        public abstract TestNavigationBuilder Navigation<TNavigation>(
            Expression<Func<TEntity, TNavigation?>> navigationExpression)
            where TNavigation : class;

        public abstract TestNavigationBuilder Navigation<TNavigation>(
            Expression<Func<TEntity, IEnumerable<TNavigation>?>> navigationExpression)
            where TNavigation : class;

        public abstract TestEntityTypeBuilder<TEntity> Ignore(
            Expression<Func<TEntity, object?>> propertyExpression);

        public abstract TestEntityTypeBuilder<TEntity> Ignore(string propertyName);

        public abstract TestIndexBuilder<TEntity> HasIndex(Expression<Func<TEntity, object?>> indexExpression);
        public abstract TestIndexBuilder<TEntity> HasIndex(Expression<Func<TEntity, object?>> indexExpression, string name);
        public abstract TestIndexBuilder<TEntity> HasIndex(params string[] propertyNames);

        public abstract TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(string navigationName)
            where TRelatedEntity : class;

        public abstract TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
            string entityTypeName,
            string navigationName)
            where TRelatedEntity : class;

        public abstract TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            string navigationName,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class;

        public abstract TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            string entityTypeName,
            string navigationName,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class;

        public abstract TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression)
            where TRelatedEntity : class;

        public abstract TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
            string entityTypeName,
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression)
            where TRelatedEntity : class;

        public abstract TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class;

        public abstract TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
            string entityTypeName,
            Expression<Func<TEntity, TRelatedEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class;

        public abstract TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(string navigationName)
            where TRelatedEntity : class;

        public abstract TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
            string entityTypeName,
            string navigationName)
            where TRelatedEntity : class;

        public abstract TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            string navigationName,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class;

        public abstract TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            string entityTypeName,
            string navigationName,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class;

        public abstract TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression)
            where TRelatedEntity : class;

        public abstract TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
            string entityTypeName,
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression)
            where TRelatedEntity : class;

        public abstract TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class;

        public abstract TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
            string entityTypeName,
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
            where TRelatedEntity : class;

        public abstract TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
            string? navigationName)
            where TRelatedEntity : class;

        public abstract TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
            Expression<Func<TEntity, TRelatedEntity?>>? navigationExpression = null)
            where TRelatedEntity : class;

        public abstract TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
            string? navigationName)
            where TRelatedEntity : class;

        public abstract TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
            Expression<Func<TEntity, IEnumerable<TRelatedEntity>?>>? navigationExpression = null)
            where TRelatedEntity : class;

        public abstract TestEntityTypeBuilder<TEntity> HasQueryFilter(Expression<Func<TEntity, bool>> filter);

        public abstract TestEntityTypeBuilder<TEntity> HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy);

        public abstract TestEntityTypeBuilder<TEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode);

        public abstract DataBuilder<TEntity> HasData(params TEntity[] data);

        public abstract DataBuilder<TEntity> HasData(params object[] data);

        public abstract DataBuilder<TEntity> HasData(IEnumerable<TEntity> data);

        public abstract DataBuilder<TEntity> HasData(IEnumerable<object> data);

        public abstract TestDiscriminatorBuilder<TDiscriminator> HasDiscriminator<TDiscriminator>(
            Expression<Func<TEntity, TDiscriminator>> propertyExpression);

        public abstract TestDiscriminatorBuilder<TDiscriminator> HasDiscriminator<TDiscriminator>(string propertyName);

        public abstract TestEntityTypeBuilder<TEntity> HasNoDiscriminator();
    }

    public abstract class TestComplexPropertyBuilder<TComplex>
    {
        public abstract IMutableComplexProperty Metadata { get; }
        public abstract TestComplexPropertyBuilder<TComplex> HasTypeAnnotation(string annotation, object? value);
        public abstract TestComplexPropertyBuilder<TComplex> HasPropertyAnnotation(string annotation, object? value);

        public abstract TestComplexTypePropertyBuilder<TProperty> Property<TProperty>(
            Expression<Func<TComplex, TProperty>> propertyExpression);

        public abstract TestComplexTypePropertyBuilder<TProperty> Property<TProperty>(string propertyName);

        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(
            Expression<Func<TComplex, TProperty>> propertyExpression);

        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(string propertyName);

        public abstract TestComplexTypePropertyBuilder<TProperty> IndexerProperty<TProperty>(string propertyName);

        public abstract TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(string propertyName);

        public abstract TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
            Expression<Func<TComplex, TProperty?>> propertyExpression);

        public abstract TestComplexPropertyBuilder<TProperty> ComplexProperty<TProperty>(
            Expression<Func<TComplex, TProperty?>> propertyExpression,
            string complexTypeName);

        public abstract TestComplexPropertyBuilder<TComplex> ComplexProperty<TProperty>(
            Expression<Func<TComplex, TProperty?>> propertyExpression,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction);

        public abstract TestComplexPropertyBuilder<TComplex> ComplexProperty<TProperty>(
            Expression<Func<TComplex, TProperty?>> propertyExpression,
            string complexTypeName,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction);

        public abstract TestComplexPropertyBuilder<TComplex> ComplexProperty<TProperty>(
            string propertyName,
            Action<TestComplexPropertyBuilder<TProperty>> buildAction);

        public abstract TestComplexPropertyBuilder<TComplex> Ignore(
            Expression<Func<TComplex, object?>> propertyExpression);

        public abstract TestComplexPropertyBuilder<TComplex> Ignore(string propertyName);
        public abstract TestComplexPropertyBuilder<TComplex> IsRequired(bool isRequired = true);
        public abstract TestComplexPropertyBuilder<TComplex> HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy);
        public abstract TestComplexPropertyBuilder<TComplex> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode);
        public abstract TestComplexPropertyBuilder<TComplex> UseDefaultPropertyAccessMode(PropertyAccessMode propertyAccessMode);
    }

    public abstract class TestDiscriminatorBuilder<TDiscriminator>
    {
        public abstract TestDiscriminatorBuilder<TDiscriminator> IsComplete(bool complete);

        public abstract TestDiscriminatorBuilder<TDiscriminator> HasValue(TDiscriminator value);

        public abstract TestDiscriminatorBuilder<TDiscriminator> HasValue<TEntity>(TDiscriminator value);

        public abstract TestDiscriminatorBuilder<TDiscriminator> HasValue(Type entityType, TDiscriminator value);

        public abstract TestDiscriminatorBuilder<TDiscriminator> HasValue(string entityTypeName, TDiscriminator value);
    }

    public abstract class TestOwnedEntityTypeBuilder<TEntity>
        where TEntity : class;

    public abstract class TestKeyBuilder<TEntity>
    {
        public abstract IMutableKey Metadata { get; }

        public abstract TestKeyBuilder<TEntity> HasAnnotation(string annotation, object? value);
    }

    public abstract class TestIndexBuilder<TEntity>
    {
        public abstract IMutableIndex Metadata { get; }

        public abstract TestIndexBuilder<TEntity> HasAnnotation(string annotation, object? value);
        public abstract TestIndexBuilder<TEntity> IsUnique(bool isUnique = true);
        public abstract TestIndexBuilder<TEntity> IsDescending(params bool[] isDescending);
    }

    public abstract class TestPropertyBuilder<TProperty>
    {
        public abstract IMutableProperty Metadata { get; }
        public abstract TestPropertyBuilder<TProperty> HasAnnotation(string annotation, object? value);
        public abstract TestPropertyBuilder<TProperty> IsRequired(bool isRequired = true);
        public abstract TestPropertyBuilder<TProperty> HasMaxLength(int maxLength);
        public abstract TestPropertyBuilder<TProperty> HasSentinel(TProperty? sentinel);
        public abstract TestPropertyBuilder<TProperty> HasPrecision(int precision);
        public abstract TestPropertyBuilder<TProperty> HasPrecision(int precision, int scale);
        public abstract TestPropertyBuilder<TProperty> IsUnicode(bool unicode = true);
        public abstract TestPropertyBuilder<TProperty> IsRowVersion();
        public abstract TestPropertyBuilder<TProperty> IsConcurrencyToken(bool isConcurrencyToken = true);

        public abstract TestPropertyBuilder<TProperty> ValueGeneratedNever();
        public abstract TestPropertyBuilder<TProperty> ValueGeneratedOnAdd();
        public abstract TestPropertyBuilder<TProperty> ValueGeneratedOnAddOrUpdate();
        public abstract TestPropertyBuilder<TProperty> ValueGeneratedOnUpdate();

        public abstract TestPropertyBuilder<TProperty> HasValueGenerator<TGenerator>()
            where TGenerator : ValueGenerator;

        public abstract TestPropertyBuilder<TProperty> HasValueGenerator(Type valueGeneratorType);

        public abstract TestPropertyBuilder<TProperty> HasValueGenerator(
            Func<IReadOnlyProperty, ITypeBase, ValueGenerator> factory);

        public abstract TestPropertyBuilder<TProperty> HasValueGeneratorFactory<TFactory>()
            where TFactory : ValueGeneratorFactory;

        public abstract TestPropertyBuilder<TProperty> HasValueGeneratorFactory(Type valueGeneratorFactoryType);

        public abstract TestPropertyBuilder<TProperty> HasField(string fieldName);
        public abstract TestPropertyBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode);

        public abstract TestPropertyBuilder<TProperty> HasConversion<TConversion>();
        public abstract TestPropertyBuilder<TProperty> HasConversion<TConversion>(ValueComparer? valueComparer);

        public abstract TestPropertyBuilder<TProperty> HasConversion<TConversion>(
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType);

        public abstract TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression);

        public abstract TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            ValueComparer? valueComparer);

        public abstract TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType);

        public abstract TestPropertyBuilder<TProperty> HasConversion<TProvider>(ValueConverter<TProperty, TProvider> converter);

        public abstract TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            ValueConverter<TProperty, TProvider> converter,
            ValueComparer? valueComparer);

        public abstract TestPropertyBuilder<TProperty> HasConversion<TProvider>(
            ValueConverter<TProperty, TProvider> converter,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType);

        public abstract TestPropertyBuilder<TProperty> HasConversion(ValueConverter? converter);
        public abstract TestPropertyBuilder<TProperty> HasConversion(ValueConverter? converter, ValueComparer? valueComparer);

        public abstract TestPropertyBuilder<TProperty> HasConversion(
            ValueConverter? converter,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType);

        public abstract TestPropertyBuilder<TProperty> HasConversion<TConverter, TComparer>()
            where TComparer : ValueComparer;

        public abstract TestPropertyBuilder<TProperty> HasConversion<TConverter, TComparer, TProviderComparer>()
            where TComparer : ValueComparer
            where TProviderComparer : ValueComparer;
    }

    public abstract class TestPrimitiveCollectionBuilder<TProperty>
    {
        public abstract IMutableProperty Metadata { get; }
        public abstract TestElementTypeBuilder ElementType();
        public abstract TestPrimitiveCollectionBuilder<TProperty> ElementType(Action<TestElementTypeBuilder> builderAction);
        public abstract TestPrimitiveCollectionBuilder<TProperty> HasAnnotation(string annotation, object? value);
        public abstract TestPrimitiveCollectionBuilder<TProperty> IsRequired(bool isRequired = true);
        public abstract TestPrimitiveCollectionBuilder<TProperty> HasMaxLength(int maxLength);
        public abstract TestPrimitiveCollectionBuilder<TProperty> HasSentinel(TProperty? sentinel);
        public abstract TestPrimitiveCollectionBuilder<TProperty> IsUnicode(bool unicode = true);
        public abstract TestPrimitiveCollectionBuilder<TProperty> IsConcurrencyToken(bool isConcurrencyToken = true);

        public abstract TestPrimitiveCollectionBuilder<TProperty> ValueGeneratedNever();
        public abstract TestPrimitiveCollectionBuilder<TProperty> ValueGeneratedOnAdd();
        public abstract TestPrimitiveCollectionBuilder<TProperty> ValueGeneratedOnAddOrUpdate();
        public abstract TestPrimitiveCollectionBuilder<TProperty> ValueGeneratedOnUpdate();

        public abstract TestPrimitiveCollectionBuilder<TProperty> HasValueGenerator<TGenerator>()
            where TGenerator : ValueGenerator;

        public abstract TestPrimitiveCollectionBuilder<TProperty> HasValueGenerator(Type valueGeneratorType);

        public abstract TestPrimitiveCollectionBuilder<TProperty> HasValueGeneratorFactory<TFactory>()
            where TFactory : ValueGeneratorFactory;

        public abstract TestPrimitiveCollectionBuilder<TProperty> HasValueGeneratorFactory(Type valueGeneratorFactoryType);

        public abstract TestPrimitiveCollectionBuilder<TProperty> HasField(string fieldName);
        public abstract TestPrimitiveCollectionBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode);
    }

    public class TestElementTypeBuilder(ElementTypeBuilder elementTypeBuilder)
    {
        public virtual ElementTypeBuilder ElementTypeBuilder { get; } = elementTypeBuilder;

        public virtual IMutableElementType Metadata
            => ElementTypeBuilder.Metadata;

        protected virtual TestElementTypeBuilder Wrap(ElementTypeBuilder elementTypeBuilder)
            => new(elementTypeBuilder);

        public virtual TestElementTypeBuilder HasAnnotation(string annotation, object? value)
            => Wrap(ElementTypeBuilder.HasAnnotation(annotation, value));

        public virtual TestElementTypeBuilder IsRequired(bool required = true)
            => Wrap(ElementTypeBuilder.IsRequired(required));

        public virtual TestElementTypeBuilder HasMaxLength(int maxLength)
            => Wrap(ElementTypeBuilder.HasMaxLength(maxLength));

        public virtual TestElementTypeBuilder HasPrecision(int precision, int scale)
            => Wrap(ElementTypeBuilder.HasPrecision(precision, scale));

        public virtual TestElementTypeBuilder HasPrecision(int precision)
            => Wrap(ElementTypeBuilder.HasPrecision(precision));

        public virtual TestElementTypeBuilder IsUnicode(bool unicode = true)
            => Wrap(ElementTypeBuilder.IsUnicode(unicode));

        public virtual TestElementTypeBuilder HasConversion<TConversion>()
            => Wrap(ElementTypeBuilder.HasConversion<TConversion>());

        public virtual TestElementTypeBuilder HasConversion(Type? conversionType)
            => Wrap(ElementTypeBuilder.HasConversion(conversionType));

        public virtual TestElementTypeBuilder HasConversion(ValueConverter? converter)
            => Wrap(ElementTypeBuilder.HasConversion(converter));

        public virtual TestElementTypeBuilder HasConversion<TConversion>(ValueComparer? valueComparer)
            => Wrap(ElementTypeBuilder.HasConversion<TConversion>(valueComparer));

        public virtual TestElementTypeBuilder HasConversion(Type conversionType, ValueComparer? valueComparer)
            => Wrap(ElementTypeBuilder.HasConversion(conversionType, valueComparer));

        public virtual TestElementTypeBuilder HasConversion(ValueConverter? converter, ValueComparer? valueComparer)
            => Wrap(ElementTypeBuilder.HasConversion(converter, valueComparer));

        public virtual TestElementTypeBuilder HasConversion<TConversion, TComparer>()
            where TComparer : ValueComparer
            => Wrap(ElementTypeBuilder.HasConversion<TConversion, TComparer>());

        public virtual TestElementTypeBuilder HasConversion(Type conversionType, Type? comparerType)
            => Wrap(ElementTypeBuilder.HasConversion(conversionType, comparerType));
    }

    public abstract class TestComplexTypePropertyBuilder<TProperty>
    {
        public abstract IMutableProperty Metadata { get; }
        public abstract TestComplexTypePropertyBuilder<TProperty> HasAnnotation(string annotation, object? value);
        public abstract TestComplexTypePropertyBuilder<TProperty> IsRequired(bool isRequired = true);
        public abstract TestComplexTypePropertyBuilder<TProperty> HasMaxLength(int maxLength);
        public abstract TestComplexTypePropertyBuilder<TProperty> HasSentinel(TProperty? sentinel);
        public abstract TestComplexTypePropertyBuilder<TProperty> HasPrecision(int precision);
        public abstract TestComplexTypePropertyBuilder<TProperty> HasPrecision(int precision, int scale);
        public abstract TestComplexTypePropertyBuilder<TProperty> IsUnicode(bool unicode = true);
        public abstract TestComplexTypePropertyBuilder<TProperty> IsRowVersion();
        public abstract TestComplexTypePropertyBuilder<TProperty> IsConcurrencyToken(bool isConcurrencyToken = true);

        public abstract TestComplexTypePropertyBuilder<TProperty> ValueGeneratedNever();
        public abstract TestComplexTypePropertyBuilder<TProperty> ValueGeneratedOnAdd();
        public abstract TestComplexTypePropertyBuilder<TProperty> ValueGeneratedOnAddOrUpdate();
        public abstract TestComplexTypePropertyBuilder<TProperty> ValueGeneratedOnUpdate();

        public abstract TestComplexTypePropertyBuilder<TProperty> HasValueGenerator<TGenerator>()
            where TGenerator : ValueGenerator;

        public abstract TestComplexTypePropertyBuilder<TProperty> HasValueGenerator(Type valueGeneratorType);

        public abstract TestComplexTypePropertyBuilder<TProperty> HasValueGeneratorFactory<TFactory>()
            where TFactory : ValueGeneratorFactory;

        public abstract TestComplexTypePropertyBuilder<TProperty> HasValueGeneratorFactory(Type valueGeneratorFactoryType);

        public abstract TestComplexTypePropertyBuilder<TProperty> HasField(string fieldName);
        public abstract TestComplexTypePropertyBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode);

        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion<TConversion>();
        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion<TConversion>(ValueComparer? valueComparer);

        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion<TConversion>(
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType);

        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression);

        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            ValueComparer? valueComparer);

        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(
            Expression<Func<TProperty, TProvider>> convertToProviderExpression,
            Expression<Func<TProvider, TProperty>> convertFromProviderExpression,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType);

        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(ValueConverter<TProperty, TProvider> converter);

        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(
            ValueConverter<TProperty, TProvider> converter,
            ValueComparer? valueComparer);

        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion<TProvider>(
            ValueConverter<TProperty, TProvider> converter,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType);

        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion(ValueConverter? converter);
        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion(ValueConverter? converter, ValueComparer? valueComparer);

        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion(
            ValueConverter? converter,
            ValueComparer? valueComparer,
            ValueComparer? providerComparerType);

        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion<TConverter, TComparer>()
            where TComparer : ValueComparer;

        public abstract TestComplexTypePropertyBuilder<TProperty> HasConversion<TConverter, TComparer, TProviderComparer>()
            where TComparer : ValueComparer
            where TProviderComparer : ValueComparer;
    }

    public abstract class TestComplexTypePrimitiveCollectionBuilder<TProperty>
    {
        public abstract IMutableProperty Metadata { get; }
        public abstract TestElementTypeBuilder ElementType();
        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> ElementType(Action<TestElementTypeBuilder> builderAction);
        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> HasAnnotation(string annotation, object? value);
        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> IsRequired(bool isRequired = true);
        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> HasMaxLength(int maxLength);
        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> HasSentinel(TProperty? sentinel);
        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> IsUnicode(bool unicode = true);
        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> IsConcurrencyToken(bool isConcurrencyToken = true);
        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> ValueGeneratedNever();
        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> ValueGeneratedOnAdd();
        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> ValueGeneratedOnAddOrUpdate();
        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> ValueGeneratedOnUpdate();

        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> HasValueGenerator<TGenerator>()
            where TGenerator : ValueGenerator;

        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> HasValueGenerator(Type valueGeneratorType);

        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> HasValueGeneratorFactory<TFactory>()
            where TFactory : ValueGeneratorFactory;

        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> HasValueGeneratorFactory(Type valueGeneratorFactoryType);
        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> HasField(string fieldName);
        public abstract TestComplexTypePrimitiveCollectionBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode);
    }

    public abstract class TestNavigationBuilder
    {
        public abstract TestNavigationBuilder HasAnnotation(string annotation, object? value);
        public abstract TestNavigationBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode);
        public abstract TestNavigationBuilder HasField(string fieldName);
        public abstract TestNavigationBuilder AutoInclude(bool autoInclude = true);
        public abstract TestNavigationBuilder EnableLazyLoading(bool lazyLoadingEnabled = true);
        public abstract TestNavigationBuilder IsRequired(bool required = true);
    }

    public abstract class TestCollectionNavigationBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(string? navigationName);

        public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(
            Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression = null);

        public abstract TestCollectionCollectionBuilder<TRelatedEntity, TEntity> WithMany(string? navigationName = null);

        public abstract TestCollectionCollectionBuilder<TRelatedEntity, TEntity> WithMany(
            Expression<Func<TRelatedEntity, IEnumerable<TEntity>?>> navigationExpression);
    }

    public abstract class TestReferenceNavigationBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        public abstract TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(string? navigationName);

        public abstract TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(
            Expression<Func<TRelatedEntity, IEnumerable<TEntity>?>>? navigationExpression = null);

        public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(string? navigationName);

        public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
            Expression<Func<TRelatedEntity, TEntity?>>? navigationExpression = null);
    }

    public abstract class TestReferenceCollectionBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        public abstract IMutableForeignKey Metadata { get; }

        public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(
            Expression<Func<TRelatedEntity, object?>> foreignKeyExpression);

        public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
            Expression<Func<TEntity, object?>> keyExpression);

        public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(
            params string[] foreignKeyPropertyNames);

        public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
            params string[] keyPropertyNames);

        public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasAnnotation(
            string annotation,
            object? value);

        public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> IsRequired(bool isRequired = true);

        public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior);
    }

    public abstract class TestReferenceReferenceBuilder<TEntity, TRelatedEntity>
        where TEntity : class
        where TRelatedEntity : class
    {
        public abstract IMutableForeignKey Metadata { get; }

        public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasAnnotation(
            string annotation,
            object? value);

        public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
            Expression<Func<TDependentEntity, object?>> foreignKeyExpression)
            where TDependentEntity : class;

        public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
            Expression<Func<TPrincipalEntity, object?>> keyExpression)
            where TPrincipalEntity : class;

        public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
            params string[] foreignKeyPropertyNames)
            where TDependentEntity : class;

        public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
            params string[] keyPropertyNames)
            where TPrincipalEntity : class;

        public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> IsRequired(bool isRequired = true);

        public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior);
    }

    public abstract class TestCollectionCollectionBuilder<TLeftEntity, TRightEntity>
        where TLeftEntity : class
        where TRightEntity : class
    {
        public abstract TestEntityTypeBuilder<Dictionary<string, object>> UsingEntity(
            string joinEntityName);

        public abstract TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>()
            where TJoinEntity : class;

        public abstract TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            string joinEntityName)
            where TJoinEntity : class;

        public abstract TestEntityTypeBuilder<TRightEntity> UsingEntity(
            Action<TestEntityTypeBuilder<Dictionary<string, object>>> configureJoinEntityType);

        public abstract TestEntityTypeBuilder<TRightEntity> UsingEntity(
            string joinEntityName,
            Action<TestEntityTypeBuilder<Dictionary<string, object>>> configureJoinEntityType);

        public abstract TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            where TJoinEntity : class;

        public abstract TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            string joinEntityName,
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            where TJoinEntity : class;

        public abstract TestEntityTypeBuilder<Dictionary<string, object>> UsingEntity(
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>> configureRight,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>> configureLeft);

        public abstract TestEntityTypeBuilder<Dictionary<string, object>> UsingEntity(
            string joinEntityName,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>> configureRight,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>> configureLeft);

        public abstract TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft)
            where TJoinEntity : class;

        public abstract TestEntityTypeBuilder<TJoinEntity> UsingEntity<TJoinEntity>(
            string joinEntityName,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft)
            where TJoinEntity : class;

        public abstract TestEntityTypeBuilder<TRightEntity> UsingEntity(
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>> configureRight,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>> configureLeft,
            Action<TestEntityTypeBuilder<Dictionary<string, object>>> configureJoinEntityType);

        public abstract TestEntityTypeBuilder<TRightEntity> UsingEntity(
            string joinEntityName,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TLeftEntity, Dictionary<string, object>>> configureRight,
            Func<TestEntityTypeBuilder<Dictionary<string, object>>,
                TestReferenceCollectionBuilder<TRightEntity, Dictionary<string, object>>> configureLeft,
            Action<TestEntityTypeBuilder<Dictionary<string, object>>> configureJoinEntityType);

        public abstract TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            where TJoinEntity : class;

        public abstract TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
            string joinEntityName,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
            Func<TestEntityTypeBuilder<TJoinEntity>,
                TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
            Action<TestEntityTypeBuilder<TJoinEntity>> configureJoinEntityType)
            where TJoinEntity : class;
    }

    public abstract class TestOwnershipBuilder<TEntity, TDependentEntity>
        where TEntity : class
        where TDependentEntity : class
    {
        public abstract IMutableForeignKey Metadata { get; }

        public abstract TestOwnershipBuilder<TEntity, TDependentEntity> HasAnnotation(
            string annotation,
            object? value);

        public abstract TestOwnershipBuilder<TEntity, TDependentEntity> HasForeignKey(
            params string[] foreignKeyPropertyNames);

        public abstract TestOwnershipBuilder<TEntity, TDependentEntity> HasForeignKey(
            Expression<Func<TDependentEntity, object?>> foreignKeyExpression);

        public abstract TestOwnershipBuilder<TEntity, TDependentEntity> HasPrincipalKey(
            params string[] keyPropertyNames);

        public abstract TestOwnershipBuilder<TEntity, TDependentEntity> HasPrincipalKey(
            Expression<Func<TEntity, object?>> keyExpression);
    }

    public abstract class TestOwnedNavigationBuilder<TEntity, TDependentEntity>
        where TEntity : class
        where TDependentEntity : class
    {
        public abstract IMutableForeignKey Metadata { get; }
        public abstract IMutableEntityType OwnedEntityType { get; }

        public abstract TestOwnedNavigationBuilder<TEntity, TDependentEntity> HasAnnotation(
            string annotation,
            object? value);

        public abstract TestKeyBuilder<TDependentEntity> HasKey(Expression<Func<TDependentEntity, object?>> keyExpression);
        public abstract TestKeyBuilder<TDependentEntity> HasKey(params string[] propertyNames);

        public abstract TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName);
        public abstract TestPropertyBuilder<TProperty> IndexerProperty<TProperty>(string propertyName);

        public abstract TestPropertyBuilder<TProperty> Property<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression);

        public abstract TestPrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(string propertyName);

        public abstract TestPrimitiveCollectionBuilder<TProperty> PrimitiveCollection<TProperty>(
            Expression<Func<TDependentEntity, TProperty>> propertyExpression);

        public abstract TestNavigationBuilder Navigation<TNavigation>(
            Expression<Func<TDependentEntity, TNavigation?>> navigationExpression)
            where TNavigation : class;

        public abstract TestNavigationBuilder Navigation<TNavigation>(
            Expression<Func<TDependentEntity, IEnumerable<TNavigation>?>> navigationExpression)
            where TNavigation : class;

        public abstract TestOwnedNavigationBuilder<TEntity, TDependentEntity> Ignore(string propertyName);

        public abstract TestOwnedNavigationBuilder<TEntity, TDependentEntity> Ignore(
            Expression<Func<TDependentEntity, object?>> propertyExpression);

        public abstract TestIndexBuilder<TDependentEntity> HasIndex(params string[] propertyNames);
        public abstract TestIndexBuilder<TDependentEntity> HasIndex(Expression<Func<TDependentEntity, object?>> indexExpression);

        public abstract TestOwnershipBuilder<TEntity, TDependentEntity> WithOwner(string? ownerReference);

        public abstract TestOwnershipBuilder<TEntity, TDependentEntity> WithOwner(
            Expression<Func<TDependentEntity, TEntity?>>? referenceExpression = null);

        public abstract TestOwnedNavigationBuilder<TDependentEntity, TNewRelatedEntity> OwnsOne<TNewRelatedEntity>(
            Expression<Func<TDependentEntity, TNewRelatedEntity?>> navigationExpression)
            where TNewRelatedEntity : class;

        public abstract TestOwnedNavigationBuilder<TDependentEntity, TNewRelatedEntity> OwnsOne<TNewRelatedEntity>(
            string entityTypeName,
            Expression<Func<TDependentEntity, TNewRelatedEntity?>> navigationExpression)
            where TNewRelatedEntity : class;

        public abstract TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewRelatedEntity>(
            Expression<Func<TDependentEntity, TNewRelatedEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewRelatedEntity>> buildAction)
            where TNewRelatedEntity : class;

        public abstract TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewRelatedEntity>(
            string entityTypeName,
            Expression<Func<TDependentEntity, TNewRelatedEntity?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewRelatedEntity>> buildAction)
            where TNewRelatedEntity : class;

        public abstract TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression)
            where TNewDependentEntity : class;

        public abstract TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
            string entityTypeName,
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression)
            where TNewDependentEntity : class;

        public abstract TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class;

        public abstract TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
            string entityTypeName,
            Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>?>> navigationExpression,
            Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
            where TNewDependentEntity : class;

        public abstract TestReferenceNavigationBuilder<TDependentEntity, TRelatedEntity> HasOne<TRelatedEntity>(
            Expression<Func<TDependentEntity, TRelatedEntity?>>? navigationExpression = null)
            where TRelatedEntity : class;

        public abstract TestOwnedNavigationBuilder<TEntity, TDependentEntity> HasChangeTrackingStrategy(
            ChangeTrackingStrategy changeTrackingStrategy);

        public abstract TestOwnedNavigationBuilder<TEntity, TDependentEntity> UsePropertyAccessMode(
            PropertyAccessMode propertyAccessMode);

        public abstract DataBuilder<TDependentEntity> HasData(params TDependentEntity[] data);

        public abstract DataBuilder<TDependentEntity> HasData(params object[] data);

        public abstract DataBuilder<TDependentEntity> HasData(IEnumerable<TDependentEntity> data);

        public abstract DataBuilder<TDependentEntity> HasData(IEnumerable<object> data);
    }
}
