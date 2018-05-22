// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public abstract partial class ModelBuilderTest
    {
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
                ForeignKeyStrictComparer foreignKeyComparer = null)
            {
                foreignKeyComparer = foreignKeyComparer ?? new ForeignKeyStrictComparer();
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

            protected virtual TestModelBuilder CreateModelBuilder()
                => CreateTestModelBuilder(InMemoryTestHelpers.Instance);

            protected TestModelBuilder HobNobBuilder()
            {
                var builder = CreateModelBuilder();

                builder.Entity<Hob>().HasKey(e => new { e.Id1, e.Id2 });
                builder.Entity<Nob>().HasKey(e => new { e.Id1, e.Id2 });

                return builder;
            }

            protected abstract TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers);
        }

        public abstract class TestModelBuilder
        {
            protected TestModelBuilder(TestHelpers testHelpers)
            {
                Log = new List<(LogLevel, EventId, string)>();
                var options = new LoggingOptions();
                options.Initialize(new DbContextOptionsBuilder().EnableSensitiveDataLogging(false).Options);
                var validationLogger = new DiagnosticsLogger<DbLoggerCategory.Model.Validation>(
                    new ListLoggerFactory(Log, l => l == DbLoggerCategory.Model.Validation.Name),
                    options,
                    new DiagnosticListener("Fake"));
                var modelLogger = new DiagnosticsLogger<DbLoggerCategory.Model>(
                    new ListLoggerFactory(Log, l => l == DbLoggerCategory.Model.Name),
                    options,
                    new DiagnosticListener("Fake"));

                var contextServices = testHelpers.CreateContextServices();

                ModelBuilder = new ModelBuilder(new CompositeConventionSetBuilder(contextServices.GetRequiredService<IEnumerable<IConventionSetBuilder>>().ToList())
                    .AddConventions(new CoreConventionSetBuilder(
                    contextServices.GetRequiredService<CoreConventionSetBuilderDependencies>().With(modelLogger))
                    .CreateConventionSet()));

                ModelValidator = new ModelValidator(new ModelValidatorDependencies(validationLogger, modelLogger));
            }

            public virtual IMutableModel Model => ModelBuilder.Model;
            protected ModelBuilder ModelBuilder { get; }
            protected IModelValidator ModelValidator { get; }
            public List<(LogLevel Level, EventId Id, string Message)> Log { get; }

            public TestModelBuilder HasAnnotation(string annotation, object value)
            {
                ModelBuilder.HasAnnotation(annotation, value);
                return this;
            }

            public abstract TestEntityTypeBuilder<TEntity> Entity<TEntity>()
                where TEntity : class;

            public abstract TestOwnedEntityTypeBuilder<TEntity> Owned<TEntity>()
                where TEntity : class;

            public abstract TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
                where TEntity : class;

            public abstract TestQueryTypeBuilder<TQuery> Query<TQuery>()
                where TQuery : class;

            public abstract TestModelBuilder Ignore<TEntity>()
                where TEntity : class;

            public virtual TestModelBuilder Validate()
            {
                var modelBuilder = ModelBuilder.GetInfrastructure().Metadata.Validate();
                ModelValidator.Validate(modelBuilder.Metadata);

                return this;
            }

            public virtual string GetDisplayName(Type entityType) => entityType.Name;

            public virtual TestModelBuilder UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
            {
                ModelBuilder.UsePropertyAccessMode(propertyAccessMode);

                return this;
            }
        }

        public abstract class TestEntityTypeBuilder<TEntity>
            where TEntity : class
        {
            public abstract IMutableEntityType Metadata { get; }
            public abstract TestEntityTypeBuilder<TEntity> HasAnnotation(string annotation, object value);

            public abstract TestEntityTypeBuilder<TEntity> HasBaseType<TBaseEntity>()
                where TBaseEntity : class;

            public abstract TestEntityTypeBuilder<TEntity> HasBaseType(string baseEntityTypeName);
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

            public abstract TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression)
                where TRelatedEntity : class;

            public abstract TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression,
                Action<TestReferenceOwnershipBuilder<TEntity, TRelatedEntity>> buildAction)
                where TRelatedEntity : class;

            public abstract TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression = null)
                where TRelatedEntity : class;

            public abstract TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression = null)
                where TRelatedEntity : class;

            public abstract TestEntityTypeBuilder<TEntity> HasQueryFilter(Expression<Func<TEntity, bool>> filter);

            public abstract TestEntityTypeBuilder<TEntity> HasChangeTrackingStrategy(ChangeTrackingStrategy changeTrackingStrategy);

            public abstract TestEntityTypeBuilder<TEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode);

            public abstract DataBuilder<TEntity> HasData(params TEntity[] data);
        }

        public abstract class TestOwnedEntityTypeBuilder<TEntity>
            where TEntity : class
        {
        }

        public abstract class TestQueryTypeBuilder<TEntity>
            where TEntity : class
        {
            public abstract IMutableEntityType Metadata { get; }
            public abstract TestQueryTypeBuilder<TEntity> HasAnnotation(string annotation, object value);

            public abstract TestQueryTypeBuilder<TEntity> HasBaseType<TBaseEntity>()
                where TBaseEntity : class;

            public abstract TestQueryTypeBuilder<TEntity> HasBaseType(string baseEntityTypeName);

            public abstract TestPropertyBuilder<TProperty> Property<TProperty>(
                Expression<Func<TEntity, TProperty>> propertyExpression);

            public abstract TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName);

            public abstract TestQueryTypeBuilder<TEntity> Ignore(
                Expression<Func<TEntity, object>> propertyExpression);

            public abstract TestQueryTypeBuilder<TEntity> Ignore(string propertyName);

            public abstract TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression = null)
                where TRelatedEntity : class;

            public abstract TestQueryTypeBuilder<TEntity> HasQueryFilter(Expression<Func<TEntity, bool>> filter);

            public abstract TestQueryTypeBuilder<TEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode);
        }

        public class TestKeyBuilder
        {
            public TestKeyBuilder(KeyBuilder keyBuilder)
            {
                KeyBuilder = keyBuilder;
            }

            private KeyBuilder KeyBuilder { get; }
            public IMutableKey Metadata => KeyBuilder.Metadata;

            public virtual TestKeyBuilder HasAnnotation(string annotation, object value)
                => new TestKeyBuilder(KeyBuilder.HasAnnotation(annotation, value));
        }

        public class TestIndexBuilder : IInfrastructure<IndexBuilder>
        {
            public TestIndexBuilder(IndexBuilder indexBuilder)
            {
                IndexBuilder = indexBuilder;
            }

            private IndexBuilder IndexBuilder { get; }
            public IMutableIndex Metadata => IndexBuilder.Metadata;

            public virtual TestIndexBuilder HasAnnotation(string annotation, object value)
                => new TestIndexBuilder(IndexBuilder.HasAnnotation(annotation, value));

            public virtual TestIndexBuilder IsUnique(bool isUnique = true)
                => new TestIndexBuilder(IndexBuilder.IsUnique(isUnique));

            IndexBuilder IInfrastructure<IndexBuilder>.Instance => IndexBuilder;
        }

        public abstract class TestPropertyBuilder<TProperty>
        {
            public abstract IMutableProperty Metadata { get; }
            public abstract TestPropertyBuilder<TProperty> HasAnnotation(string annotation, object value);
            public abstract TestPropertyBuilder<TProperty> IsRequired(bool isRequired = true);
            public abstract TestPropertyBuilder<TProperty> HasMaxLength(int maxLength);
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
            public abstract TestPropertyBuilder<TProperty> HasValueGenerator(Func<IProperty, IEntityType, ValueGenerator> factory);

            public abstract TestPropertyBuilder<TProperty> HasField(string fieldName);
            public abstract TestPropertyBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode);

            public abstract TestPropertyBuilder<TProperty> HasConversion<TProvider>();
            public abstract TestPropertyBuilder<TProperty> HasConversion(Type providerClrType);

            public abstract TestPropertyBuilder<TProperty> HasConversion<TProvider>(
                Expression<Func<TProperty, TProvider>> convertToProviderExpression,
                Expression<Func<TProvider, TProperty>> convertFromProviderExpression);

            public abstract TestPropertyBuilder<TProperty> HasConversion<TProvider>(ValueConverter<TProperty, TProvider> converter);
            public abstract TestPropertyBuilder<TProperty> HasConversion(ValueConverter converter);
        }

        public abstract class TestCollectionNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null);
        }

        public abstract class TestReferenceNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public abstract TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(
                Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> navigationExpression = null);

            public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null);
        }

        public abstract class TestReferenceCollectionBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public abstract IMutableForeignKey Metadata { get; }

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

            public abstract TestReferenceCollectionBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior);
        }

        public abstract class TestReferenceReferenceBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public abstract IMutableForeignKey Metadata { get; }

            public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasAnnotation(
                string annotation, object value);

            public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(
                Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                where TDependentEntity : class;

            public abstract TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(
                Expression<Func<TPrincipalEntity, object>> keyExpression)
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

        public abstract class TestReferenceOwnershipBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public abstract IMutableForeignKey Metadata { get; }
            public abstract IMutableEntityType OwnedEntityType { get; }

            public abstract TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasForeignKeyAnnotation(
                string annotation, object value);

            public abstract TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasForeignKey(
                params string[] foreignKeyPropertyNames);

            public abstract TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasForeignKey(
                Expression<Func<TRelatedEntity, object>> foreignKeyExpression);

            public abstract TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
                params string[] keyPropertyNames);

            public abstract TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
                Expression<Func<TEntity, object>> keyExpression);

            public abstract TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior);

            public abstract TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasEntityTypeAnnotation(string annotation, object value);

            public abstract TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName);
            public abstract TestPropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TRelatedEntity, TProperty>> propertyExpression);

            public abstract TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> Ignore(string propertyName);

            public abstract TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> Ignore(
                Expression<Func<TRelatedEntity, object>> propertyExpression);

            public abstract TestIndexBuilder HasIndex(params string[] propertyNames);
            public abstract TestIndexBuilder HasIndex(Expression<Func<TRelatedEntity, object>> indexExpression);

            public abstract TestReferenceOwnershipBuilder<TRelatedEntity, TNewRelatedEntity> OwnsOne<TNewRelatedEntity>(
                Expression<Func<TRelatedEntity, TNewRelatedEntity>> navigationExpression)
                where TNewRelatedEntity : class;

            public abstract TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> OwnsOne<TNewRelatedEntity>(
                Expression<Func<TRelatedEntity, TNewRelatedEntity>> navigationExpression,
                Action<TestReferenceOwnershipBuilder<TRelatedEntity, TNewRelatedEntity>> buildAction)
                where TNewRelatedEntity : class;

            public abstract TestReferenceNavigationBuilder<TRelatedEntity, TNewRelatedEntity> HasOne<TNewRelatedEntity>(
                Expression<Func<TRelatedEntity, TNewRelatedEntity>> navigationExpression = null)
                where TNewRelatedEntity : class;

            public abstract TestCollectionNavigationBuilder<TRelatedEntity, TNewRelatedEntity> HasMany<TNewRelatedEntity>(
                Expression<Func<TRelatedEntity, IEnumerable<TNewRelatedEntity>>> navigationExpression = null)
                where TNewRelatedEntity : class;

            public abstract TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasChangeTrackingStrategy(
                ChangeTrackingStrategy changeTrackingStrategy);

            public abstract TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode);
        }
    }
}
