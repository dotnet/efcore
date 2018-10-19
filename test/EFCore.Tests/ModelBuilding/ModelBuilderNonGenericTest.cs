// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Microsoft.EntityFrameworkCore.ModelBuilding
{
    public class ModelBuilderNonGenericTest : ModelBuilderTest
    {
        public class NonGenericNonRelationship : NonRelationshipTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new NonGenericTestModelBuilder(testHelpers);
        }

        public class NonGenericInheritance : InheritanceTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new NonGenericTestModelBuilder(testHelpers);
        }

        public class NonGenericOwnedTypes : OwnedTypesTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new NonGenericTestModelBuilder(testHelpers);
        }

        public class NonGenericQueryTypes : QueryTypesTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new NonGenericTestModelBuilder(testHelpers);
        }

        public class NonGenericOneToMany : OneToManyTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new NonGenericTestModelBuilder(testHelpers);
        }

        public class NonGenericManyToOne : ManyToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new NonGenericTestModelBuilder(testHelpers);
        }

        public class NonGenericOneToOne : OneToOneTestBase
        {
            protected override TestModelBuilder CreateTestModelBuilder(TestHelpers testHelpers)
                => new NonGenericTestModelBuilder(testHelpers);
        }

        private class NonGenericTestModelBuilder : TestModelBuilder
        {
            public NonGenericTestModelBuilder(TestHelpers testHelpers)
                : base(testHelpers)
            {
            }

            public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
                => new NonGenericTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity(typeof(TEntity)));

            public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
            {
                ModelBuilder.Entity(
                    typeof(TEntity), entityTypeBuilder =>
                        buildAction(new NonGenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder)));
                return this;
            }

            public override TestQueryTypeBuilder<TQuery> Query<TQuery>()
                => new NonGenericTestQueryTypeBuilder<TQuery>(ModelBuilder.Query(typeof(TQuery)));

            public override TestOwnedEntityTypeBuilder<TEntity> Owned<TEntity>()
                => new NonGenericTestOwnedEntityTypeBuilder<TEntity>(ModelBuilder.Owned(typeof(TEntity)));

            public override TestModelBuilder Ignore<TEntity>()
            {
                ModelBuilder.Ignore(typeof(TEntity));
                return this;
            }
        }

        protected class NonGenericTestEntityTypeBuilder<TEntity> : TestEntityTypeBuilder<TEntity>, IInfrastructure<EntityTypeBuilder>
            where TEntity : class
        {
            public NonGenericTestEntityTypeBuilder(EntityTypeBuilder entityTypeBuilder)
            {
                EntityTypeBuilder = entityTypeBuilder;
            }

            protected EntityTypeBuilder EntityTypeBuilder { get; }
            public override IMutableEntityType Metadata => EntityTypeBuilder.Metadata;

            protected virtual NonGenericTestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder entityTypeBuilder)
                => new NonGenericTestEntityTypeBuilder<TEntity>(entityTypeBuilder);

            public override TestEntityTypeBuilder<TEntity> HasAnnotation(string annotation, object value)
                => Wrap(EntityTypeBuilder.HasAnnotation(annotation, value));

            public override TestEntityTypeBuilder<TEntity> HasBaseType<TBaseEntity>()
                => Wrap(EntityTypeBuilder.HasBaseType(typeof(TBaseEntity)));

            public override TestEntityTypeBuilder<TEntity> HasBaseType(string baseEntityTypeName)
                => Wrap(EntityTypeBuilder.HasBaseType(baseEntityTypeName));

            public override TestKeyBuilder HasKey(Expression<Func<TEntity, object>> keyExpression)
                => new TestKeyBuilder(EntityTypeBuilder.HasKey(keyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestKeyBuilder HasKey(params string[] propertyNames)
                => new TestKeyBuilder(EntityTypeBuilder.HasKey(propertyNames));

            public override TestKeyBuilder HasAlternateKey(Expression<Func<TEntity, object>> keyExpression)
                => new TestKeyBuilder(EntityTypeBuilder.HasAlternateKey(keyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestKeyBuilder HasAlternateKey(params string[] propertyNames)
                => new TestKeyBuilder(EntityTypeBuilder.HasAlternateKey(propertyNames));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            {
                var propertyInfo = propertyExpression.GetPropertyAccess();
                return new NonGenericTestPropertyBuilder<TProperty>(EntityTypeBuilder.Property(propertyInfo.PropertyType, propertyInfo.GetSimpleMemberName()));
            }

            public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
                => new NonGenericTestPropertyBuilder<TProperty>(EntityTypeBuilder.Property<TProperty>(propertyName));

            public override TestEntityTypeBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> propertyExpression)
                => Wrap(EntityTypeBuilder.Ignore(propertyExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestEntityTypeBuilder<TEntity> Ignore(string propertyName)
                => Wrap(EntityTypeBuilder.Ignore(propertyName));

            public override TestIndexBuilder HasIndex(Expression<Func<TEntity, object>> indexExpression)
                => new TestIndexBuilder(EntityTypeBuilder.HasIndex(indexExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestIndexBuilder HasIndex(params string[] propertyNames)
                => new TestIndexBuilder(EntityTypeBuilder.HasIndex(propertyNames));

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression)
                => new NonGenericTestReferenceOwnershipBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.OwnsOne(typeof(TRelatedEntity), navigationExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestEntityTypeBuilder<TEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression,
                Action<TestReferenceOwnershipBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(EntityTypeBuilder.OwnsOne(
                    typeof(TRelatedEntity),
                    navigationExpression.GetPropertyAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericTestReferenceOwnershipBuilder<TEntity, TRelatedEntity>(r))));

            public override TestCollectionOwnershipBuilder<TEntity, TRelatedEntity> OwnsMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression)
                => new NonGenericTestCollectionOwnershipBuilder<TEntity, TRelatedEntity>(
                    EntityTypeBuilder.OwnsMany(typeof(TRelatedEntity), navigationExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestEntityTypeBuilder<TEntity> OwnsMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression,
                Action<TestCollectionOwnershipBuilder<TEntity, TRelatedEntity>> buildAction)
                => Wrap(EntityTypeBuilder.OwnsMany(
                    typeof(TRelatedEntity),
                    navigationExpression.GetPropertyAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericTestCollectionOwnershipBuilder<TEntity, TRelatedEntity>(r))));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression = null)
                => new NonGenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasOne(typeof(TRelatedEntity), navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));

            public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(
                Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> navigationExpression = null)
                => new NonGenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasMany(typeof(TRelatedEntity), navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));

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

            public EntityTypeBuilder Instance => EntityTypeBuilder;
        }

        protected class NonGenericTestOwnedEntityTypeBuilder<TEntity> : TestOwnedEntityTypeBuilder<TEntity>, IInfrastructure<OwnedEntityTypeBuilder>
            where TEntity : class
        {
            public NonGenericTestOwnedEntityTypeBuilder(OwnedEntityTypeBuilder ownedEntityTypeBuilder)
            {
                OwnedEntityTypeBuilder = ownedEntityTypeBuilder;
            }

            protected OwnedEntityTypeBuilder OwnedEntityTypeBuilder { get; }

            public OwnedEntityTypeBuilder Instance => OwnedEntityTypeBuilder;
        }

        protected class NonGenericTestQueryTypeBuilder<TEntity> : TestQueryTypeBuilder<TEntity>, IInfrastructure<QueryTypeBuilder>
            where TEntity : class
        {
            public NonGenericTestQueryTypeBuilder(QueryTypeBuilder queryTypeBuilder)
            {
                QueryTypeBuilder = queryTypeBuilder;
            }

            protected QueryTypeBuilder QueryTypeBuilder { get; }

            public override IMutableEntityType Metadata => QueryTypeBuilder.Metadata;

            protected virtual NonGenericTestQueryTypeBuilder<TEntity> Wrap(QueryTypeBuilder queryTypeBuilder)
                => new NonGenericTestQueryTypeBuilder<TEntity>(queryTypeBuilder);

            public override TestQueryTypeBuilder<TEntity> HasAnnotation(string annotation, object value)
                => Wrap(QueryTypeBuilder.HasAnnotation(annotation, value));

            public override TestQueryTypeBuilder<TEntity> HasBaseType<TBaseEntity>()
                => Wrap(QueryTypeBuilder.HasBaseType(typeof(TBaseEntity)));

            public override TestQueryTypeBuilder<TEntity> HasBaseType(string baseEntityTypeName)
                => Wrap(QueryTypeBuilder.HasBaseType(baseEntityTypeName));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TEntity, TProperty>> propertyExpression)
            {
                var propertyInfo = propertyExpression.GetPropertyAccess();
                return new NonGenericTestPropertyBuilder<TProperty>(QueryTypeBuilder.Property(propertyInfo.PropertyType, propertyInfo.GetSimpleMemberName()));
            }

            public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
                => new NonGenericTestPropertyBuilder<TProperty>(QueryTypeBuilder.Property<TProperty>(propertyName));

            public override TestQueryTypeBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> propertyExpression)
                => Wrap(QueryTypeBuilder.Ignore(propertyExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestQueryTypeBuilder<TEntity> Ignore(string propertyName)
                => Wrap(QueryTypeBuilder.Ignore(propertyName));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(
                Expression<Func<TEntity, TRelatedEntity>> navigationExpression = null)
                => new NonGenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(QueryTypeBuilder.HasOne(typeof(TRelatedEntity), navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));

            public override TestQueryTypeBuilder<TEntity> HasQueryFilter(Expression<Func<TEntity, bool>> filter)
                => Wrap(QueryTypeBuilder.HasQueryFilter(filter));

            public override TestQueryTypeBuilder<TEntity> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
                => Wrap(QueryTypeBuilder.UsePropertyAccessMode(propertyAccessMode));

            public QueryTypeBuilder Instance => QueryTypeBuilder;
        }

        protected class NonGenericTestPropertyBuilder<TProperty> : TestPropertyBuilder<TProperty>, IInfrastructure<PropertyBuilder>
        {
            public NonGenericTestPropertyBuilder(PropertyBuilder propertyBuilder)
            {
                PropertyBuilder = propertyBuilder;
            }

            private PropertyBuilder PropertyBuilder { get; }

            public override IMutableProperty Metadata => PropertyBuilder.Metadata;

            public override TestPropertyBuilder<TProperty> HasAnnotation(string annotation, object value)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasAnnotation(annotation, value));

            public override TestPropertyBuilder<TProperty> IsRequired(bool isRequired = true)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.IsRequired(isRequired));

            public override TestPropertyBuilder<TProperty> HasMaxLength(int maxLength)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasMaxLength(maxLength));

            public override TestPropertyBuilder<TProperty> IsUnicode(bool unicode = true)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.IsUnicode(unicode));

            public override TestPropertyBuilder<TProperty> IsRowVersion()
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.IsRowVersion());

            public override TestPropertyBuilder<TProperty> IsConcurrencyToken(bool isConcurrencyToken = true)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.IsConcurrencyToken(isConcurrencyToken));

            public override TestPropertyBuilder<TProperty> ValueGeneratedNever()
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.ValueGeneratedNever());

            public override TestPropertyBuilder<TProperty> ValueGeneratedOnAdd()
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.ValueGeneratedOnAdd());

            public override TestPropertyBuilder<TProperty> ValueGeneratedOnAddOrUpdate()
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.ValueGeneratedOnAddOrUpdate());

            public override TestPropertyBuilder<TProperty> ValueGeneratedOnUpdate()
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.ValueGeneratedOnUpdate());

            public override TestPropertyBuilder<TProperty> HasValueGenerator<TGenerator>()
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasValueGenerator<TGenerator>());

            public override TestPropertyBuilder<TProperty> HasValueGenerator(Type valueGeneratorType)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasValueGenerator(valueGeneratorType));

            public override TestPropertyBuilder<TProperty> HasValueGenerator(Func<IProperty, IEntityType, ValueGenerator> factory)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasValueGenerator(factory));

            public override TestPropertyBuilder<TProperty> HasField(string fieldName)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasField(fieldName));

            public override TestPropertyBuilder<TProperty> UsePropertyAccessMode(PropertyAccessMode propertyAccessMode)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.UsePropertyAccessMode(propertyAccessMode));

            public override TestPropertyBuilder<TProperty> HasConversion<TProvider>()
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasConversion<TProvider>());

            public override TestPropertyBuilder<TProperty> HasConversion(Type providerClrType)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasConversion(providerClrType));

            public override TestPropertyBuilder<TProperty> HasConversion<TProvider>(
                Expression<Func<TProperty, TProvider>> convertToProviderExpression,
                Expression<Func<TProvider, TProperty>> convertFromProviderExpression)
                => new NonGenericTestPropertyBuilder<TProperty>(
                    PropertyBuilder.HasConversion(
                        new ValueConverter<TProperty, TProvider>(convertToProviderExpression, convertFromProviderExpression)));

            public override TestPropertyBuilder<TProperty> HasConversion<TStore>(ValueConverter<TProperty, TStore> converter)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasConversion(converter));

            public override TestPropertyBuilder<TProperty> HasConversion(ValueConverter converter)
                => new NonGenericTestPropertyBuilder<TProperty>(PropertyBuilder.HasConversion(converter));

            PropertyBuilder IInfrastructure<PropertyBuilder>.Instance => PropertyBuilder;
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

            public override TestReferenceCollectionBuilder<TRelatedEntity, TEntity> WithMany(
                Expression<Func<TRelatedEntity, IEnumerable<TEntity>>> navigationExpression = null)
                => new NonGenericTestReferenceCollectionBuilder<TRelatedEntity, TEntity>(
                    ReferenceNavigationBuilder.WithMany(navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(
                Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null)
                => new NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(
                    ReferenceNavigationBuilder.WithOne(navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));
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

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> WithOne(Expression<Func<TRelatedEntity, TEntity>> navigationExpression = null)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(CollectionNavigationBuilder.WithOne(navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));
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

            public override IMutableForeignKey Metadata => ReferenceCollectionBuilder.Metadata;

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(Expression<Func<TRelatedEntity, object>> foreignKeyExpression)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.HasForeignKey(foreignKeyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(Expression<Func<TEntity, object>> keyExpression)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.HasPrincipalKey(keyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasForeignKey(params string[] foreignKeyPropertyNames)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.HasForeignKey(foreignKeyPropertyNames));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasPrincipalKey(params string[] keyPropertyNames)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.HasPrincipalKey(keyPropertyNames));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> HasAnnotation(string annotation, object value)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.HasAnnotation(annotation, value));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> IsRequired(bool isRequired = true)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.IsRequired(isRequired));

            public override TestReferenceCollectionBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior)
                => new NonGenericTestReferenceCollectionBuilder<TEntity, TRelatedEntity>(ReferenceCollectionBuilder.OnDelete(deleteBehavior));
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

            public override IMutableForeignKey Metadata => ReferenceReferenceBuilder.Metadata;

            protected virtual NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(ReferenceReferenceBuilder referenceReferenceBuilder)
                => new NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder);

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasAnnotation(string annotation, object value)
                => Wrap(ReferenceReferenceBuilder.HasAnnotation(annotation, value));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                => Wrap(
                    ReferenceReferenceBuilder.HasForeignKey(
                        typeof(TDependentEntity), foreignKeyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(Expression<Func<TPrincipalEntity, object>> keyExpression)
                => Wrap(
                    ReferenceReferenceBuilder.HasPrincipalKey(
                        typeof(TPrincipalEntity), keyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

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

        protected class NonGenericTestReferenceOwnershipBuilder<TEntity, TRelatedEntity>
            : TestReferenceOwnershipBuilder<TEntity, TRelatedEntity>, IInfrastructure<ReferenceOwnershipBuilder>
            where TEntity : class
            where TRelatedEntity : class
        {
            public NonGenericTestReferenceOwnershipBuilder(ReferenceOwnershipBuilder referenceOwnershipBuilder)
            {
                ReferenceOwnershipBuilder = referenceOwnershipBuilder;
            }

            protected ReferenceOwnershipBuilder ReferenceOwnershipBuilder { get; }

            public override IMutableForeignKey Metadata => ReferenceOwnershipBuilder.Metadata;
            public override IMutableEntityType OwnedEntityType => ReferenceOwnershipBuilder.OwnedEntityType;

            protected virtual NonGenericTestReferenceOwnershipBuilder<TNewEntity, TNewRelatedEntity> Wrap<TNewEntity, TNewRelatedEntity>(
                ReferenceOwnershipBuilder referenceOwnershipBuilder)
                where TNewEntity : class
                where TNewRelatedEntity : class
                => new NonGenericTestReferenceOwnershipBuilder<TNewEntity, TNewRelatedEntity>(referenceOwnershipBuilder);

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasEntityTypeAnnotation(string annotation, object value)
                => Wrap<TEntity, TRelatedEntity>(ReferenceOwnershipBuilder.HasEntityTypeAnnotation(annotation, value));

            public override TestKeyBuilder HasKey(Expression<Func<TRelatedEntity, object>> keyExpression)
                => new TestKeyBuilder(ReferenceOwnershipBuilder.HasKey(keyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestKeyBuilder HasKey(params string[] propertyNames)
                => new TestKeyBuilder(ReferenceOwnershipBuilder.HasKey(propertyNames));

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasForeignKeyAnnotation(string annotation, object value)
                => Wrap<TEntity, TRelatedEntity>(ReferenceOwnershipBuilder.HasForeignKeyAnnotation(annotation, value));

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasForeignKey(
                params string[] foreignKeyPropertyNames)
                => Wrap<TEntity, TRelatedEntity>(ReferenceOwnershipBuilder.HasForeignKey(foreignKeyPropertyNames));

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasForeignKey(
                Expression<Func<TRelatedEntity, object>> foreignKeyExpression)
                => Wrap<TEntity, TRelatedEntity>(
                    ReferenceOwnershipBuilder.HasForeignKey(
                        foreignKeyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
                params string[] keyPropertyNames)
                => Wrap<TEntity, TRelatedEntity>(ReferenceOwnershipBuilder.HasPrincipalKey(keyPropertyNames));

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasPrincipalKey(
                Expression<Func<TEntity, object>> keyExpression)
                => Wrap<TEntity, TRelatedEntity>(
                    ReferenceOwnershipBuilder.HasPrincipalKey(
                        keyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> OnDelete(DeleteBehavior deleteBehavior)
                => Wrap<TEntity, TRelatedEntity>(ReferenceOwnershipBuilder.OnDelete(deleteBehavior));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
                => new NonGenericTestPropertyBuilder<TProperty>(ReferenceOwnershipBuilder.Property<TProperty>(propertyName));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TRelatedEntity, TProperty>> propertyExpression)
            {
                var propertyInfo = propertyExpression.GetPropertyAccess();
                return new NonGenericTestPropertyBuilder<TProperty>(ReferenceOwnershipBuilder.Property(propertyInfo.PropertyType, propertyInfo.GetSimpleMemberName()));
            }

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> Ignore(string propertyName)
                => Wrap<TEntity, TRelatedEntity>(ReferenceOwnershipBuilder.Ignore(propertyName));

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> Ignore(
                Expression<Func<TRelatedEntity, object>> propertyExpression)
                => Wrap<TEntity, TRelatedEntity>(ReferenceOwnershipBuilder.Ignore(propertyExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestIndexBuilder HasIndex(params string[] propertyNames)
                => new TestIndexBuilder(ReferenceOwnershipBuilder.HasIndex(propertyNames));

            public override TestIndexBuilder HasIndex(Expression<Func<TRelatedEntity, object>> indexExpression)
                => new TestIndexBuilder(
                    ReferenceOwnershipBuilder.HasIndex(
                        indexExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceOwnershipBuilder<TRelatedEntity, TNewRelatedEntity> OwnsOne<TNewRelatedEntity>(
                Expression<Func<TRelatedEntity, TNewRelatedEntity>> navigationExpression)
                => Wrap<TRelatedEntity, TNewRelatedEntity>(ReferenceOwnershipBuilder.OwnsOne(
                    typeof(TNewRelatedEntity), navigationExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> OwnsOne<TNewRelatedEntity>(
                Expression<Func<TRelatedEntity, TNewRelatedEntity>> navigationExpression,
                Action<TestReferenceOwnershipBuilder<TRelatedEntity, TNewRelatedEntity>> buildAction)
                => Wrap<TEntity, TRelatedEntity>(ReferenceOwnershipBuilder.OwnsOne(
                    typeof(TNewRelatedEntity),
                    navigationExpression.GetPropertyAccess().GetSimpleMemberName(),
                    r => buildAction(Wrap<TRelatedEntity, TNewRelatedEntity>(r))));

            public override TestCollectionOwnershipBuilder<TRelatedEntity, TDependentEntity> OwnsMany<TDependentEntity>(
                Expression<Func<TRelatedEntity, IEnumerable<TDependentEntity>>> navigationExpression)
                => new NonGenericTestCollectionOwnershipBuilder<TRelatedEntity, TDependentEntity>(
                    ReferenceOwnershipBuilder.OwnsMany(typeof(TDependentEntity), navigationExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> OwnsMany<TDependentEntity>(
                Expression<Func<TRelatedEntity, IEnumerable<TDependentEntity>>> navigationExpression,
                Action<TestCollectionOwnershipBuilder<TRelatedEntity, TDependentEntity>> buildAction)
                => Wrap<TEntity, TRelatedEntity>(ReferenceOwnershipBuilder.OwnsMany(
                    typeof(TDependentEntity),
                    navigationExpression.GetPropertyAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericTestCollectionOwnershipBuilder<TRelatedEntity, TDependentEntity>(r))));

            public override TestReferenceNavigationBuilder<TRelatedEntity, TNewRelatedEntity> HasOne<TNewRelatedEntity>(
                Expression<Func<TRelatedEntity, TNewRelatedEntity>> navigationExpression = null)
                => new NonGenericTestReferenceNavigationBuilder<TRelatedEntity, TNewRelatedEntity>(
                    ReferenceOwnershipBuilder.HasOne(typeof(TNewRelatedEntity), navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));

            public override TestCollectionNavigationBuilder<TRelatedEntity, TNewRelatedEntity> HasMany<TNewRelatedEntity>(Expression<Func<TRelatedEntity, IEnumerable<TNewRelatedEntity>>> navigationExpression = null)
                => new NonGenericTestCollectionNavigationBuilder<TRelatedEntity, TNewRelatedEntity>(
                    ReferenceOwnershipBuilder.HasMany(typeof(TNewRelatedEntity), navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> HasChangeTrackingStrategy(
                ChangeTrackingStrategy changeTrackingStrategy)
                => Wrap<TEntity, TRelatedEntity>(ReferenceOwnershipBuilder.HasChangeTrackingStrategy(changeTrackingStrategy));

            public override TestReferenceOwnershipBuilder<TEntity, TRelatedEntity> UsePropertyAccessMode(
                PropertyAccessMode propertyAccessMode)
                => Wrap<TEntity, TRelatedEntity>(ReferenceOwnershipBuilder.UsePropertyAccessMode(propertyAccessMode));

            public override DataBuilder<TRelatedEntity> HasData(params TRelatedEntity[] data)
            {
                ReferenceOwnershipBuilder.HasData(data);
                return new DataBuilder<TRelatedEntity>();
            }

            public override DataBuilder<TRelatedEntity> HasData(params object[] data)
            {
                ReferenceOwnershipBuilder.HasData(data);
                return new DataBuilder<TRelatedEntity>();
            }

            ReferenceOwnershipBuilder IInfrastructure<ReferenceOwnershipBuilder>.Instance => ReferenceOwnershipBuilder;
        }

        protected class NonGenericTestCollectionOwnershipBuilder<TEntity, TDependentEntity>
            : TestCollectionOwnershipBuilder<TEntity, TDependentEntity>, IInfrastructure<CollectionOwnershipBuilder>
            where TEntity : class
            where TDependentEntity : class
        {
            public NonGenericTestCollectionOwnershipBuilder(CollectionOwnershipBuilder collectionOwnershipBuilder)
            {
                CollectionOwnershipBuilder = collectionOwnershipBuilder;
            }

            protected CollectionOwnershipBuilder CollectionOwnershipBuilder { get; }

            public override IMutableForeignKey Metadata => CollectionOwnershipBuilder.Metadata;
            public override IMutableEntityType OwnedEntityType => CollectionOwnershipBuilder.OwnedEntityType;

            protected virtual NonGenericTestCollectionOwnershipBuilder<TNewEntity, TNewRelatedEntity> Wrap<TNewEntity, TNewRelatedEntity>(
                CollectionOwnershipBuilder collectionOwnershipBuilder)
                where TNewEntity : class
                where TNewRelatedEntity : class
                => new NonGenericTestCollectionOwnershipBuilder<TNewEntity, TNewRelatedEntity>(collectionOwnershipBuilder);

            public override TestCollectionOwnershipBuilder<TEntity, TDependentEntity> HasEntityTypeAnnotation(string annotation, object value)
                => Wrap<TEntity, TDependentEntity>(CollectionOwnershipBuilder.HasEntityTypeAnnotation(annotation, value));

            public override TestKeyBuilder HasKey(Expression<Func<TDependentEntity, object>> keyExpression)
                => new TestKeyBuilder(CollectionOwnershipBuilder.HasKey(keyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestKeyBuilder HasKey(params string[] propertyNames)
                => new TestKeyBuilder(CollectionOwnershipBuilder.HasKey(propertyNames));

            public override TestCollectionOwnershipBuilder<TEntity, TDependentEntity> HasForeignKeyAnnotation(string annotation, object value)
                => Wrap<TEntity, TDependentEntity>(CollectionOwnershipBuilder.HasForeignKeyAnnotation(annotation, value));

            public override TestCollectionOwnershipBuilder<TEntity, TDependentEntity> HasForeignKey(
                params string[] foreignKeyPropertyNames)
                => Wrap<TEntity, TDependentEntity>(CollectionOwnershipBuilder.HasForeignKey(foreignKeyPropertyNames));

            public override TestCollectionOwnershipBuilder<TEntity, TDependentEntity> HasForeignKey(
                Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                => Wrap<TEntity, TDependentEntity>(CollectionOwnershipBuilder.HasForeignKey(
                    foreignKeyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestCollectionOwnershipBuilder<TEntity, TDependentEntity> HasPrincipalKey(
                params string[] keyPropertyNames)
                => Wrap<TEntity, TDependentEntity>(CollectionOwnershipBuilder.HasPrincipalKey(keyPropertyNames));

            public override TestCollectionOwnershipBuilder<TEntity, TDependentEntity> HasPrincipalKey(
                Expression<Func<TEntity, object>> keyExpression)
                => Wrap<TEntity, TDependentEntity>(CollectionOwnershipBuilder.HasPrincipalKey(
                    keyExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestCollectionOwnershipBuilder<TEntity, TDependentEntity> OnDelete(DeleteBehavior deleteBehavior)
                => Wrap<TEntity, TDependentEntity>(CollectionOwnershipBuilder.OnDelete(deleteBehavior));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(string propertyName)
                => new NonGenericTestPropertyBuilder<TProperty>(CollectionOwnershipBuilder.Property<TProperty>(propertyName));

            public override TestPropertyBuilder<TProperty> Property<TProperty>(Expression<Func<TDependentEntity, TProperty>> propertyExpression)
            {
                var propertyInfo = propertyExpression.GetPropertyAccess();
                return new NonGenericTestPropertyBuilder<TProperty>(CollectionOwnershipBuilder.Property(propertyInfo.PropertyType, propertyInfo.GetSimpleMemberName()));
            }

            public override TestCollectionOwnershipBuilder<TEntity, TDependentEntity> Ignore(string propertyName)
                => Wrap<TEntity, TDependentEntity>(CollectionOwnershipBuilder.Ignore(propertyName));

            public override TestCollectionOwnershipBuilder<TEntity, TDependentEntity> Ignore(
                Expression<Func<TDependentEntity, object>> propertyExpression)
                => Wrap<TEntity, TDependentEntity>(CollectionOwnershipBuilder.Ignore(propertyExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestIndexBuilder HasIndex(params string[] propertyNames)
                => new TestIndexBuilder(CollectionOwnershipBuilder.HasIndex(propertyNames));

            public override TestIndexBuilder HasIndex(Expression<Func<TDependentEntity, object>> indexExpression)
                => new TestIndexBuilder(CollectionOwnershipBuilder.HasIndex(
                    indexExpression.GetPropertyAccessList().Select(p => p.GetSimpleMemberName()).ToArray()));

            public override TestReferenceOwnershipBuilder<TDependentEntity, TRelatedEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TDependentEntity, TRelatedEntity>> navigationExpression)
                => new NonGenericTestReferenceOwnershipBuilder<TDependentEntity, TRelatedEntity>(CollectionOwnershipBuilder.OwnsOne(
                    typeof(TRelatedEntity), navigationExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestCollectionOwnershipBuilder<TEntity, TDependentEntity> OwnsOne<TRelatedEntity>(
                Expression<Func<TDependentEntity, TRelatedEntity>> navigationExpression,
                Action<TestReferenceOwnershipBuilder<TDependentEntity, TRelatedEntity>> buildAction)
                => Wrap<TEntity, TDependentEntity>(CollectionOwnershipBuilder.OwnsOne(
                    typeof(TRelatedEntity),
                    navigationExpression.GetPropertyAccess().GetSimpleMemberName(),
                    r => buildAction(new NonGenericTestReferenceOwnershipBuilder<TDependentEntity, TRelatedEntity>(r))));

            public override TestCollectionOwnershipBuilder<TDependentEntity, TNewDependentEntity> OwnsMany<TNewDependentEntity>(
                Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression)
                => Wrap<TDependentEntity, TNewDependentEntity>(
                    CollectionOwnershipBuilder.OwnsMany(typeof(TNewDependentEntity), navigationExpression.GetPropertyAccess().GetSimpleMemberName()));

            public override TestCollectionOwnershipBuilder<TEntity, TDependentEntity> OwnsMany<TNewDependentEntity>(
                Expression<Func<TDependentEntity, IEnumerable<TNewDependentEntity>>> navigationExpression,
                Action<TestCollectionOwnershipBuilder<TDependentEntity, TNewDependentEntity>> buildAction)
                => Wrap<TEntity, TDependentEntity>(CollectionOwnershipBuilder.OwnsMany(
                    typeof(TNewDependentEntity),
                    navigationExpression.GetPropertyAccess().GetSimpleMemberName(),
                    r => buildAction(Wrap<TDependentEntity, TNewDependentEntity>(r))));

            public override TestReferenceNavigationBuilder<TDependentEntity, TNewRelatedEntity> HasOne<TNewRelatedEntity>(
                Expression<Func<TDependentEntity, TNewRelatedEntity>> navigationExpression = null)
                => new NonGenericTestReferenceNavigationBuilder<TDependentEntity, TNewRelatedEntity>(
                    CollectionOwnershipBuilder.HasOne(typeof(TNewRelatedEntity), navigationExpression?.GetPropertyAccess().GetSimpleMemberName()));

            public override TestCollectionOwnershipBuilder<TEntity, TDependentEntity> HasChangeTrackingStrategy(
                ChangeTrackingStrategy changeTrackingStrategy)
                => Wrap<TEntity, TDependentEntity>(CollectionOwnershipBuilder.HasChangeTrackingStrategy(changeTrackingStrategy));

            public override TestCollectionOwnershipBuilder<TEntity, TDependentEntity> UsePropertyAccessMode(
                PropertyAccessMode propertyAccessMode)
                => Wrap<TEntity, TDependentEntity>(CollectionOwnershipBuilder.UsePropertyAccessMode(propertyAccessMode));

            public override DataBuilder<TDependentEntity> HasData(params TDependentEntity[] data)
            {
                CollectionOwnershipBuilder.HasData(data);
                return new DataBuilder<TDependentEntity>();
            }

            public override DataBuilder<TDependentEntity> HasData(params object[] data)
            {
                CollectionOwnershipBuilder.HasData(data);
                return new DataBuilder<TDependentEntity>();
            }

            CollectionOwnershipBuilder IInfrastructure<CollectionOwnershipBuilder>.Instance => CollectionOwnershipBuilder;
        }
    }
}
