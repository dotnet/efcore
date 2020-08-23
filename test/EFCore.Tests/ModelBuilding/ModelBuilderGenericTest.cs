// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class ModelBuilderGenericTest : ModelBuilderTest
    {
        [ConditionalFact]
        public void Can_create_a_model_builder_with_given_conventions_only()
        {
            var convention = new TestConvention();
            var conventions = new ConventionSet();
            conventions.EntityTypeAddedConventions.Add(convention);

            var modelBuilder = new ModelBuilder(conventions);

            modelBuilder.Entity<Random>();

            Assert.True(convention.Applied);
            Assert.NotNull(modelBuilder.Model.FindEntityType(typeof(Random)));
        }

        private class TestConvention : IEntityTypeAddedConvention
        {
            public bool Applied { get; private set; }

            public void ProcessEntityTypeAdded(
                IConventionEntityTypeBuilder entityTypeBuilder,
                IConventionContext<IConventionEntityTypeBuilder> context)
            {
                Applied = true;
            }
        }

        [ConditionalFact]
        public void Can_discover_large_models_through_navigations()
        {
            var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<GiantModel.RelatedEntity1>();

            Assert.Equal(2000, modelBuilder.Model.GetEntityTypes().Count());
        }

        public class GenericNonRelationship : NonRelationshipTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new GenericTestModelBuilder(testHelpers);

            [ConditionalFact]
            public virtual void Changing_propertyInfo_updates_Property()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<DoubleProperty>().Property(e => ((IReplacable)e).Property);

                modelBuilder.FinalizeModel();

                var property = modelBuilder.Model.FindEntityType(typeof(DoubleProperty)).GetProperty("Property");
                Assert.EndsWith(typeof(IReplacable).Name + "." + nameof(IReplacable.Property), property.GetIdentifyingMemberInfo().Name);
            }
        }

        public class GenericInheritance : InheritanceTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new GenericTestModelBuilder(testHelpers);
        }

        public class GenericOwnedTypes : OwnedTypesTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new GenericTestModelBuilder(testHelpers);
        }

        public class GenericKeylessEntities : KeylessEntitiesTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new GenericTestModelBuilder(testHelpers);
        }

        public class GenericOneToMany : OneToManyTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new GenericTestModelBuilder(testHelpers);
        }

        public class GenericManyToOne : ManyToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new GenericTestModelBuilder(testHelpers);
        }

        public class GenericManyToMany : ManyToManyTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new GenericTestModelBuilder(testHelpers);
        }

        public class GenericOneToOne : OneToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new GenericTestModelBuilder(testHelpers);
        }

        protected class GenericTestModelBuilder : TestModelBuilder
        {
            public GenericTestModelBuilder(TestHelpers testHelpers)
                : base(testHelpers)
            {
            }

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

        protected class GenericTestEntityTypeBuilder<TEntity> : TestEntityTypeBuilder<TEntity>, IInfrastructure<EntityTypeBuilder<TEntity>>
            where TEntity : class
        {
            public GenericTestEntityTypeBuilder(EntityTypeBuilder<TEntity> entityTypeBuilder)
            {
                EntityTypeBuilder = entityTypeBuilder;
            }

            protected EntityTypeBuilder<TEntity> EntityTypeBuilder { get; }

            public override IMutableEntityType Metadata
                => EntityTypeBuilder.Metadata;

            protected virtual TestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder<TEntity> entityTypeBuilder)
                => new GenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder);

            public override TestEntityTypeBuilder<TEntity> HasAnnotation(string annotation, object value)
                => Wrap(EntityTypeBuilder.HasAnnotation(annotation, value));

            public override TestEntityTypeBuilder<TEntity> HasBaseType<TBaseEntity>()
                => Wrap(EntityTypeBuilder.HasBaseType<TBaseEntity>());

            public override TestEntityTypeBuilder<TEntity> HasBaseType(string baseEntityTypeName)
                => Wrap(EntityTypeBuilder.HasBaseType(baseEntityTypeName));

            public override TestKeyBuilder<TEntity> HasKey(Expression<Func<TEntity, object>> keyExpression)
                => new GenericTestKeyBuilder<TEntity>((KeyBuilder<TEntity>)EntityTypeBuilder.HasKey(keyExpression));

            public override TestKeyBuilder<TEntity> HasKey(params string[] propertyNames)
                => new GenericTestKeyBuilder<TEntity>(EntityTypeBuilder.HasKey(propertyNames));

            public override TestKeyBuilder<TEntity> HasAlternateKey(Expression<Func<TEntity, object>> keyExpression)
                => new GenericTestKeyBuilder<TEntity>(EntityTypeBuilder.HasAlternateKey(keyExpression));

            public override TestKeyBuilder<TEntity> HasAlternateKey(params string[] propertyNames)
                => new GenericTestKeyBuilder<TEntity>(EntityTypeBuilder.HasAlternateKey(propertyNames));

            public override TestEntityTypeBuilder<TEntity> HasNoKey()
                => Wrap(EntityTypeBuilder.HasNoKey());

            public override TestPropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
                => new GenericTestPropertyBuilder<TProperty>(EntityTypeBuilder.Property(propertyExpression));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
                => new GenericTestPropertyBuilder<TProperty>(EntityTypeBuilder.Property<TProperty>(propertyName));

            public override TestPropertyBuilder<TProperty> IndexerProperty<TProperty>(string propertyName)
                => new GenericTestPropertyBuilder<TProperty>(EntityTypeBuilder.IndexerProperty<TProperty>(propertyName));

            public override TestNavigationBuilder Navigation<TNavigation>(
                Expression<Func<TEntity, TNavigation>> navigationExpression)
                => new GenericTestNavigationBuilder<TEntity, TNavigation>(EntityTypeBuilder.Navigation(navigationExpression));

            public override TestNavigationBuilder Navigation<TNavigation>(
                Expression<Func<TEntity, IEnumerable<TNavigation>>> navigationExpression)
                => new GenericTestNavigationBuilder<TEntity, TNavigation>(EntityTypeBuilder.Navigation(navigationExpression));

            public override TestEntityTypeBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> propertyExpression)
                => Wrap(EntityTypeBuilder.Ignore(propertyExpression));

            public override TestEntityTypeBuilder<TEntity> Ignore(string propertyName)
                => Wrap(EntityTypeBuilder.Ignore(propertyName));

            public override TestIndexBuilder<TEntity> HasIndex(Expression<Func<TEntity, object>> indexExpression)
                => new GenericTestIndexBuilder<TEntity>(EntityTypeBuilder.HasIndex(indexExpression));

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
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression)
                => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.OwnsOne(navigationExpression));

            public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
                string entityTypeName,
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression)
                => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.OwnsOne(
                        entityTypeName, navigationExpression));

            public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(
                    EntityTypeBuilder.OwnsOne(
                        navigationExpression,
                        r => buildAction(new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

            public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
                string entityTypeName,
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
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
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression)
                => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.OwnsMany(navigationExpression));

            public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
                string entityTypeName,
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression)
                => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.OwnsMany(entityTypeName, navigationExpression));

            public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(
                    EntityTypeBuilder.OwnsMany(
                        navigationExpression,
                        r => buildAction(new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

            public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
                string entityTypeName,
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(
                    EntityTypeBuilder.OwnsMany(
                        entityTypeName, navigationExpression,
                        r => buildAction(new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(string navigationName)
                => new GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.HasOne<TRelatedEntity>(navigationName));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression = null)
                => new GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasOne(navigationExpression));

            public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(string navigationName)
                => new GenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.HasMany<TRelatedEntity>(navigationName));

            public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression = null)
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
                => new GenericTestDiscriminatorBuilder<TDiscriminator>(EntityTypeBuilder.HasDiscriminator(propertyExpression));

            public override TestDiscriminatorBuilder<TDiscriminator> HasDiscriminator<TDiscriminator>(string propertyName)
                => new GenericTestDiscriminatorBuilder<TDiscriminator>(EntityTypeBuilder.HasDiscriminator<TDiscriminator>(propertyName));

            public override TestEntityTypeBuilder<TEntity> HasNoDiscriminator()
                => Wrap(EntityTypeBuilder.HasNoDiscriminator());

            public EntityTypeBuilder<TEntity> Instance
                => EntityTypeBuilder;
        }

        protected class GenericTestDiscriminatorBuilder<TDiscriminator> : TestDiscriminatorBuilder<TDiscriminator>
        {
            public GenericTestDiscriminatorBuilder(DiscriminatorBuilder<TDiscriminator> discriminatorBuilder)
            {
                DiscriminatorBuilder = discriminatorBuilder;
            }

            protected DiscriminatorBuilder<TDiscriminator> DiscriminatorBuilder { get; }

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

        protected class GenericTestOwnedEntityTypeBuilder<TEntity> : TestOwnedEntityTypeBuilder<TEntity>,
            IInfrastructure<OwnedEntityTypeBuilder<TEntity>>
            where TEntity : class
        {
            public GenericTestOwnedEntityTypeBuilder(OwnedEntityTypeBuilder<TEntity> ownedEntityTypeBuilder)
            {
                OwnedEntityTypeBuilder = ownedEntityTypeBuilder;
            }

            protected OwnedEntityTypeBuilder<TEntity> OwnedEntityTypeBuilder { get; }

            public OwnedEntityTypeBuilder<TEntity> Instance
                => OwnedEntityTypeBuilder;
        }

        protected class GenericTestPropertyBuilder<TProperty> : TestPropertyBuilder<TProperty>, IInfrastructure<PropertyBuilder<TProperty>>
        {
            public GenericTestPropertyBuilder(PropertyBuilder<TProperty> propertyBuilder)
            {
                PropertyBuilder = propertyBuilder;
            }

            private PropertyBuilder<TProperty> PropertyBuilder { get; }

            public override IMutableProperty Metadata
                => PropertyBuilder.Metadata;

            public override TestPropertyBuilder<TProperty> HasAnnotation(string annotation, object value)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasAnnotation(annotation, value));

            public override TestPropertyBuilder<TProperty> IsRequired(bool isRequired = true)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.IsRequired(isRequired));

            public override TestPropertyBuilder<TProperty> HasMaxLength(int maxLength)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasMaxLength(maxLength));

            public override TestPropertyBuilder<TProperty> IsUnicode(bool unicode = true)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.IsUnicode(unicode));

            public override TestPropertyBuilder<TProperty> IsRowVersion()
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.IsRowVersion());

            public override TestPropertyBuilder<TProperty> IsConcurrencyToken(bool isConcurrencyToken = true)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.IsConcurrencyToken(isConcurrencyToken));

            public override TestPropertyBuilder<TProperty> ValueGeneratedNever()
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.ValueGeneratedNever());

            public override TestPropertyBuilder<TProperty> ValueGeneratedOnAdd()
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.ValueGeneratedOnAdd());

            public override TestPropertyBuilder<TProperty> ValueGeneratedOnAddOrUpdate()
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.ValueGeneratedOnAddOrUpdate());

            public override TestPropertyBuilder<TProperty> ValueGeneratedOnUpdate()
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.ValueGeneratedOnUpdate());

            public override TestPropertyBuilder<TProperty> HasValueGenerator<TGenerator>()
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasValueGenerator<TGenerator>());

            public override TestPropertyBuilder<TProperty> HasValueGenerator(Type valueGeneratorType)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasValueGenerator(valueGeneratorType));

            public override TestPropertyBuilder<TProperty> HasValueGenerator(Func<IProperty, IEntityType, ValueGenerator> factory)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasValueGenerator(factory));

            public override TestPropertyBuilder<TProperty> HasField(string fieldName)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasField(fieldName));

            public override TestPropertyBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.UsePropertyAccessMode(propertyAccessMode));

            public override TestPropertyBuilder<TProperty> HasConversion<TProvider>()
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasConversion<TProvider>());

            public override TestPropertyBuilder<TProperty> HasConversion(Type providerClrType)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasConversion(providerClrType));

            public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(
                Expression<Func<TProperty, TProvider>> convertToProviderExpression,
                Expression<Func<TProvider, TProperty>> convertFromProviderExpression)
                => new GenericTestPropertyBuilder<TProperty>(
                    PropertyBuilder.HasConversion(
                        convertToProviderExpression,
                        convertFromProviderExpression));

            public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(ValueConverter<TProperty, TProvider> converter)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasConversion(converter));

            public override TestPropertyBuilder<TProperty> HasConversion(ValueConverter converter)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasConversion(converter));

            PropertyBuilder<TProperty> IInfrastructure<PropertyBuilder<TProperty>>.Instance
                => PropertyBuilder;
        }

        protected class GenericTestKeyBuilder<TEntity> : TestKeyBuilder<TEntity>, IInfrastructure<KeyBuilder<TEntity>>
        {
            public GenericTestKeyBuilder(KeyBuilder<TEntity> keyBuilder)
            {
                KeyBuilder = keyBuilder;
            }

            private KeyBuilder<TEntity> KeyBuilder { get; }

            public override IMutableKey Metadata
                => KeyBuilder.Metadata;

            public override TestKeyBuilder<TEntity> HasAnnotation(string annotation, object value)
                => new GenericTestKeyBuilder<TEntity>(KeyBuilder.HasAnnotation(annotation, value));

            KeyBuilder<TEntity> IInfrastructure<KeyBuilder<TEntity>>.Instance
                => KeyBuilder;
        }

        public class GenericTestIndexBuilder<TEntity> : TestIndexBuilder<TEntity>, IInfrastructure<IndexBuilder<TEntity>>
        {
            public GenericTestIndexBuilder(IndexBuilder<TEntity> indexBuilder)
            {
                IndexBuilder = indexBuilder;
            }

            private IndexBuilder<TEntity> IndexBuilder { get; }

            public override IMutableIndex Metadata
                => IndexBuilder.Metadata;

            public override TestIndexBuilder<TEntity> HasAnnotation(string annotation, object value)
                => new GenericTestIndexBuilder<TEntity>(IndexBuilder.HasAnnotation(annotation, value));

            public override TestIndexBuilder<TEntity> IsUnique(bool isUnique = true)
                => new GenericTestIndexBuilder<TEntity>(IndexBuilder.IsUnique(isUnique));

            IndexBuilder<TEntity> IInfrastructure<IndexBuilder<TEntity>>.Instance
                => IndexBuilder;
        }

        protected class GenericTestNavigationBuilder<TSource, TTarget> : TestNavigationBuilder
            where TSource : class
            where TTarget : class
        {
            public GenericTestNavigationBuilder(NavigationBuilder<TSource, TTarget> navigationBuilder)
            {
                NavigationBuilder = navigationBuilder;
            }

            private NavigationBuilder<TSource, TTarget> NavigationBuilder { get; }

            public override TestNavigationBuilder HasAnnotation(string annotation, object value)
                => new GenericTestNavigationBuilder<TSource, TTarget>(NavigationBuilder.HasAnnotation(annotation, value));

            public override TestNavigationBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
                => new GenericTestNavigationBuilder<TSource, TTarget>(NavigationBuilder.UsePropertyAccessMode(propertyAccessMode));

            public override TestNavigationBuilder HasField(string fieldName)
                => new GenericTestNavigationBuilder<TSource, TTarget>(NavigationBuilder.HasField(fieldName));

            public override TestNavigationBuilder AutoInclude(bool autoInclude = true)
                => new GenericTestNavigationBuilder<TSource, TTarget>(NavigationBuilder.AutoInclude(autoInclude));

            public override TestNavigationBuilder IsRequired(bool required = true)
                => new GenericTestNavigationBuilder<TSource, TTarget>(NavigationBuilder.IsRequired(required));
        }

        protected class
            GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity> : TestReferenceNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTestReferenceNavigationBuilder(ReferenceNavigationBuilder<TEntity, TRelatedEntity> referenceNavigationBuilder)
            {
                ReferenceNavigationBuilder = referenceNavigationBuilder;
            }

            protected ReferenceNavigationBuilder<TEntity, TRelatedEntity> ReferenceNavigationBuilder { get; }

            public override TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(string navigationName)
                => new GenericTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(
                    ReferenceNavigationBuilder.WithMany(navigationName));

            public override TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(
                Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> navigationExpression = null)
                => new GenericTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(
                    ReferenceNavigationBuilder.WithMany(navigationExpression));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(string navigationName)
                => new GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                    ReferenceNavigationBuilder.WithOne(navigationName));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null)
                => new GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                    ReferenceNavigationBuilder.WithOne(navigationExpression));
        }

        protected class GenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity> :
            TestCollectionNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTestCollectionNavigationBuilder(CollectionNavigationBuilder<TEntity, TRelatedEntity> collectionNavigationBuilder)
            {
                CollectionNavigationBuilder = collectionNavigationBuilder;
            }

            protected CollectionNavigationBuilder<TEntity, TRelatedEntity> CollectionNavigationBuilder { get; }

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(string navigationName)
                => new GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                    CollectionNavigationBuilder.WithOne(navigationName));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null)
                => new GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                    CollectionNavigationBuilder.WithOne(navigationExpression));

            public override TestCollectionCollectionBuilder<TRelatedEntity, TEntity> WithMany(
                string navigationName)
                => new GenericTestCollectionCollectionBuilder<TRelatedEntity, TEntity>(
                    CollectionNavigationBuilder.WithMany(navigationName));

            public override TestCollectionCollectionBuilder<TRelatedEntity, TEntity> WithMany(
                Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> navigationExpression)
                => new GenericTestCollectionCollectionBuilder<TRelatedEntity, TEntity>(
                    CollectionNavigationBuilder.WithMany(navigationExpression));
        }

        protected class GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>
            : TestReferenceCollectionBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTestReferenceCollectionBuilder(ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder)
            {
                ReferenceCollectionBuilder = referenceCollectionBuilder;
            }

            public ReferenceCollectionBuilder<TEntity, TRelatedEntity> ReferenceCollectionBuilder { get; }

            public override IMutableForeignKey Metadata
                => ReferenceCollectionBuilder.Metadata;

            protected virtual GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity> Wrap(
                ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder)
                => new GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(referenceCollectionBuilder);

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(
                Expression<Func<TRelatedEntity, object>> foreignKeyExpression)
                => Wrap(ReferenceCollectionBuilder.HasForeignKey(foreignKeyExpression));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
                Expression<Func<TEntity, object>> keyExpression)
                => Wrap(ReferenceCollectionBuilder.HasPrincipalKey(keyExpression));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(params string[] foreignKeyPropertyNames)
                => Wrap(ReferenceCollectionBuilder.HasForeignKey(foreignKeyPropertyNames));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(params string[] keyPropertyNames)
                => Wrap(ReferenceCollectionBuilder.HasPrincipalKey(keyPropertyNames));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasAnnotation(string annotation, object value)
                => Wrap(ReferenceCollectionBuilder.HasAnnotation(annotation, value));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> IsRequired(bool isRequired = true)
                => Wrap(ReferenceCollectionBuilder.IsRequired(isRequired));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior)
                => Wrap(ReferenceCollectionBuilder.OnDelete(deleteBehavior));
        }

        protected class GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> :
            TestReferenceReferenceBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTestReferenceReferenceBuilder(ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder)
            {
                ReferenceReferenceBuilder = referenceReferenceBuilder;
            }

            protected ReferenceReferenceBuilder<TEntity, TRelatedEntity> ReferenceReferenceBuilder { get; }

            public override IMutableForeignKey Metadata
                => ReferenceReferenceBuilder.Metadata;

            protected virtual GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(
                ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder)
                => new GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder);

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasAnnotation(string annotation, object value)
                => Wrap(ReferenceReferenceBuilder.HasAnnotation(annotation, value));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
                Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                => Wrap(ReferenceReferenceBuilder.HasForeignKey(foreignKeyExpression));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                Expression<Func<TPrincipalEntity, object>> keyExpression)
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

        protected class GenericTestCollectionCollectionBuilder<TLeftEntity, TRightEntity> :
            TestCollectionCollectionBuilder<TLeftEntity, TRightEntity>
            where TLeftEntity : class
            where TRightEntity : class
        {
            public GenericTestCollectionCollectionBuilder(
                CollectionCollectionBuilder<TLeftEntity, TRightEntity> collectionCollectionBuilder)
            {
                CollectionCollectionBuilder = collectionCollectionBuilder;
            }

            protected CollectionCollectionBuilder<TLeftEntity, TRightEntity> CollectionCollectionBuilder { get; }

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

            public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
                Func<TestEntityTypeBuilder<TJoinEntity>,
                    TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
                Func<TestEntityTypeBuilder<TJoinEntity>,
                    TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
                Action<TestEntityTypeBuilder<TJoinEntity>> configureJoin)
                where TJoinEntity : class
                => new GenericTestEntityTypeBuilder<TRightEntity>(
                    CollectionCollectionBuilder.UsingEntity<TJoinEntity>(
                        l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                            new GenericTestEntityTypeBuilder<TJoinEntity>(l))).ReferenceCollectionBuilder,
                        r => ((GenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                            new GenericTestEntityTypeBuilder<TJoinEntity>(r))).ReferenceCollectionBuilder,
                        e => configureJoin(new GenericTestEntityTypeBuilder<TJoinEntity>(e))));

            public override TestEntityTypeBuilder<TRightEntity> UsingEntity<TJoinEntity>(
                string joinEntityName,
                Func<TestEntityTypeBuilder<TJoinEntity>,
                    TestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>> configureRight,
                Func<TestEntityTypeBuilder<TJoinEntity>,
                    TestReferenceCollectionBuilder<TRightEntity, TJoinEntity>> configureLeft,
                Action<TestEntityTypeBuilder<TJoinEntity>> configureJoin)
                where TJoinEntity : class
                => new GenericTestEntityTypeBuilder<TRightEntity>(
                    CollectionCollectionBuilder.UsingEntity<TJoinEntity>(
                        joinEntityName,
                        l => ((GenericTestReferenceCollectionBuilder<TLeftEntity, TJoinEntity>)configureRight(
                            new GenericTestEntityTypeBuilder<TJoinEntity>(l))).ReferenceCollectionBuilder,
                        r => ((GenericTestReferenceCollectionBuilder<TRightEntity, TJoinEntity>)configureLeft(
                            new GenericTestEntityTypeBuilder<TJoinEntity>(r))).ReferenceCollectionBuilder,
                        e => configureJoin(new GenericTestEntityTypeBuilder<TJoinEntity>(e))));
        }

        protected class GenericTestOwnershipBuilder<TEntity, TDependentEntity>
            : TestOwnershipBuilder<TEntity, TDependentEntity>, IInfrastructure<OwnershipBuilder<TEntity, TDependentEntity>>
            where TEntity : class
            where TDependentEntity : class
        {
            public GenericTestOwnershipBuilder(OwnershipBuilder<TEntity, TDependentEntity> ownershipBuilder)
            {
                OwnershipBuilder = ownershipBuilder;
            }

            protected OwnershipBuilder<TEntity, TDependentEntity> OwnershipBuilder { get; }

            public override IMutableForeignKey Metadata
                => OwnershipBuilder.Metadata;

            protected virtual GenericTestOwnershipBuilder<TNewEntity, TNewDependentEntity> Wrap<TNewEntity, TNewDependentEntity>(
                OwnershipBuilder<TNewEntity, TNewDependentEntity> ownershipBuilder)
                where TNewEntity : class
                where TNewDependentEntity : class
                => new GenericTestOwnershipBuilder<TNewEntity, TNewDependentEntity>(ownershipBuilder);

            public override TestOwnershipBuilder<TEntity, TDependentEntity> HasAnnotation(string annotation, object value)
                => Wrap(OwnershipBuilder.HasAnnotation(annotation, value));

            public override TestOwnershipBuilder<TEntity, TDependentEntity> HasForeignKey(
                params string[] foreignKeyPropertyNames)
                => Wrap(OwnershipBuilder.HasForeignKey(foreignKeyPropertyNames));

            public override TestOwnershipBuilder<TEntity, TDependentEntity> HasForeignKey(
                Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                => Wrap(OwnershipBuilder.HasForeignKey(foreignKeyExpression));

            public override TestOwnershipBuilder<TEntity, TDependentEntity> HasPrincipalKey(
                params string[] keyPropertyNames)
                => Wrap(OwnershipBuilder.HasPrincipalKey(keyPropertyNames));

            public override TestOwnershipBuilder<TEntity, TDependentEntity> HasPrincipalKey(
                Expression<Func<TEntity, object>> keyExpression)
                => Wrap(OwnershipBuilder.HasPrincipalKey(keyExpression));

            OwnershipBuilder<TEntity, TDependentEntity> IInfrastructure<OwnershipBuilder<TEntity, TDependentEntity>>.Instance
                => OwnershipBuilder;
        }

        protected class GenericTestOwnedNavigationBuilder<TEntity, TDependentEntity>
            : TestOwnedNavigationBuilder<TEntity, TDependentEntity>,
                IInfrastructure<OwnedNavigationBuilder<TEntity, TDependentEntity>>
            where TEntity : class
            where TDependentEntity : class
        {
            public GenericTestOwnedNavigationBuilder(OwnedNavigationBuilder<TEntity, TDependentEntity> ownedNavigationBuilder)
            {
                OwnedNavigationBuilder = ownedNavigationBuilder;
            }

            protected OwnedNavigationBuilder<TEntity, TDependentEntity> OwnedNavigationBuilder { get; }

            public override IMutableForeignKey Metadata
                => OwnedNavigationBuilder.Metadata;

            public override IMutableEntityType OwnedEntityType
                => OwnedNavigationBuilder.OwnedEntityType;

            protected virtual GenericTestOwnedNavigationBuilder<TNewEntity, TNewDependentEntity> Wrap<TNewEntity, TNewDependentEntity>(
                OwnedNavigationBuilder<TNewEntity, TNewDependentEntity> ownershipBuilder)
                where TNewEntity : class
                where TNewDependentEntity : class
                => new GenericTestOwnedNavigationBuilder<TNewEntity, TNewDependentEntity>(ownershipBuilder);

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> HasAnnotation(
                string annotation,
                object value)
                => Wrap(OwnedNavigationBuilder.HasAnnotation(annotation, value));

            public override TestKeyBuilder<TDependentEntity> HasKey(Expression<Func<TDependentEntity, object>> keyExpression)
                => new GenericTestKeyBuilder<TDependentEntity>(OwnedNavigationBuilder.HasKey(keyExpression));

            public override TestKeyBuilder<TDependentEntity> HasKey(params string[] propertyNames)
                => new GenericTestKeyBuilder<TDependentEntity>(OwnedNavigationBuilder.HasKey(propertyNames));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
                => new GenericTestPropertyBuilder<TProperty>(OwnedNavigationBuilder.Property<TProperty>(propertyName));

            public override TestPropertyBuilder<TProperty> IndexerProperty<TProperty>(string propertyName)
                => new GenericTestPropertyBuilder<TProperty>(OwnedNavigationBuilder.IndexerProperty<TProperty>(propertyName));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(
                Expression<Func<TDependentEntity, TProperty>> propertyExpression)
                => new GenericTestPropertyBuilder<TProperty>(OwnedNavigationBuilder.Property(propertyExpression));

            public override TestNavigationBuilder Navigation<TNavigation>(
                Expression<Func<TDependentEntity, TNavigation>> navigationExpression)
                where TNavigation : class
                => new GenericTestNavigationBuilder<TDependentEntity, TNavigation>(OwnedNavigationBuilder.Navigation(navigationExpression));

            public override TestNavigationBuilder Navigation<TNavigation>(
                Expression<Func<TDependentEntity, IEnumerable<TNavigation>>> navigationExpression)
                where TNavigation : class
                => new GenericTestNavigationBuilder<TDependentEntity, TNavigation>(OwnedNavigationBuilder.Navigation(navigationExpression));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> Ignore(string propertyName)
                => Wrap(OwnedNavigationBuilder.Ignore(propertyName));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> Ignore(
                Expression<Func<TDependentEntity, object>> propertyExpression)
                => Wrap(OwnedNavigationBuilder.Ignore(propertyExpression));

            public override TestIndexBuilder<TEntity> HasIndex(params string[] propertyNames)
                => new GenericTestIndexBuilder<TEntity>(OwnedNavigationBuilder.HasIndex(propertyNames));

            public override TestIndexBuilder<TEntity> HasIndex(Expression<Func<TDependentEntity, object>> indexExpression)
                => new GenericTestIndexBuilder<TEntity>(OwnedNavigationBuilder.HasIndex(indexExpression));

            public override TestOwnershipBuilder<TEntity, TDependentEntity> WithOwner(string ownerReference)
                => new GenericTestOwnershipBuilder<TEntity, TDependentEntity>(
                    OwnedNavigationBuilder.WithOwner(ownerReference));

            public override TestOwnershipBuilder<TEntity, TDependentEntity> WithOwner(
                Expression<Func<TDependentEntity, TEntity>> referenceExpression = null)
                => new GenericTestOwnershipBuilder<TEntity, TDependentEntity>(
                    OwnedNavigationBuilder.WithOwner(referenceExpression));

            public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
                Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression)
                => Wrap(OwnedNavigationBuilder.OwnsOne(navigationExpression));

            public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
                string entityTypeName,
                Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression)
                => Wrap(OwnedNavigationBuilder.OwnsOne(entityTypeName, navigationExpression));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
                Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
                => Wrap(OwnedNavigationBuilder.OwnsOne(navigationExpression, r => buildAction(Wrap(r))));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
                string entityTypeName,
                Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
                => Wrap(OwnedNavigationBuilder.OwnsOne(entityTypeName, navigationExpression, r => buildAction(Wrap(r))));

            public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
                Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression)
                => Wrap(OwnedNavigationBuilder.OwnsMany(navigationExpression));

            public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
                string entityTypeName,
                Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression)
                => Wrap(OwnedNavigationBuilder.OwnsMany(entityTypeName, navigationExpression));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
                Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
                => Wrap(OwnedNavigationBuilder.OwnsMany(navigationExpression, r => buildAction(Wrap(r))));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
                string entityTypeName,
                Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
                => Wrap(OwnedNavigationBuilder.OwnsMany(entityTypeName, navigationExpression, r => buildAction(Wrap(r))));

            public override TestReferenceNavigationBuilder<TDependentEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TDependentEntity, TRelatedEntity>> navigationExpression = null)
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

            OwnedNavigationBuilder<TEntity, TDependentEntity> IInfrastructure<OwnedNavigationBuilder<TEntity, TDependentEntity>>.
                Instance
                => OwnedNavigationBuilder;
        }
    }
}
