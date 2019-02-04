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
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
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
        [Fact]
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

            public InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
            {
                Applied = true;

                return entityTypeBuilder;
            }
        }

        [Fact]
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

            [Fact]
            public virtual void Changing_propertyInfo_updates_Property()
            {
                var modelBuilder = CreateModelBuilder();

                modelBuilder.Entity<DoubleProperty>().Property(e => ((IReplacable)e).Property);

                modelBuilder.Validate();

                var property = modelBuilder.Model.FindEntityType(typeof(DoubleProperty)).GetProperty("Property");
                Assert.True(
                    property.GetIdentifyingMemberInfo().Name
                        .EndsWith(typeof(IReplacable).Name + "." + nameof(IReplacable.Property)));
            }

            [Fact]
            public virtual void Can_add_ignore_explicit_interface_implementation_property()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<EntityBase>().Ignore(e => ((IEntityBase)e).Target);

                Assert.Empty(modelBuilder.Model.FindEntityType(typeof(EntityBase)).GetProperties());

                modelBuilder.Entity<EntityBase>().Property(e => ((IEntityBase)e).Target);

                Assert.Equal(1, modelBuilder.Model.FindEntityType(typeof(EntityBase)).GetProperties().Count());
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

        public class GenericQueryTypes : QueryTypesTestBase
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

            public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
            {
                ModelBuilder.Entity<TEntity>(
                    entityTypeBuilder =>
                        buildAction(new GenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
                return this;
            }

            public override TestOwnedEntityTypeBuilder<TEntity> Owned<TEntity>()
                => new GenericTestOwnedEntityTypeBuilder<TEntity>(ModelBuilder.Owned<TEntity>());

            public override TestQueryTypeBuilder<TQuery> Query<TQuery>()
                => new GenericTestQueryTypeBuilder<TQuery>(ModelBuilder.Query<TQuery>());

            public override TestModelBuilder Query<TQuery>(Action<TestQueryTypeBuilder<TQuery>> buildAction)
            {
                ModelBuilder.Query<TQuery>(queryTypeBuilder => buildAction(new GenericTestQueryTypeBuilder<TQuery>(queryTypeBuilder)));
                return this;
            }

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
            public override IMutableEntityType Metadata => EntityTypeBuilder.Metadata;

            protected virtual TestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder<TEntity> entityTypeBuilder)
                => new GenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder);

            public override TestEntityTypeBuilder<TEntity> HasAnnotation(string annotation, object value)
                => Wrap(EntityTypeBuilder.HasAnnotation(annotation, value));

            public override TestEntityTypeBuilder<TEntity> HasBaseType<TBaseEntity>()
                => Wrap(EntityTypeBuilder.HasBaseType<TBaseEntity>());

            public override TestEntityTypeBuilder<TEntity> HasBaseType(string baseEntityTypeName)
                => Wrap(EntityTypeBuilder.HasBaseType(baseEntityTypeName));

            public override TestKeyBuilder HasKey(Expression<Func<TEntity, object>> keyExpression)
                => new TestKeyBuilder(EntityTypeBuilder.HasKey(keyExpression));

            public override TestKeyBuilder HasKey(params string[] propertyNames)
                => new TestKeyBuilder(EntityTypeBuilder.HasKey(propertyNames));

            public override TestKeyBuilder HasAlternateKey(Expression<Func<TEntity, object>> keyExpression)
                => new TestKeyBuilder(EntityTypeBuilder.HasAlternateKey(keyExpression));

            public override TestKeyBuilder HasAlternateKey(params string[] propertyNames)
                => new TestKeyBuilder(EntityTypeBuilder.HasAlternateKey(propertyNames));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
                => new GenericTestPropertyBuilder<TProperty>(EntityTypeBuilder.Property(propertyExpression));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
                => new GenericTestPropertyBuilder<TProperty>(EntityTypeBuilder.Property<TProperty>(propertyName));

            public override TestEntityTypeBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> propertyExpression)
                => Wrap(EntityTypeBuilder.Ignore(propertyExpression));

            public override TestEntityTypeBuilder<TEntity> Ignore(string propertyName)
                => Wrap(EntityTypeBuilder.Ignore(propertyName));

            public override TestIndexBuilder HasIndex(Expression<Func<TEntity, object>> indexExpression)
                => new TestIndexBuilder(EntityTypeBuilder.HasIndex(indexExpression));

            public override TestIndexBuilder HasIndex(params string[] propertyNames)
                => new TestIndexBuilder(EntityTypeBuilder.HasIndex(propertyNames));

            public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression)
                => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.OwnsOne(navigationExpression));

            public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(
                    EntityTypeBuilder.OwnsOne(
                        navigationExpression,
                        r => buildAction(new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

            public override TestOwnedNavigationBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression)
                => new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.OwnsMany(navigationExpression));

            public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(
                    EntityTypeBuilder.OwnsMany(
                        navigationExpression,
                        r => buildAction(new GenericTestOwnedNavigationBuilder<TEntity, TRelatedEntity>(r))));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression = null)
                => new GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasOne(navigationExpression));

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

            public EntityTypeBuilder<TEntity> Instance => EntityTypeBuilder;
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

            public OwnedEntityTypeBuilder<TEntity> Instance => OwnedEntityTypeBuilder;
        }

        protected class GenericTestQueryTypeBuilder<TQuery> : TestQueryTypeBuilder<TQuery>, IInfrastructure<QueryTypeBuilder<TQuery>>
            where TQuery : class
        {
            public GenericTestQueryTypeBuilder(QueryTypeBuilder<TQuery> queryTypeBuilder)
            {
                QueryTypeBuilder = queryTypeBuilder;
            }

            protected QueryTypeBuilder<TQuery> QueryTypeBuilder { get; }
            public override IMutableEntityType Metadata => QueryTypeBuilder.Metadata;

            protected virtual TestQueryTypeBuilder<TQuery> Wrap(QueryTypeBuilder<TQuery> entityTypeBuilder)
                => new GenericTestQueryTypeBuilder<TQuery>(entityTypeBuilder);

            public override TestQueryTypeBuilder<TQuery> HasAnnotation(string annotation, object value)
                => Wrap(QueryTypeBuilder.HasAnnotation(annotation, value));

            public override TestQueryTypeBuilder<TQuery> HasBaseType<TBaseEntity>()
                => Wrap(QueryTypeBuilder.HasBaseType<TBaseEntity>());

            public override TestQueryTypeBuilder<TQuery> HasBaseType(string baseEntityTypeName)
                => Wrap(QueryTypeBuilder.HasBaseType(baseEntityTypeName));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TQuery, TProperty>> propertyExpression)
                => new GenericTestPropertyBuilder<TProperty>(QueryTypeBuilder.Property(propertyExpression));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
                => new GenericTestPropertyBuilder<TProperty>(QueryTypeBuilder.Property<TProperty>(propertyName));

            public override TestQueryTypeBuilder<TQuery> Ignore(Expression<Func<TQuery, object>> propertyExpression)
                => Wrap(QueryTypeBuilder.Ignore(propertyExpression));

            public override TestQueryTypeBuilder<TQuery> Ignore(string propertyName)
                => Wrap(QueryTypeBuilder.Ignore(propertyName));

            public override TestReferenceNavigationBuilder<TQuery, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TQuery, TRelatedEntity>> navigationExpression = null)
                => new GenericTestReferenceNavigationBuilder<TQuery, TRelatedEntity>(QueryTypeBuilder.HasOne(navigationExpression));

            public override TestQueryTypeBuilder<TQuery> HasQueryFilter(Expression<Func<TQuery, bool>> filter)
                => Wrap(QueryTypeBuilder.HasQueryFilter(filter));

            public virtual TestQueryTypeBuilder<TQuery> ToQuery(Expression<Func<IQueryable<TQuery>>> query)
                => Wrap(QueryTypeBuilder.ToQuery(query));

            public override TestQueryTypeBuilder<TQuery> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
                => Wrap(QueryTypeBuilder.UsePropertyAccessMode(propertyAccessMode));

            public QueryTypeBuilder<TQuery> Instance => QueryTypeBuilder;
        }

        protected class GenericTestPropertyBuilder<TProperty> : TestPropertyBuilder<TProperty>, IInfrastructure<PropertyBuilder<TProperty>>
        {
            public GenericTestPropertyBuilder(PropertyBuilder<TProperty> propertyBuilder)
            {
                PropertyBuilder = propertyBuilder;
            }

            private PropertyBuilder<TProperty> PropertyBuilder { get; }

            public override IMutableProperty Metadata => PropertyBuilder.Metadata;

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

            PropertyBuilder<TProperty> IInfrastructure<PropertyBuilder<TProperty>>.Instance => PropertyBuilder;
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

            public override TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(
                Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> navigationExpression = null)
                => new GenericTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(
                    ReferenceNavigationBuilder.WithMany(navigationExpression));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null)
                => new GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                    ReferenceNavigationBuilder.WithOne(navigationExpression));
        }

        protected class
            GenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity> : TestCollectionNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTestCollectionNavigationBuilder(CollectionNavigationBuilder<TEntity, TRelatedEntity> collectionNavigationBuilder)
            {
                CollectionNavigationBuilder = collectionNavigationBuilder;
            }

            protected CollectionNavigationBuilder<TEntity, TRelatedEntity> CollectionNavigationBuilder { get; }

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null)
                => new GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(
                    CollectionNavigationBuilder.WithOne(navigationExpression));
        }

        protected class
            GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity> : TestReferenceCollectionBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTestReferenceCollectionBuilder(ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder)
            {
                ReferenceCollectionBuilder = referenceCollectionBuilder;
            }

            protected ReferenceCollectionBuilder<TEntity, TRelatedEntity> ReferenceCollectionBuilder { get; }

            public override IMutableForeignKey Metadata => ReferenceCollectionBuilder.Metadata;

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

            public override IMutableForeignKey Metadata => ReferenceReferenceBuilder.Metadata;

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

            public override IMutableForeignKey Metadata => OwnershipBuilder.Metadata;

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

            public override IMutableForeignKey Metadata => OwnedNavigationBuilder.Metadata;
            public override IMutableEntityType OwnedEntityType => OwnedNavigationBuilder.OwnedEntityType;

            protected virtual GenericTestOwnedNavigationBuilder<TNewEntity, TNewDependentEntity> Wrap<TNewEntity, TNewDependentEntity>(
                OwnedNavigationBuilder<TNewEntity, TNewDependentEntity> ownershipBuilder)
                where TNewEntity : class
                where TNewDependentEntity : class
                => new GenericTestOwnedNavigationBuilder<TNewEntity, TNewDependentEntity>(ownershipBuilder);

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> HasAnnotation(
                string annotation, object value)
                => Wrap(OwnedNavigationBuilder.HasAnnotation(annotation, value));

            public override TestKeyBuilder HasKey(Expression<Func<TDependentEntity, object>> keyExpression)
                => new TestKeyBuilder(OwnedNavigationBuilder.HasKey(keyExpression));

            public override TestKeyBuilder HasKey(params string[] propertyNames)
                => new TestKeyBuilder(OwnedNavigationBuilder.HasKey(propertyNames));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
                => new GenericTestPropertyBuilder<TProperty>(OwnedNavigationBuilder.Property<TProperty>(propertyName));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(
                Expression<Func<TDependentEntity, TProperty>> propertyExpression)
                => new GenericTestPropertyBuilder<TProperty>(OwnedNavigationBuilder.Property(propertyExpression));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> Ignore(string propertyName)
                => Wrap(OwnedNavigationBuilder.Ignore(propertyName));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> Ignore(
                Expression<Func<TDependentEntity, object>> propertyExpression)
                => Wrap(OwnedNavigationBuilder.Ignore(propertyExpression));

            public override TestIndexBuilder HasIndex(params string[] propertyNames)
                => new TestIndexBuilder(OwnedNavigationBuilder.HasIndex(propertyNames));

            public override TestIndexBuilder HasIndex(Expression<Func<TDependentEntity, object>> indexExpression)
                => new TestIndexBuilder(OwnedNavigationBuilder.HasIndex(indexExpression));

            public override TestOwnershipBuilder<TEntity, TDependentEntity> WithOwner(
                Expression<Func<TDependentEntity, TEntity>> referenceExpression = null)
                => new GenericTestOwnershipBuilder<TEntity, TDependentEntity>(
                    OwnedNavigationBuilder.WithOwner(referenceExpression));

            public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsOne<TNewDependentEntity>(
                Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression)
                => Wrap(OwnedNavigationBuilder.OwnsOne(navigationExpression));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsOne<TNewDependentEntity>(
                Expression<Func<TDependentEntity, TNewDependentEntity>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
                => Wrap(OwnedNavigationBuilder.OwnsOne(navigationExpression, r => buildAction(Wrap(r))));

            public override TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
                Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression)
                => Wrap(OwnedNavigationBuilder.OwnsMany(navigationExpression));

            public override TestOwnedNavigationBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
                Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression,
                Action<TestOwnedNavigationBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
                => Wrap(OwnedNavigationBuilder.OwnsMany(navigationExpression, r => buildAction(Wrap(r))));

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

            OwnedNavigationBuilder<TEntity, TDependentEntity> IInfrastructure<OwnedNavigationBuilder<TEntity, TDependentEntity>>.
                Instance
                => OwnedNavigationBuilder;
        }
    }
}
