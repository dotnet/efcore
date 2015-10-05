// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata.Conventions;
using Xunit;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;

namespace Microsoft.Data.Entity.Metadata.Internal.Test
{
    public class InternalModelBuilderTest
    {
        [Fact]
        public void Entity_returns_same_instance_for_entity_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            var entityBuilder = modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit);

            Assert.NotNull(entityBuilder);
            Assert.NotNull(model.FindEntityType(typeof(Customer)));
            Assert.Same(entityBuilder, modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));
        }

        [Fact]
        public void Entity_returns_same_instance_for_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            var entityBuilder = modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation);

            Assert.NotNull(entityBuilder);
            Assert.NotNull(model.FindEntityType(typeof(Customer).FullName));
            Assert.Same(entityBuilder, modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit));
        }

        [Fact]
        public void Can_ignore_lower_or_equal_source_entity_type_using_entity_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention);

            Assert.True(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.Null(model.FindEntityType(typeof(Customer)));
            Assert.True(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));
            Assert.Null(modelBuilder.Entity(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.NotNull(modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit));

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Explicit));
            Assert.Null(model.FindEntityType(typeof(Customer)));
        }

        [Fact]
        public void Can_ignore_lower_or_equal_source_entity_type_using_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);
            modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation);

            Assert.True(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));

            Assert.Null(model.FindEntityType(typeof(Customer).FullName));
            Assert.True(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.Explicit));
            Assert.Null(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));

            Assert.NotNull(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.Explicit));

            Assert.True(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.Explicit));
            Assert.Null(model.FindEntityType(typeof(Customer).FullName));
        }

        [Fact]
        public void Cannot_ignore_higher_source_entity_type_using_entity_clr_type()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            Assert.True(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));
            Assert.Null(modelBuilder.Entity(typeof(Customer), ConfigurationSource.DataAnnotation));
            Assert.NotNull(modelBuilder.Entity(typeof(Customer), ConfigurationSource.Explicit));

            Assert.False(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.NotNull(model.FindEntityType(typeof(Customer)));
        }

        [Fact]
        public void Cannot_ignore_higher_source_entity_type_using_entity_type_name()
        {
            var model = new Model();
            var modelBuilder = CreateModelBuilder(model);

            Assert.True(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.Convention));
            Assert.Null(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.Convention));
            Assert.NotNull(modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));

            Assert.False(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.Convention));

            Assert.NotNull(model.FindEntityType(typeof(Customer).FullName));
        }

        [Fact]
        public void Can_ignore_existing_entity_type_using_entity_clr_type()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer));
            var modelBuilder = CreateModelBuilder(model);
            Assert.Same(entityType, modelBuilder.Entity(typeof(Customer), ConfigurationSource.Convention).Metadata);
            Assert.False(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));
            Assert.NotNull(model.FindEntityType(typeof(Customer)));

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Explicit));

            Assert.Null(model.FindEntityType(typeof(Customer)));
        }

        [Fact]
        public void Can_ignore_existing_entity_type_using_entity_type_name()
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(Customer).FullName);
            var modelBuilder = CreateModelBuilder(model);

            Assert.Same(entityType, modelBuilder.Entity(typeof(Customer).FullName, ConfigurationSource.Convention).Metadata);
            Assert.False(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.DataAnnotation));
            Assert.NotNull(model.FindEntityType(typeof(Customer).FullName));

            Assert.NotNull(modelBuilder.Ignore(typeof(Customer).FullName, ConfigurationSource.Explicit));

            Assert.Null(model.FindEntityType(typeof(Customer).FullName));
        }

        [Fact]
        public void Can_ignore_entity_type_referenced_from_lower_or_equal_source_foreign_key()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Convention)
                .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
            var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Explicit);

            Assert.NotNull(orderEntityTypeBuilder.HasForeignKey(typeof(Customer), new[] { Order.CustomerIdProperty }, ConfigurationSource.DataAnnotation));

            Assert.True(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.Equal(typeof(Order), modelBuilder.Metadata.EntityTypes.Single().ClrType);
            Assert.Empty(orderEntityTypeBuilder.Metadata.GetForeignKeys());
        }

        [Fact]
        public void Cannot_ignore_entity_type_referenced_from_higher_source_foreign_key()
        {
            var modelBuilder = CreateModelBuilder();
            modelBuilder
                .Entity(typeof(Customer), ConfigurationSource.Convention)
                .PrimaryKey(new[] { Customer.IdProperty }, ConfigurationSource.Convention);
            var orderEntityTypeBuilder = modelBuilder.Entity(typeof(Order), ConfigurationSource.Convention);

            Assert.NotNull(orderEntityTypeBuilder.HasForeignKey(typeof(Customer), new[] { Order.CustomerIdProperty }, ConfigurationSource.Explicit));

            Assert.False(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

            Assert.Equal(2, modelBuilder.Metadata.EntityTypes.Count);
            Assert.Equal(1, orderEntityTypeBuilder.Metadata.GetForeignKeys().Count());
        }

        [Fact]
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
            orderEntityTypeBuilder.HasForeignKey(typeof(Customer), new[] { Order.CustomerIdProperty }, ConfigurationSource.Convention);
            orderEntityTypeBuilder.HasForeignKey(typeof(Product), new[] { Order.ProductIdProperty }, ConfigurationSource.Convention);

            Assert.True(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.Explicit));

            modelBuilder = new ModelCleanupConvention().Apply(modelBuilder);
            Assert.Empty(modelBuilder.Metadata.EntityTypes);
        }

        [Fact]
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
            orderEntityTypeBuilder.HasForeignKey(typeof(Product), new[] { Order.ProductIdProperty }, ConfigurationSource.Convention)
                .DependentToPrincipal("Product", ConfigurationSource.Convention);

            Assert.True(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

            modelBuilder = new ModelCleanupConvention().Apply(modelBuilder);
            Assert.Equal(new[] { typeof(Order), typeof(Product) }, modelBuilder.Metadata.EntityTypes.Select(et => et.ClrType));
            Assert.Equal(typeof(Product), orderEntityTypeBuilder.Metadata.GetForeignKeys().Single().PrincipalEntityType.ClrType);
        }

        [Fact]
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
            orderEntityTypeBuilder.HasForeignKey(typeof(Product), new[] { Order.ProductIdProperty }, ConfigurationSource.Convention)
                .PrincipalToDependent("Order", ConfigurationSource.Convention);

            Assert.True(modelBuilder.Ignore(typeof(Customer), ConfigurationSource.DataAnnotation));

            modelBuilder = new ModelCleanupConvention().Apply(modelBuilder);
            Assert.Equal(new[] { typeof(Order), typeof(Product) }, modelBuilder.Metadata.EntityTypes.Select(et => et.ClrType));
            Assert.Equal(typeof(Product), orderEntityTypeBuilder.Metadata.GetForeignKeys().Single().PrincipalEntityType.ClrType);
        }

        protected virtual InternalModelBuilder CreateModelBuilder(Model model = null)
            => new InternalModelBuilder(model ?? new Model(), new ConventionSet());

        private class Customer
        {
            public static readonly PropertyInfo IdProperty = typeof(Customer).GetProperty("Id");

            public int Id { get; set; }
            public string Name { get; set; }
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
        }

        private class Product
        {
            public static readonly PropertyInfo IdProperty = typeof(Product).GetProperty("Id");
            public int Id { get; set; }
            public Order Order { get; set; }
        }
    }
}
