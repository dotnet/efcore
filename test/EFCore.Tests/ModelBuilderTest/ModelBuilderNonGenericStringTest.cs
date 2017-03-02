// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Xunit;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.Tests
{
    public class ModelBuilderNonGenericStringTest : ModelBuilderNonGenericTest
    {
        public class NonGenericStringOneToManyType : OneToManyTestBase
        {
            [Fact]
            public virtual void Can_create_shadow_navigations_between_shadow_entity_types()
            {
                var modelBuilder = (NonGenericStringTestModelBuilder)CreateModelBuilder();
                var foreignKey = modelBuilder.Entity("Order")
                    .HasOne("Customer", "Customer")
                    .WithMany("Orders")
                    .Metadata;

                Assert.Equal("Customer", modelBuilder.Model.FindEntityType("Order")?.GetNavigations().Single().Name);
                Assert.Equal("Orders", modelBuilder.Model.FindEntityType("Customer")?.GetNavigations().Single().Name);
                Assert.False(foreignKey.IsUnique);

                Assert.Equal(
                    CoreStrings.ShadowEntity("Customer"),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.Validate()).Message);
            }

            [Fact]
            public virtual void Cannot_create_navigation_on_non_shadow_entity_targeting_shadow_entity()
            {
                var modelBuilder = (NonGenericStringTestModelBuilder)CreateModelBuilder();
                var orderEntityType = modelBuilder.Entity(typeof(Order));

                Assert.Equal(
                    CoreStrings.NavigationToShadowEntity("Customer", typeof(Order).ShortDisplayName(), "Customer"),
                    Assert.Throws<InvalidOperationException>(() => orderEntityType.HasOne("Customer", "Customer")).Message);
            }

            [Fact]
            public virtual void Cannot_create_shadow_navigation_between_non_shadow_entity_types()
            {
                var modelBuilder = (NonGenericStringTestModelBuilder)CreateModelBuilder();
                var orderEntityType = modelBuilder.Entity(typeof(Order));

                Assert.Equal(
                    CoreStrings.NoClrNavigation("CustomerNavigation", typeof(Order).ShortDisplayName()),
                    Assert.Throws<InvalidOperationException>(() => orderEntityType.HasOne(typeof(Customer), "CustomerNavigation")).Message);
            }

            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder) => new NonGenericStringTestModelBuilder(modelBuilder);
        }

        public class NonGenericStringManyToOneType : ManyToOneTestBase
        {
            [Fact]
            public virtual void Can_create_shadow_navigations_between_shadow_entity_types()
            {
                var modelBuilder = (NonGenericStringTestModelBuilder)CreateModelBuilder();
                var foreignKey = modelBuilder.Entity("Customer")
                    .HasMany("Order", "Orders")
                    .WithOne("Customer")
                    .Metadata;

                Assert.Equal("Customer", modelBuilder.Model.FindEntityType("Order")?.GetNavigations().Single().Name);
                Assert.Equal("Orders", modelBuilder.Model.FindEntityType("Customer")?.GetNavigations().Single().Name);
                Assert.False(foreignKey.IsUnique);

                Assert.Equal(
                    CoreStrings.ShadowEntity("Customer"),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.Validate()).Message);
            }

            [Fact]
            public virtual void Cannot_create_navigation_on_non_shadow_entity_targeting_shadow_entity()
            {
                var modelBuilder = (NonGenericStringTestModelBuilder)CreateModelBuilder();
                var customerEntityType = modelBuilder.Entity(typeof(Customer));

                Assert.Equal(
                    CoreStrings.NavigationToShadowEntity("Orders", typeof(Customer).ShortDisplayName(), "Order"),
                    Assert.Throws<InvalidOperationException>(() => customerEntityType.HasMany("Order", "Orders")).Message);
            }

            [Fact]
            public virtual void Cannot_create_shadow_navigation_between_non_shadow_entity_types()
            {
                var modelBuilder = (NonGenericStringTestModelBuilder)CreateModelBuilder();
                var customerEntityType = modelBuilder.Entity(typeof(Customer));

                Assert.Equal(
                    CoreStrings.NoClrNavigation("OrdersNavigation", typeof(Customer).ShortDisplayName()),
                    Assert.Throws<InvalidOperationException>(() => customerEntityType.HasMany(typeof(Order), "OrdersNavigation")).Message);
            }

            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder) => new NonGenericStringTestModelBuilder(modelBuilder);
        }

        public class NonGenericStringOneToOneType : OneToOneTestBase
        {
            [Fact]
            public virtual void Can_create_shadow_navigations_between_shadow_entity_types()
            {
                var modelBuilder = (NonGenericStringTestModelBuilder)CreateModelBuilder();
                var foreignKey = modelBuilder.Entity("Order")
                    .HasOne("OrderDetails", "OrderDetails")
                    .WithOne("Order")
                    .HasForeignKey("OrderDetails", "OrderId")
                    .Metadata;

                Assert.Equal("OrderDetails", modelBuilder.Model.FindEntityType("Order")?.GetNavigations().Single().Name);
                Assert.Equal("Order", modelBuilder.Model.FindEntityType("OrderDetails")?.GetNavigations().Single().Name);
                Assert.True(foreignKey.IsUnique);

                Assert.Equal(
                    CoreStrings.ShadowEntity("Order"),
                    Assert.Throws<InvalidOperationException>(() => modelBuilder.Validate()).Message);
            }

            [Fact]
            public virtual void Cannot_create_navigation_on_non_shadow_entity_targeting_shadow_entity()
            {
                var modelBuilder = (NonGenericStringTestModelBuilder)CreateModelBuilder();
                var orderEntityType = modelBuilder.Entity(typeof(Order));

                Assert.Equal(
                    CoreStrings.NavigationToShadowEntity(nameof(Order.Details), typeof(Order).ShortDisplayName(), nameof(OrderDetails)),
                    Assert.Throws<InvalidOperationException>(() => orderEntityType.HasOne(nameof(OrderDetails), nameof(Order.Details))).Message);
            }

            [Fact]
            public virtual void Cannot_create_shadow_navigation_between_non_shadow_entity_types()
            {
                var modelBuilder = (NonGenericStringTestModelBuilder)CreateModelBuilder();
                var orderEntityType = modelBuilder.Entity(typeof(Order));

                Assert.Equal(
                    CoreStrings.NoClrNavigation("OrderDetailsNavigation", typeof(Order).ShortDisplayName()),
                    Assert.Throws<InvalidOperationException>(() => orderEntityType.HasOne(typeof(OrderDetails), "OrderDetailsNavigation")).Message);
            }

            protected override TestModelBuilder CreateTestModelBuilder(ModelBuilder modelBuilder) => new NonGenericStringTestModelBuilder(modelBuilder);
        }

        private class NonGenericStringTestModelBuilder : TestModelBuilder
        {
            public NonGenericStringTestModelBuilder(ModelBuilder modelBuilder)
                : base(modelBuilder)
            {
            }

            public override TestEntityTypeBuilder<TEntity> Entity<TEntity>()
                => new NonGenericStringTestEntityTypeBuilder<TEntity>(ModelBuilder.Entity(typeof(TEntity)));

            public NonGenericStringTestEntityTypeBuilder Entity(Type type)
                => new NonGenericStringTestEntityTypeBuilder(ModelBuilder.Entity(type));

            public NonGenericStringTestEntityTypeBuilder Entity(string name)
                => new NonGenericStringTestEntityTypeBuilder(ModelBuilder.Entity(name));

            public override TestModelBuilder Entity<TEntity>(Action<TestEntityTypeBuilder<TEntity>> buildAction)
                => new NonGenericStringTestModelBuilder(ModelBuilder.Entity(typeof(TEntity), entityTypeBuilder =>
                        buildAction(new NonGenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder))));

            public override TestModelBuilder Ignore<TEntity>()
                => new NonGenericStringTestModelBuilder(ModelBuilder.Ignore(typeof(TEntity)));

            public override string GetDisplayName(Type entityType) => entityType.FullName;
        }

        private class NonGenericStringTestEntityTypeBuilder<TEntity> : NonGenericTestEntityTypeBuilder<TEntity>
            where TEntity : class
        {
            public NonGenericStringTestEntityTypeBuilder(EntityTypeBuilder entityTypeBuilder)
                : base(entityTypeBuilder)
            {
            }

            protected override NonGenericTestEntityTypeBuilder<TEntity> Wrap(EntityTypeBuilder entityTypeBuilder)
                => new NonGenericStringTestEntityTypeBuilder<TEntity>(entityTypeBuilder);

            public override TestEntityTypeBuilder<TEntity> HasBaseType<TBaseEntity>()
                => Wrap(EntityTypeBuilder.HasBaseType(typeof(TBaseEntity)));

            public override TestReferenceNavigationBuilder<TEntity, TRelatedEntity> HasOne<TRelatedEntity>(Expression<Func<TEntity, TRelatedEntity>> reference = null)
                => new NonGenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasOne(typeof(TRelatedEntity).FullName, reference?.GetPropertyAccess().Name));

            public override TestCollectionNavigationBuilder<TEntity, TRelatedEntity> HasMany<TRelatedEntity>(Expression<Func<TEntity, IEnumerable<TRelatedEntity>>> collection = null)
                => new NonGenericTestCollectionNavigationBuilder<TEntity, TRelatedEntity>(EntityTypeBuilder.HasMany(typeof(TRelatedEntity).FullName, collection?.GetPropertyAccess().Name));
        }

        private class NonGenericStringTestEntityTypeBuilder
        {
            public NonGenericStringTestEntityTypeBuilder(EntityTypeBuilder entityTypeBuilder)
            {
                EntityTypeBuilder = entityTypeBuilder;
            }

            private EntityTypeBuilder EntityTypeBuilder { get; }

            public NonGenericStringTestReferenceNavigationBuilder HasOne(string relatedTypeName, string navigationName)
                => new NonGenericStringTestReferenceNavigationBuilder(EntityTypeBuilder.HasOne(relatedTypeName, navigationName));

            public NonGenericStringTestCollectionNavigationBuilder HasMany(string relatedTypeName, string navigationName)
                => new NonGenericStringTestCollectionNavigationBuilder(EntityTypeBuilder.HasMany(relatedTypeName, navigationName));

            public NonGenericStringTestReferenceNavigationBuilder HasOne(Type relatedType, string navigationName)
                => new NonGenericStringTestReferenceNavigationBuilder(EntityTypeBuilder.HasOne(relatedType, navigationName));

            public NonGenericStringTestCollectionNavigationBuilder HasMany(Type relatedType, string navigationName)
                => new NonGenericStringTestCollectionNavigationBuilder(EntityTypeBuilder.HasMany(relatedType, navigationName));
        }

        private class NonGenericStringTestReferenceNavigationBuilder<TEntity, TRelatedEntity> : NonGenericTestReferenceNavigationBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public NonGenericStringTestReferenceNavigationBuilder(ReferenceNavigationBuilder referenceNavigationBuilder)
                : base(referenceNavigationBuilder)
            {
            }

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> WithOne(Expression<Func<TRelatedEntity, TEntity>> reference = null)
            {
                var referenceName = reference?.GetPropertyAccess().Name;
                return new NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(ReferenceNavigationBuilder.WithOne(referenceName));
            }
        }

        private class NonGenericStringTestReferenceNavigationBuilder
        {
            public NonGenericStringTestReferenceNavigationBuilder(ReferenceNavigationBuilder referenceNavigationBuilder)
            {
                ReferenceNavigationBuilder = referenceNavigationBuilder;
            }

            private ReferenceNavigationBuilder ReferenceNavigationBuilder { get; }

            public NonGenericStringTestReferenceCollectionBuilder WithMany(string collection = null)
                => new NonGenericStringTestReferenceCollectionBuilder(ReferenceNavigationBuilder.WithMany(collection));

            public NonGenericStringTestReferenceReferenceBuilder WithOne(string reference = null)
                => new NonGenericStringTestReferenceReferenceBuilder(ReferenceNavigationBuilder.WithOne(reference));
        }

        private class NonGenericStringTestCollectionNavigationBuilder
        {
            public NonGenericStringTestCollectionNavigationBuilder(CollectionNavigationBuilder collectionNavigationBuilder)
            {
                CollectionNavigationBuilder = collectionNavigationBuilder;
            }

            private CollectionNavigationBuilder CollectionNavigationBuilder { get; }

            public NonGenericStringTestReferenceCollectionBuilder WithOne(string reference = null)
                => new NonGenericStringTestReferenceCollectionBuilder(CollectionNavigationBuilder.WithOne(reference));
        }

        private class NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity> : NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity>
            where TEntity : class
            where TRelatedEntity : class
        {
            public NonGenericStringTestReferenceReferenceBuilder(ReferenceReferenceBuilder referenceReferenceBuilder)
                : base(referenceReferenceBuilder)
            {
            }

            protected override NonGenericTestReferenceReferenceBuilder<TEntity, TRelatedEntity> Wrap(ReferenceReferenceBuilder referenceReferenceBuilder)
                => new NonGenericStringTestReferenceReferenceBuilder<TEntity, TRelatedEntity>(referenceReferenceBuilder);

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasForeignKey<TDependentEntity>(Expression<Func<TDependentEntity, object>> foreignKeyExpression)
                => Wrap(ReferenceReferenceBuilder.HasForeignKey(typeof(TDependentEntity).FullName, foreignKeyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));

            public override TestReferenceReferenceBuilder<TEntity, TRelatedEntity> HasPrincipalKey<TPrincipalEntity>(Expression<Func<TPrincipalEntity, object>> keyExpression)
                => Wrap(ReferenceReferenceBuilder.HasPrincipalKey(typeof(TPrincipalEntity).FullName, keyExpression.GetPropertyAccessList().Select(p => p.Name).ToArray()));
        }

        private class NonGenericStringTestReferenceReferenceBuilder
        {
            public NonGenericStringTestReferenceReferenceBuilder(ReferenceReferenceBuilder referenceReferenceBuilder)
            {
                ReferenceReferenceBuilder = referenceReferenceBuilder;
            }

            public NonGenericStringTestReferenceReferenceBuilder HasForeignKey(
                    string dependentEntityTypeName, params string[] foreignKeyPropertyNames)
                => new NonGenericStringTestReferenceReferenceBuilder(
                    ReferenceReferenceBuilder.HasForeignKey(dependentEntityTypeName, foreignKeyPropertyNames));

            public NonGenericStringTestReferenceReferenceBuilder HasPrincipalKey(
                    string principalEntityTypeName, params string[] keyPropertyNames)
                => new NonGenericStringTestReferenceReferenceBuilder
                    (ReferenceReferenceBuilder.HasPrincipalKey(principalEntityTypeName, keyPropertyNames));

            private ReferenceReferenceBuilder ReferenceReferenceBuilder { get; }
            public IMutableForeignKey Metadata => ReferenceReferenceBuilder.Metadata;
        }

        private class NonGenericStringTestReferenceCollectionBuilder
        {
            public NonGenericStringTestReferenceCollectionBuilder(ReferenceCollectionBuilder referenceCollectionBuilder)
            {
                ReferenceCollectionBuilder = referenceCollectionBuilder;
            }

            private ReferenceCollectionBuilder ReferenceCollectionBuilder { get; }
            public IMutableForeignKey Metadata => ReferenceCollectionBuilder.Metadata;
        }
    }
}
