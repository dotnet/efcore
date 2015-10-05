// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Data.Entity.FunctionalTests.TestUtilities;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.Data.Entity.Tests
{
    public abstract partial class ModelBuilderTest
    {
        // TODO: add convention-less tests
        // Issue #2410

        public abstract class ModelBuilderTestBase
        {
            protected void AssertEqual(
                IEnumerable<string> expectedNames,
                IEnumerable<string> actualNames,
                StringComparer stringComparer = null)
            {
                stringComparer = stringComparer ?? StringComparer.Ordinal;
                Assert.Equal(
                    new SortedSet<string>(expectedNames, stringComparer),
                    new SortedSet<string>(actualNames, stringComparer),
                    stringComparer);
            }

            protected void AssertEqual(
                IEnumerable<IProperty> expectedProperties,
                IEnumerable<IProperty> actualProperties,
                PropertyComparer propertyComparer = null)
            {
                propertyComparer = propertyComparer ?? new PropertyComparer();
                Assert.Equal(
                    new SortedSet<IProperty>(expectedProperties, propertyComparer),
                    new SortedSet<IProperty>(actualProperties, propertyComparer),
                    propertyComparer);
            }

            protected void AssertEqual(
                IEnumerable<INavigation> expectedNavigations,
                IEnumerable<INavigation> actualNavigations,
                NavigationComparer navigationComparer = null)
            {
                navigationComparer = navigationComparer ?? new NavigationComparer();
                Assert.Equal(
                    new SortedSet<INavigation>(expectedNavigations, navigationComparer),
                    new SortedSet<INavigation>(actualNavigations, navigationComparer),
                    navigationComparer);
            }

            protected void AssertEqual(
                IEnumerable<IKey> expectedKeys,
                IEnumerable<IKey> actualKeys,
                KeyComparer keyComparer = null)
            {
                keyComparer = keyComparer ?? new KeyComparer();
                Assert.Equal(
                    new SortedSet<IKey>(expectedKeys, keyComparer),
                    new SortedSet<IKey>(actualKeys, keyComparer),
                    keyComparer);
            }

            protected void AssertEqual(
                IEnumerable<IForeignKey> expectedForeignKeys,
                IEnumerable<IForeignKey> actualForeignKeys,
                ForeignKeyComparer foreignKeyComparer = null)
            {
                foreignKeyComparer = foreignKeyComparer ?? new ForeignKeyComparer();
                Assert.Equal(
                    new SortedSet<IForeignKey>(expectedForeignKeys, foreignKeyComparer),
                    new SortedSet<IForeignKey>(actualForeignKeys, foreignKeyComparer),
                    foreignKeyComparer);
            }

            protected void AssertEqual(
                IEnumerable<IIndex> expectedIndexes,
                IEnumerable<IIndex> actualIndexes,
                IndexComparer indexComparer = null)
            {
                indexComparer = indexComparer ?? new IndexComparer();
                Assert.Equal(
                    new SortedSet<IIndex>(expectedIndexes, indexComparer),
                    new SortedSet<IIndex>(actualIndexes, indexComparer),
                    indexComparer);
            }

            protected TestModelBuilder CreateModelBuilder() => CreateModelBuilder(new Model());

            protected virtual TestModelBuilder CreateModelBuilder(Model model)
                => CreateTestModelBuilder(TestHelpers.Instance.CreateConventionBuilder(model));

            protected TestModelBuilder HobNobBuilder()
            {
                var builder = CreateModelBuilder(new Model());

                builder.Entity<Hob>().HasKey(e => new { e.Id1, e.Id2 });
                builder.Entity<Nob>().HasKey(e => new { e.Id1, e.Id2 });

                return builder;
            }

            protected abstract TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder);
        }

        public abstract class TestModelBuilder
        {
            protected TestModelBuilder(ModelBuilder modelBuilder)
            {
                ModelBuilder = modelBuilder;
            }

            public virtual Model Model => ModelBuilder.Model;
            protected ModelBuilder ModelBuilder { get; }

            public abstract TestEntityTypeBuilder<TEntity> Entity<TEntity>()
                where TEntity : class;

            public abstract TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
                where TEntity : class;

            public abstract TestModelBuilder Ignore<TEntity>()
                where TEntity : class;

            public virtual TestModelBuilder Validate()
            {                
                return ModelBuilder.Validate() == null ? null : this;
            }
        }

        public abstract class TestEntityTypeBuilder<TEntity>
            where TEntity : class
        {
            public abstract EntityType Metadata { get; }
            public abstract TestEntityTypeBuilder<TEntity> HasAnnotation(string annotation, object value);

            public abstract TestEntityTypeBuilder<TEntity> BaseEntity<TBaseEntity>()
                where TBaseEntity : class;

            public abstract TestEntityTypeBuilder<TEntity> BaseEntity(string baseEntityTypeName);
            public abstract TestKeyBuilder HasKey(Expression<Func<TEntity, object>> keyExpression);
            public abstract TestKeyBuilder HasKey(params string[] propertyNames);
            public abstract TestKeyBuilder HasAlternateKey(Expression<Func<TEntity, object>> keyExpression);
            public abstract TestKeyBuilder HasAlternateKey(params string[] propertyNames);

            public abstract TestPropertyBuilder<TProperty> Property<TProperty>(
                Expression<Func<TEntity, TProperty>> propertyExpression);

            public abstract TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName);

            public abstract TestEntityTypeBuilder<TEntity> Ignore(
                Expression<Func<TEntity, object>> propertyExpression);

            public abstract TestEntityTypeBuilder<TEntity> Ignore(string propertyName);

            public abstract TestIndexBuilder HasIndex(Expression<Func<TEntity, object>> indexExpression);
            public abstract TestIndexBuilder HasIndex(params string[] propertyNames);

            public abstract TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> reference = null)
                where TRelatedEntity : class;

            public abstract TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collection = null)
                where TRelatedEntity : class;
        }

        public class TestKeyBuilder
        {
            public TestKeyBuilder(KeyBuilder keyBuilder)
            {
                KeyBuilder = keyBuilder;
            }

            private KeyBuilder KeyBuilder { get; }
            public Key Metadata => KeyBuilder.Metadata;

            public virtual TestKeyBuilder HasAnnotation(string annotation, object value)
            {
                return new TestKeyBuilder(KeyBuilder.HasAnnotation(annotation, value));
            }
        }

        public class TestIndexBuilder
        {
            public TestIndexBuilder(IndexBuilder indexBuilder)
            {
                IndexBuilder = indexBuilder;
            }

            private IndexBuilder IndexBuilder { get; }
            public Index Metadata => IndexBuilder.Metadata;

            public virtual TestIndexBuilder HasAnnotation(string annotation, object value)
            {
                return new TestIndexBuilder(IndexBuilder.HasAnnotation(annotation, value));
            }

            public virtual TestIndexBuilder IsUnique(bool isUnique = true)
            {
                return new TestIndexBuilder(IndexBuilder.IsUnique(isUnique));
            }
        }

        public abstract class TestPropertyBuilder<TProperty>
        {
            public abstract Property Metadata { get; }
            public abstract TestPropertyBuilder<TProperty> HasAnnotation(string annotation, object value);
            public abstract TestPropertyBuilder<TProperty> IsRequired(bool isRequired = true);
            public abstract TestPropertyBuilder<TProperty> HasMaxLength(int maxLength);
            public abstract TestPropertyBuilder<TProperty> IsConcurrencyToken(bool isConcurrencyToken = true);

            public abstract TestPropertyBuilder<TProperty> ValueGeneratedNever();
            public abstract TestPropertyBuilder<TProperty> ValueGeneratedOnAdd();
            public abstract TestPropertyBuilder<TProperty> ValueGeneratedOnAddOrUpdate();
        }

        public abstract class TestCollectionNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> reference = null);
        }

        public abstract class TestReferenceNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public abstract TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(
                Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> collection = null);

            public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> reference = null);
        }

        public abstract class TestReferenceCollectionBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public abstract ForeignKey Metadata { get; }

            public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(
                Expression<Func<TRelatedEntity, object>> foreignKeyExpression);

            public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
                Expression<Func<TEntity, object>> keyExpression);

            public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(
                params string[] foreignKeyPropertyNames);

            public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
                params string[] keyPropertyNames);

            public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasAnnotation(
                string annotation, object value);

            public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> IsRequired(bool isRequired = true);

            public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WillCascadeOnDelete(bool cascade = true);
        }

        public abstract class TestReferenceReferenceBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public abstract ForeignKey Metadata { get; }

            public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasAnnotation(
                string annotation, object value);

            public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
                Expression<Func<TDependentEntity, object>> foreignKeyExpression);

            public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                Expression<Func<TPrincipalEntity, object>> keyExpression);

            public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey(
                Type dependentEntityType, params string[] foreignKeyPropertyNames);

            public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
                Type principalEntityType, params string[] keyPropertyNames);

            public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> IsRequired(bool isRequired = true);

            public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WillCascadeOnDelete(bool cascade = true);
        }
    }
}
