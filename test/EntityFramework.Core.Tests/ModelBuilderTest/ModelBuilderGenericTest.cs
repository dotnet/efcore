// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Microsoft.Data.Entity.Metadata.Internal;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Tests
{
    public class ModelBuilderGenericTest : ModelBuilderTest
    {
        [Fact]
        public void Can_create_a_model_builder_with_given_conventions_and_model()
        {
            var convention = new TestConvention();
            var conventions = new ConventionSet();
            conventions.EntityTypeAddedConventions.Add(convention);

            var model = new Model();
            var modelBuilder = new ModelBuilder(conventions, model);

            Assert.Same(model, modelBuilder.Model);

            modelBuilder.Entity<Random>();

            Assert.True(convention.Applied);
            Assert.NotNull(model.GetEntityType(typeof(Random)));
        }

        [Fact]
        public void Can_create_a_model_builder_with_given_conventions_only()
        {
            var convention = new TestConvention();
            var conventions = new ConventionSet();
            conventions.EntityTypeAddedConventions.Add(convention);

            var modelBuilder = new ModelBuilder(conventions);

            modelBuilder.Entity<Random>();

            Assert.True(convention.Applied);
            Assert.NotNull(modelBuilder.Model.GetEntityType(typeof(Random)));
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

        public class GenericNonRelationship : NonRelationshipTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
            {
                return new GenericTestModelBuilder(modelBuilder);
            }
        }

        public class GenericOneToMany : OneToManyTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
            {
                return new GenericTestModelBuilder(modelBuilder);
            }
        }

        public class GenericManyToOne : ManyToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
            {
                return new GenericTestModelBuilder(modelBuilder);
            }
        }

        public class GenericOneToOne : OneToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
            {
                return new GenericTestModelBuilder(modelBuilder);
            }
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
            public override EntityType Metadata => EntityTypeBuilder.Metadata;

            protected virtual TestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder<TEntity> entityTypeBuilder)
                => new GenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder);

            public override TestEntityTypeBuilder<TEntity> Annotation(string annotation, object value)
                => Wrap(EntityTypeBuilder.Annotation(annotation, value));

            public override TestKeyBuilder Key(Expression<Func<TEntity, object>> keyExpression)
                => new TestKeyBuilder(EntityTypeBuilder.Key(keyExpression));

            public override TestKeyBuilder Key(params string[] propertyNames)
                => new TestKeyBuilder(EntityTypeBuilder.Key(propertyNames));

            public override TestKeyBuilder AlternateKey(Expression<Func<TEntity, object>> keyExpression)
                => new TestKeyBuilder(EntityTypeBuilder.AlternateKey(keyExpression));

            public override TestKeyBuilder AlternateKey(params string[] propertyNames)
                => new TestKeyBuilder(EntityTypeBuilder.AlternateKey(propertyNames));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
                => new GenericTestPropertyBuilder<TProperty>(EntityTypeBuilder.Property(propertyExpression));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
                => new GenericTestPropertyBuilder<TProperty>(EntityTypeBuilder.Property<TProperty>(propertyName));

            public override TestEntityTypeBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> propertyExpression)
                => Wrap(EntityTypeBuilder.Ignore(propertyExpression));

            public override TestEntityTypeBuilder<TEntity> Ignore(string propertyName)
                => Wrap(EntityTypeBuilder.Ignore(propertyName));

            public override TestIndexBuilder Index(Expression<Func<TEntity, object>> indexExpression)
                => new TestIndexBuilder(EntityTypeBuilder.Index(indexExpression));

            public override TestIndexBuilder Index(params string[] propertyNames)
                => new TestIndexBuilder(EntityTypeBuilder.Index(propertyNames));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> Reference<TRelatedEntity>(Expression<Func<TEntity, TRelatedEntity>> reference = null)
                => new GenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.Reference(reference));

            public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> Collection<TRelatedEntity>(Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collection = null)
                => new GenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.Collection(collection));
        }

        protected class GenericTestPropertyBuilder<TProperty> : TestPropertyBuilder<TProperty>
        {
            public GenericTestPropertyBuilder(PropertyBuilder<TProperty> propertyBuilder)
            {
                PropertyBuilder = propertyBuilder;
            }

            private PropertyBuilder<TProperty> PropertyBuilder { get; }

            public override Property Metadata => PropertyBuilder.Metadata;

            public override TestPropertyBuilder<TProperty> Annotation(string annotation, object value)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.Annotation(annotation, value));

            public override TestPropertyBuilder<TProperty> Required(bool isRequired = true)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.Required(isRequired));

            public override TestPropertyBuilder<TProperty> MaxLength(int maxLength)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.MaxLength(maxLength));

            public override TestPropertyBuilder<TProperty> ConcurrencyToken(bool isConcurrencyToken = true)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.ConcurrencyToken(isConcurrencyToken));

            public override TestPropertyBuilder<TProperty> StoreGeneratedPattern(StoreGeneratedPattern storeGeneratedPattern)
                => new GenericTestPropertyBuilder<TProperty>(PropertyBuilder.StoreGeneratedPattern(storeGeneratedPattern));
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

            public override TestReferenceCollectionBuilder<TRelatedEntity, TEntity> InverseCollection(Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> collection = null)
                => new GenericTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(ReferenceNavigationBuilder.InverseCollection(collection));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> InverseReference(Expression<Func<TRelatedEntity, TEntity>> reference = null)
                => new GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(ReferenceNavigationBuilder.InverseReference(reference));
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

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> InverseReference(Expression<Func<TRelatedEntity, TEntity>> reference = null)
                => new GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(CollectionNavigationBuilder.InverseReference(reference));
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

            public override ForeignKey Metadata => ReferenceCollectionBuilder.Metadata;

            protected virtual GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity> Wrap(ReferenceCollectionBuilder<TEntity, TRelatedEntity> referenceCollectionBuilder)
                => new GenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(referenceCollectionBuilder);

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> ForeignKey(Expression<Func<TRelatedEntity, object>> foreignKeyExpression)
                => Wrap(ReferenceCollectionBuilder.ForeignKey(foreignKeyExpression));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> PrincipalKey(Expression<Func<TEntity, object>> keyExpression)
                => Wrap(ReferenceCollectionBuilder.PrincipalKey(keyExpression));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> ForeignKey(params string[] foreignKeyPropertyNames)
                => Wrap(ReferenceCollectionBuilder.ForeignKey(foreignKeyPropertyNames));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> PrincipalKey(params string[] keyPropertyNames)
                => Wrap(ReferenceCollectionBuilder.PrincipalKey(keyPropertyNames));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> Annotation(string annotation, object value)
                => Wrap(ReferenceCollectionBuilder.Annotation(annotation, value));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> Required(bool isRequired = true)
                => Wrap(ReferenceCollectionBuilder.Required(isRequired));
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

            public override ForeignKey Metadata => ReferenceReferenceBuilder.Metadata;

            protected virtual GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(ReferenceReferenceBuilder<TEntity, TRelatedEntity> referenceReferenceBuilder)
                => new GenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder);

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> Annotation(string annotation, object value)
                => Wrap(ReferenceReferenceBuilder.Annotation(annotation, value));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> ForeignKey<TDependentEntity>(Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                => Wrap(ReferenceReferenceBuilder.ForeignKey(foreignKeyExpression));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> PrincipalKey<TPrincipalEntity>(Expression<Func<TPrincipalEntity, object>> keyExpression)
                => Wrap(ReferenceReferenceBuilder.PrincipalKey(keyExpression));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> ForeignKey(Type dependentEntityType, params string[] foreignKeyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.ForeignKey(dependentEntityType, foreignKeyPropertyNames));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> PrincipalKey(Type principalEntityType, params string[] keyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.PrincipalKey(principalEntityType, keyPropertyNames));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> Required(bool isRequired = true)
                => Wrap(ReferenceReferenceBuilder.Required(isRequired));
        }
    }
}
