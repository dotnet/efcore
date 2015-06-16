// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Tests
{
    public class ModelBuilderNonGenericTest : ModelBuilderTest
    {
        public class NonGenericNonRelationship : NonRelationshipTestBase
        {
            [Fact]
            public void Can_set_model_annotation()
            {
                var model = new Model();
                var modelBuilder = (NonGenericTestModelBuilder)CreateModelBuilder(model);

                modelBuilder = modelBuilder.Annotation("Fus", "Ro");

                Assert.NotNull(modelBuilder);
                Assert.Equal("Ro", model.GetAnnotation("Fus").Value);
            }

            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
            {
                return new NonGenericTestModelBuilder(modelBuilder);
            }
        }

        public class NonGenericOneToMany : OneToManyTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
            {
                return new NonGenericTestModelBuilder(modelBuilder);
            }
        }

        public class NonGenericManyToOne : ManyToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
            {
                return new NonGenericTestModelBuilder(modelBuilder);
            }
        }

        public class NonGenericOneToOne : OneToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder)
            {
                return new NonGenericTestModelBuilder(modelBuilder);
            }
        }

        private class NonGenericTestModelBuilder : TestModelBuilder
        {
            public NonGenericTestModelBuilder(ModelBuilder modelBuilder)
                : base(modelBuilder)
            {
            }

            public NonGenericTestModelBuilder Annotation(string annotation, object value)
                => new NonGenericTestModelBuilder(ModelBuilder.Annotation(annotation, value));

            public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
                => new NonGenericTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity(typeof(TEntity)));

            public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
                => new NonGenericTestModelBuilder(ModelBuilder.Entity(typeof(TEntity), entityTypeBuilder =>
                    buildAction(new NonGenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder))));

            public override TestModelBuilder Ignore<TEntity>()
                => new NonGenericTestModelBuilder(ModelBuilder.Ignore(typeof(TEntity)));
        }

        protected class NonGenericTestEntityTypeBuilder<TEntity> : TestEntityTypeBuilder<TEntity>
            where TEntity : class
        {
            public NonGenericTestEntityTypeBuilder(EntityTypeBuilder entityTypeBuilder)
            {
                EntityTypeBuilder = entityTypeBuilder;
            }

            protected EntityTypeBuilder EntityTypeBuilder { get; }
            public override EntityType Metadata => EntityTypeBuilder.Metadata;

            protected virtual NonGenericTestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder entityTypeBuilder)
                => new NonGenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder);

            public override TestEntityTypeBuilder<TEntity> Annotation(string annotation, object value)
                => Wrap(EntityTypeBuilder.Annotation(annotation, value));

            public override TestKeyBuilder Key(Expression<Func<TEntity, object>> keyExpression)
                => new TestKeyBuilder(EntityTypeBuilder.Key(keyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));

            public override TestKeyBuilder Key(params string[] propertyNames)
                => new TestKeyBuilder(EntityTypeBuilder.Key(propertyNames));

            public override TestKeyBuilder AlternateKey(Expression<Func<TEntity, object>> keyExpression)
                => new TestKeyBuilder(EntityTypeBuilder.AlternateKey(keyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));

            public override TestKeyBuilder AlternateKey(params string[] propertyNames)
                => new TestKeyBuilder(EntityTypeBuilder.AlternateKey(propertyNames));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            {
                var propertyInfo = propertyExpression.GetPropertyAccess();
                return new NonGenericTestPropertyBuilder<TProperty>(EntityTypeBuilder.Property(propertyInfo.PropertyType, propertyInfo.Name));
            }

            public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
                => new NonGenericTestPropertyBuilder<TProperty>(EntityTypeBuilder.Property<TProperty>(propertyName));

            public override TestEntityTypeBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> propertyExpression)
                => Wrap(EntityTypeBuilder.Ignore(propertyExpression.GetPropertyAccess().Name));

            public override TestEntityTypeBuilder<TEntity> Ignore(string propertyName)
                => Wrap(EntityTypeBuilder.Ignore(propertyName));

            public override TestIndexBuilder Index(Expression<Func<TEntity, object>> indexExpression)
                => new TestIndexBuilder(EntityTypeBuilder.Index(indexExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));

            public override TestIndexBuilder Index(params string[] propertyNames)
                => new TestIndexBuilder(EntityTypeBuilder.Index(propertyNames));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> Reference<TRelatedEntity>(Expression<Func<TEntity, TRelatedEntity>> reference = null)
                => new NonGenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.Reference(typeof(TRelatedEntity), reference?.GetPropertyAccess().Name));

            public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> Collection<TRelatedEntity>(Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collection = null)
                => new NonGenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.Collection(typeof(TRelatedEntity), collection?.GetPropertyAccess().Name));
        }

        protected class NonGenericTestPropertyBuilder<TProperty> : TestPropertyBuilder<TProperty>
        {
            public NonGenericTestPropertyBuilder(PropertyBuilder propertyBuilder)
            {
                PropertyBuilder = propertyBuilder;
            }

            private PropertyBuilder PropertyBuilder { get; }

            public override Property Metadata => PropertyBuilder.Metadata;

            public override TestPropertyBuilder<TProperty> Annotation(string annotation, object value)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.Annotation(annotation, value));

            public override TestPropertyBuilder<TProperty> Required(bool isRequired = true)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.Required(isRequired));

            public override TestPropertyBuilder<TProperty> MaxLength(int maxLength)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.MaxLength(maxLength));

            public override TestPropertyBuilder<TProperty> ConcurrencyToken(bool isConcurrencyToken = true)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.ConcurrencyToken(isConcurrencyToken));

            public override TestPropertyBuilder<TProperty> StoreGeneratedPattern(StoreGeneratedPattern storeGeneratedPattern)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.StoreGeneratedPattern(storeGeneratedPattern));
        }

        protected class NonGenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity> : TestReferenceNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public NonGenericTestReferenceNavigationBuilder(ReferenceNavigationBuilder referenceNavigationBuilder)
            {
                ReferenceNavigationBuilder = referenceNavigationBuilder;
            }

            protected ReferenceNavigationBuilder ReferenceNavigationBuilder { get; }

            public override TestReferenceCollectionBuilder<TRelatedEntity, TEntity> InverseCollection(Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> collection = null)
                => new NonGenericTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(ReferenceNavigationBuilder.InverseCollection(collection?.GetPropertyAccess().Name));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> InverseReference(Expression<Func<TRelatedEntity, TEntity>> reference = null)
                => new NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(ReferenceNavigationBuilder.InverseReference(reference?.GetPropertyAccess().Name));
        }

        protected class NonGenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity> : TestCollectionNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public NonGenericTestCollectionNavigationBuilder(CollectionNavigationBuilder collectionNavigationBuilder)
            {
                CollectionNavigationBuilder = collectionNavigationBuilder;
            }

            private CollectionNavigationBuilder CollectionNavigationBuilder { get; }

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> InverseReference(Expression<Func<TRelatedEntity, TEntity>> reference = null)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(CollectionNavigationBuilder.InverseReference(reference?.GetPropertyAccess().Name));
        }

        protected class NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity> : TestReferenceCollectionBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public NonGenericTestReferenceCollectionBuilder(ReferenceCollectionBuilder referenceCollectionBuilder)
            {
                ReferenceCollectionBuilder = referenceCollectionBuilder;
            }

            private ReferenceCollectionBuilder ReferenceCollectionBuilder { get; }

            public override ForeignKey Metadata => ReferenceCollectionBuilder.Metadata;

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> ForeignKey(Expression<Func<TRelatedEntity, object>> foreignKeyExpression)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.ForeignKey(foreignKeyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> PrincipalKey(Expression<Func<TEntity, object>> keyExpression)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.PrincipalKey(keyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> ForeignKey(params string[] foreignKeyPropertyNames)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.ForeignKey(foreignKeyPropertyNames));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> PrincipalKey(params string[] keyPropertyNames)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.PrincipalKey(keyPropertyNames));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> Annotation(string annotation, object value)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.Annotation(annotation, value));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> Required(bool isRequired = true)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.Required(isRequired));
        }

        protected class NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> : TestReferenceReferenceBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public NonGenericTestReferenceReferenceBuilder(ReferenceReferenceBuilder referenceReferenceBuilder)
            {
                ReferenceReferenceBuilder = referenceReferenceBuilder;
            }

            protected ReferenceReferenceBuilder ReferenceReferenceBuilder { get; }

            public override ForeignKey Metadata => ReferenceReferenceBuilder.Metadata;

            protected virtual NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(ReferenceReferenceBuilder referenceReferenceBuilder)
                => new NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder);

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> Annotation(string annotation, object value)
                => Wrap(ReferenceReferenceBuilder.Annotation(annotation, value));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> ForeignKey<TDependentEntity>(Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                => Wrap(ReferenceReferenceBuilder.ForeignKey(typeof(TDependentEntity), foreignKeyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> PrincipalKey<TPrincipalEntity>(Expression<Func<TPrincipalEntity, object>> keyExpression)
                => Wrap(ReferenceReferenceBuilder.PrincipalKey(typeof(TPrincipalEntity), keyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> ForeignKey(Type dependentEntityType, params string[] foreignKeyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.ForeignKey(dependentEntityType, foreignKeyPropertyNames));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> PrincipalKey(Type principalEntityType, params string[] keyPropertyNames)
                => Wrap(ReferenceReferenceBuilder.PrincipalKey(principalEntityType, keyPropertyNames));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> Required(bool isRequired = true)
                => Wrap(ReferenceReferenceBuilder.Required(isRequired));
        }
    }
}
