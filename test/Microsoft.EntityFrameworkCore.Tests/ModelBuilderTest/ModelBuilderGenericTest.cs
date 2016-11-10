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
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Tests
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

        private class TestConvention : IEntityTypeConvention
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
            var modelBuilder = TestHelpers.Instance.CreateConventionBuilder();

            modelBuilder.Entity<GiantModel.RelatedEntity1>();

            Assert.Equal(2000, modelBuilder.Model.GetEntityTypes().Count());
        }

        public class GenericNonRelationship : NonRelationshipTestBase
        {
            [Fact]
            public virtual void Can_add_ignore_explicit_interface_implementation_property()
            {
                var modelBuilder = CreateModelBuilder();
                modelBuilder.Entity<EntityBase>().Ignore(e => ((IEntityBase)e).Target);

                Assert.Empty(modelBuilder.Model.FindEntityType(typeof(EntityBase)).GetProperties());

                modelBuilder.Entity<EntityBase>().Property(e => ((IEntityBase)e).Target);

                Assert.Equal(1, modelBuilder.Model.FindEntityType(typeof(EntityBase)).GetProperties().Count());
            }

            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
                => new GenericTestModelBuilder(modelBuilder);
        }

        public class GenericInheritance : InheritanceTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
                => new GenericTestModelBuilder(modelBuilder);
        }

        public class GenericOneToMany : OneToManyTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
                => new GenericTestModelBuilder(modelBuilder);
        }

        public class GenericManyToOne : ManyToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
                => new GenericTestModelBuilder(modelBuilder);
        }

        public class GenericOneToOne : OneToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
                => new GenericTestModelBuilder(modelBuilder);
        }

        protected class GenericTestModelBuilder : TestModelBuilder
        {
            public GenericTestModelBuilder(ModelBuilder modelBuilder)
                : base(modelBuilder)
            {
            }

            public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
                => new GenericTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity<TEntity>());

            public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
                => new GenericTestModelBuilder(ModelBuilder.Entity<TEntity>(entityTypeBuilder =>
                        buildAction(new GenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder))));

            public override TestModelBuilder Ignore<TEntity>()
                => new GenericTestModelBuilder(ModelBuilder.Ignore<TEntity>());
        }

        protected class GenericTestEntityTypeBuilder<TEntity> : TestEntityTypeBuilder<TEntity>
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

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(Expression<Func<TEntity, TRelatedEntity>> reference = null)
                => new GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasOne(reference));

            public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collection = null)
                => new GenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasMany(collection));

            public override TestEntityTypeBuilder<TEntity> HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy)
                => Wrap(EntityTypeBuilder.HasChangeTrackingStrategy(changeTrackingStrategy));

            public override TestEntityTypeBuilder<TEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
                => Wrap(EntityTypeBuilder.UsePropertyAccessMode(propertyAccessMode));
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

            PropertyBuilder<TProperty> IInfrastructure<PropertyBuilder<TProperty>>.Instance => PropertyBuilder;
        }

        protected class GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity> : TestReferenceNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTestReferenceNavigationBuilder(ReferenceNavigationBuilder<TEntity, TRelatedEntity> referenceNavigationBuilder)
            {
                ReferenceNavigationBuilder = referenceNavigationBuilder;
            }

            protected ReferenceNavigationBuilder<TEntity, TRelatedEntity> ReferenceNavigationBuilder { get; }

            public override TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> collection = null)
                => new GenericTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(ReferenceNavigationBuilder.WithMany(collection));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(Expression<Func<TRelatedEntity, TEntity>> reference = null)
                => new GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(ReferenceNavigationBuilder.WithOne(reference));
        }

        protected class GenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity> : TestCollectionNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTestCollectionNavigationBuilder(CollectionNavigationBuilder<TEntity, TRelatedEntity> collectionNavigationBuilder)
            {
                CollectionNavigationBuilder = collectionNavigationBuilder;
            }

            protected CollectionNavigationBuilder<TEntity, TRelatedEntity> CollectionNavigationBuilder { get; }

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(Expression<Func<TRelatedEntity, TEntity>> reference = null)
                => new GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(CollectionNavigationBuilder.WithOne(reference));
        }

        protected class GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity> : TestReferenceCollectionBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTestReferenceCollectionBuilder(ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder)
            {
                ReferenceCollectionBuilder = referenceCollectionBuilder;
            }

            protected ReferenceCollectionBuilder<TEntity, TRelatedEntity> ReferenceCollectionBuilder { get; }

            public override IMutableForeignKey Metadata => ReferenceCollectionBuilder.Metadata;

            protected virtual GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity> Wrap(ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder)
                => new GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(referenceCollectionBuilder);

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(Expression<Func<TRelatedEntity, object>> foreignKeyExpression)
                => Wrap(ReferenceCollectionBuilder.HasForeignKey(foreignKeyExpression));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(Expression<Func<TEntity, object>> keyExpression)
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

        protected class GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> : TestReferenceReferenceBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public GenericTestReferenceReferenceBuilder(ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder)
            {
                ReferenceReferenceBuilder = referenceReferenceBuilder;
            }

            protected ReferenceReferenceBuilder<TEntity, TRelatedEntity> ReferenceReferenceBuilder { get; }

            public override IMutableForeignKey Metadata => ReferenceReferenceBuilder.Metadata;

            protected virtual GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder)
                => new GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder);

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasAnnotation(string annotation, object value)
                => Wrap(ReferenceReferenceBuilder.HasAnnotation(annotation, value));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                => Wrap(ReferenceReferenceBuilder.HasForeignKey(foreignKeyExpression));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(Expression<Func<TPrincipalEntity, object>> keyExpression)
                => Wrap(ReferenceReferenceBuilder.HasPrincipalKey(keyExpression));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(params string[] foreignKeyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.HasForeignKey<TDependentEntity>(foreignKeyPropertyNames));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(params string[] keyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.HasPrincipalKey<TPrincipalEntity>(keyPropertyNames));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> IsRequired(bool isRequired = true)
                => Wrap(ReferenceReferenceBuilder.IsRequired(isRequired));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior)
                => Wrap(ReferenceReferenceBuilder.OnDelete(deleteBehavior));
        }
    }
}
