// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable UnusedMember.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class InternalModelBuilderTest
    {
        [ConditionalFact]
        public void Entity_returns_same_instance_for_entity_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder);
            Assert.NotNull(model.FindEntityType(typeof(Customer)));
            Assert.Same(entityBuilder, modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));
        }

        [ConditionalFact]
        public void Entity_returns_same_instance_for_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            var entityBuilder = modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation);

            Assert.NotNull(entityBuilder);
            Assert.NotNull(model.FindEntityType(typeof(Customer).FullName));
            Assert.Same(entityBuilder, modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit));
        }

        [ConditionalFact]
        public void Can_ignore_lower_or_equal_source_entity_type_using_entity_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.Null(model.FindEntityType(typeof(Customer)));
            Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));
            Assert.Null(modelBuilder.Entity(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.NotNull(modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit));

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Explicit));
            Assert.Null(model.FindEntityType(typeof(Customer)));
        }

        [ConditionalFact]
        public void Can_ignore_lower_or_equal_source_entity_type_using_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation);

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));

            Assert.Null(model.FindEntityType(typeof(Customer).FullName));
            Assert.NotNull(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.Explicit));
            Assert.Null(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));

            Assert.NotNull(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.Explicit));

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.Explicit));
            Assert.Null(model.FindEntityType(typeof(Customer).FullName));
        }

        [ConditionalFact]
        public void Cannot_ignore_higher_source_entity_type_using_entity_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));
            Assert.Null(modelBuilder.Entity(typeof(Customer), ConfigurationSource.DataAnnotation));
            Assert.NotNull(modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit));

            Assert.Null(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.NotNull(model.FindEntityType(typeof(Customer)));
        }

        [ConditionalFact]
        public void Cannot_ignore_higher_source_entity_type_using_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.Convention));
            Assert.Null(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.Convention));
            Assert.NotNull(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));

            Assert.Null(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.Convention));

            Assert.NotNull(model.FindEntityType(typeof(Customer).FullName));
        }

        [ConditionalFact]
        public void Can_ignore_existing_entity_type_using_entity_clr_type()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer), ConfigurationSource.Explicit);
            var modelBuilder = CreateModelBuilder(model);
            Assert.Same(entityType, modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention).Metadata);
            Assert.Null(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));
            Assert.NotNull(model.FindEntityType(typeof(Customer)));

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Explicit));

            Assert.Null(model.FindEntityType(typeof(Customer)));
        }

        [ConditionalFact]
        public void Can_ignore_existing_entity_type_using_entity_type_name()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer).FullName, ConfigurationSource.Explicit);
            var modelBuilder = CreateModelBuilder(model);

            Assert.Same(entityType, modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.Convention).Metadata);
            Assert.Null(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));
            Assert.NotNull(model.FindEntityType(typeof(Customer).FullName));

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.Explicit));

            Assert.Null(model.FindEntityType(typeof(Customer).FullName));
        }

        [ConditionalFact]
        public void Can_ignore_entity_type_referenced_from_lower_or_equal_source_foreign_key()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Convention)
                .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
            var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(
                orderEntityTypeBuilder.HasRelationship(
                    typeof(Customer), new[] { Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.Equal(typeof(Order), modelBuilder.Metadata.GetEntityTypes().Single().ClrType);
            Assert.Empty(orderEntityTypeBuilder.Metadata.GetForeignKeys());
        }

        [ConditionalFact]
        public void Can_ignore_entity_type_referencing_higher_or_equal_source_foreign_key()
        {
            var modelBuilder = CreateModelBuilder();
            var customerEntityTypeBuilder = modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Explicit)
                .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
            var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);

            Assert.NotNull(
                orderEntityTypeBuilder
                    .HasRelationship(typeof(Customer), new[] { Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

            Assert.NotNull(modelBuilder.Ignore(typeof(Order), ConfigurationSource.DataAnnotation));

            Assert.Equal(typeof(Customer), modelBuilder.Metadata.GetEntityTypes().Single().ClrType);
            Assert.Empty(customerEntityTypeBuilder.Metadata.GetReferencingForeignKeys());
        }

        [ConditionalFact]
        public void Can_ignore_entity_type_with_base_and_derived_types()
        {
            var modelBuilder = CreateModelBuilder();
            var baseEntityTypeBuilder = modelBuilder.Entity(typeof(Base), ConfigurationSource.Explicit);
            var customerEntityTypeBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);
            var specialCustomerEntityTypeBuilder = modelBuilder.Entity(typeof(SpecialCustomer), ConfigurationSource.Explicit);

            Assert.NotNull(customerEntityTypeBuilder.HasBaseType(baseEntityTypeBuilder.Metadata, ConfigurationSource.Convention));
            Assert.NotNull(
                specialCustomerEntityTypeBuilder.HasBaseType(customerEntityTypeBuilder.Metadata, ConfigurationSource.Convention));

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.Equal(2, modelBuilder.Metadata.GetEntityTypes().Count());
            Assert.Same(baseEntityTypeBuilder.Metadata, specialCustomerEntityTypeBuilder.Metadata.BaseType);
        }

        [ConditionalFact]
        public void Cannot_ignore_entity_type_referenced_from_higher_source_foreign_key()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Convention)
                .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
            var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);

            Assert.NotNull(
                orderEntityTypeBuilder.HasRelationship(typeof(Customer), new[] { Order.CustomerIdProperty }, ConfigurationSource.Explicit));

            Assert.Null(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.Equal(2, modelBuilder.Metadata.GetEntityTypes().Count());
            Assert.Single(orderEntityTypeBuilder.Metadata.GetForeignKeys());
        }

        [ConditionalFact]
        public void Ignoring_an_entity_type_removes_lower_source_orphaned_entity_types()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Convention)
                .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
            modelBuilder
                .Entity(typeof(Product), ConfigurationSource.Convention)
                .PrimaryKey(new[] { Product.IdProperty }, ConfigurationSource.Convention);

            var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);
            orderEntityTypeBuilder.HasRelationship(typeof(Customer), new[] { Order.CustomerIdProperty }, ConfigurationSource.Convention);
            orderEntityTypeBuilder.HasRelationship(typeof(Product), new[] { Order.ProductIdProperty }, ConfigurationSource.Convention);

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Explicit));

            Cleanup(modelBuilder);
            Assert.Empty(modelBuilder.Metadata.GetEntityTypes());
        }

        [ConditionalFact]
        public void Ignoring_an_entity_type_does_not_remove_referenced_lower_source_entity_types()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Convention)
                .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
            modelBuilder
                .Entity(typeof(Product), ConfigurationSource.Convention)
                .PrimaryKey(new[] { Product.IdProperty }, ConfigurationSource.Convention);

            var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);
            orderEntityTypeBuilder.HasRelationship(typeof(Product), new[] { Order.ProductIdProperty }, ConfigurationSource.Convention)
                .HasNavigation(
                    "Product",
                    pointsToPrincipal: true,
                    ConfigurationSource.Convention);

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

            Cleanup(modelBuilder);
            Assert.Equal(new[] { typeof(Order), typeof(Product) }, modelBuilder.Metadata.GetEntityTypes().Select(et => et.ClrType));
            Assert.Equal(typeof(Product), orderEntityTypeBuilder.Metadata.GetForeignKeys().Single().PrincipalEntityType.ClrType);
        }

        [ConditionalFact]
        public void Ignoring_an_entity_type_does_not_remove_referencing_lower_source_entity_types()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Convention)
                .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
            modelBuilder
                .Entity(typeof(Product), ConfigurationSource.Explicit)
                .PrimaryKey(new[] { Product.IdProperty }, ConfigurationSource.Convention);

            var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);
            orderEntityTypeBuilder.HasRelationship(typeof(Product), new[] { Order.ProductIdProperty }, ConfigurationSource.Convention)
                .HasNavigation(
                    "Order",
                    pointsToPrincipal: false,
                    ConfigurationSource.Convention);

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

            Cleanup(modelBuilder);
            Assert.Equal(new[] { typeof(Order), typeof(Product) }, modelBuilder.Metadata.GetEntityTypes().Select(et => et.ClrType));
            Assert.Equal(typeof(Product), orderEntityTypeBuilder.Metadata.GetForeignKeys().Single().PrincipalEntityType.ClrType);
        }

        [ConditionalFact]
        public void Can_mark_type_as_owned_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);

            Assert.NotNull(modelBuilder.Entity(typeof(Details), ConfigurationSource.Convention));

            Assert.False(model.IsOwned(typeof(Details)));

            Assert.NotNull(entityBuilder.HasOwnership(typeof(Details), nameof(Customer.Details), ConfigurationSource.Convention));

            Assert.NotNull(modelBuilder.Ignore(typeof(Details), ConfigurationSource.Convention));

            Assert.Empty(model.GetEntityTypes(typeof(Details)));

            Assert.Null(entityBuilder.HasOwnership(typeof(Details), nameof(Customer.Details), ConfigurationSource.Convention));

            Assert.Null(modelBuilder.Owned(typeof(Details), ConfigurationSource.Convention));

            Assert.NotNull(entityBuilder.HasOwnership(typeof(Details), nameof(Customer.Details), ConfigurationSource.DataAnnotation));

            Assert.NotNull(modelBuilder.Owned(typeof(Details), ConfigurationSource.Convention));

            Assert.NotNull(modelBuilder.Owned(typeof(Details), ConfigurationSource.DataAnnotation));

            Assert.True(model.IsOwned(typeof(Details)));

            Assert.NotNull(
                modelBuilder.Entity(typeof(Product), ConfigurationSource.Explicit)
                    .HasOwnership(typeof(Details), nameof(Product.Details), ConfigurationSource.Convention));

            Assert.Null(modelBuilder.Ignore(typeof(Details), ConfigurationSource.Convention));

            Assert.Equal(2, model.GetEntityTypes(typeof(Details)).Count);

            Assert.Equal(
                CoreStrings.ClashingOwnedEntityType(typeof(Details).Name),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Entity(typeof(Details), ConfigurationSource.Explicit)).Message);

            Assert.NotNull(modelBuilder.Ignore(typeof(Details), ConfigurationSource.Explicit));

            Assert.False(model.IsOwned(typeof(Details)));

            Assert.NotNull(modelBuilder.Entity(typeof(Details), ConfigurationSource.Explicit));

            Assert.Empty(model.GetEntityTypes(typeof(Details)).Where(e => e.DefiningNavigationName != null));

            Assert.Null(modelBuilder.Owned(typeof(Details), ConfigurationSource.Convention));

            Assert.Equal(
                CoreStrings.ClashingNonOwnedEntityType(typeof(Details).Name),
                Assert.Throws<InvalidOperationException>(() => modelBuilder.Owned(typeof(Details), ConfigurationSource.Explicit)).Message);
        }

        private static void Cleanup(InternalModelBuilder modelBuilder)
        {
            new ModelCleanupConvention(CreateDependencies())
                .ProcessModelFinalized(
                    modelBuilder,
                    new ConventionContext<IConventionModelBuilder>(modelBuilder.Metadata.ConventionDispatcher));
        }

        private static ProviderConventionSetBuilderDependencies CreateDependencies()
            => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

        protected virtual InternalModelBuilder CreateModelBuilder(Model model = null)
            => new InternalModelBuilder(model ?? new Model());

        private class Base
        {
            public int Id { get; set; }
        }

        private class Customer : Base
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");

            public string Name { get; set; }
            public Details Details { get; set; }
        }

        private class SpecialCustomer : Customer
        {
        }

        private class Order
        {
            public static readonly PropertyInfo IdProperty = typeof(Order).GetProperty("Id");
            public static readonly PropertyInfo CustomerIdProperty = typeof(Order).GetProperty("CustomerId");
            public static readonly PropertyInfo ProductIdProperty = typeof(Order).GetProperty("ProductId");

            public int Id { get; set; }
            public int CustomerId { get; set; }
            public Customer Customer { get; set; }
            public int ProductId { get; set; }
            public Product Product { get; set; }
            public Details Details { get; set; }
        }

        private class Product
        {
            public static readonly PropertyInfo IdProperty = typeof(Product).GetProperty("Id");
            public int Id { get; set; }
            public Order Order { get; set; }
            public Details Details { get; set; }
        }

        private class Details
        {
            public string Name { get; set; }
        }
    }
}
