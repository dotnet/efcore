// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Metadata.Builders
{
    public class SqliteBuilderExtensionsTest
    {
        [Fact]
        public void Can_set_column_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnName("MyNameIs")
                .Metadata;

            Assert.Equal("MyNameIs", property.Relational().ColumnName);
        }

        [Fact]
        public void Can_set_column_name_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Name")
                .HasColumnName("MyNameIs")
                .Metadata;

            Assert.Equal("MyNameIs", property.Relational().ColumnName);
        }

        [Fact]
        public void Can_set_column_type()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasColumnType("nvarchar(DA)")
                .Metadata;

            Assert.Equal("nvarchar(DA)", property.Relational().ColumnType);
        }

        [Fact]
        public void Can_set_column_type_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Name")
                .HasColumnType("nvarchar(DA)")
                .Metadata;

            Assert.Equal("nvarchar(DA)", property.Relational().ColumnType);
        }

        [Fact]
        public void Can_set_column_default_expression()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasDefaultValueSql("VanillaCoke")
                .Metadata;

            Assert.Equal("VanillaCoke", property.Relational().DefaultValueSql);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Can_set_column_default_expression_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Name")
                .HasDefaultValueSql("VanillaCoke")
                .Metadata;

            Assert.Equal("VanillaCoke", property.Relational().DefaultValueSql);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Setting_column_default_expression_does_not_modify_explicitly_set_value_generated()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ValueGeneratedNever()
                .HasDefaultValueSql("VanillaCoke")
                .Metadata;

            Assert.Equal("VanillaCoke", property.Relational().DefaultValueSql);
            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [Fact]
        public void Setting_column_default_expression_does_not_modify_explicitly_set_value_generated_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Name")
                .ValueGeneratedNever()
                .HasDefaultValueSql("VanillaCoke")
                .Metadata;

            Assert.Equal("VanillaCoke", property.Relational().DefaultValueSql);
            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [Fact]
        public void Can_set_column_default_value()
        {
            var modelBuilder = CreateConventionModelBuilder();
            var valueString = "DefaultValue";

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasDefaultValue(valueString)
                .Metadata;

            Assert.Equal(valueString, property.Relational().DefaultValue);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Can_set_column_default_value_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();
            var valueString = "DefaultValue";

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Name")
                .HasDefaultValue(valueString)
                .Metadata;

            Assert.Equal(valueString, property.Relational().DefaultValue);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Setting_column_default_value_does_not_modify_explicitly_set_value_generated()
        {
            var modelBuilder = CreateConventionModelBuilder();
            var valueString = "DefaultValue";

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ValueGeneratedNever()
                .HasDefaultValue(valueString)
                .Metadata;

            Assert.Equal(valueString, property.Relational().DefaultValue);
            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [Fact]
        public void Setting_column_default_value_does_not_modify_explicitly_set_value_generated_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();
            var valueString = "DefaultValue";

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Name")
                .ValueGeneratedNever()
                .HasDefaultValue(valueString)
                .Metadata;

            Assert.Equal(valueString, property.Relational().DefaultValue);
            Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        }

        [Fact]
        public void Can_set_key_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var key = modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .HasName("LemonSupreme")
                .Metadata;

            Assert.Equal("LemonSupreme", key.Relational().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var foreignKey = modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .HasConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Relational().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var foreignKey = modelBuilder
                .Entity<Customer>().HasMany(typeof(Order)).WithOne()
                .HasConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Relational().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var foreignKey = modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .HasConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Relational().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var foreignKey = modelBuilder
                .Entity<Order>().HasOne(typeof(Customer)).WithMany()
                .HasConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Relational().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var foreignKey = modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .HasConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Relational().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var foreignKey = modelBuilder
                .Entity<Order>().HasOne(typeof(OrderDetails)).WithOne()
                .HasConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Relational().Name);
        }

        [Fact]
        public void Can_set_index_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var index = modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .HasName("Dexter")
                .Metadata;

            Assert.Equal("Dexter", index.Relational().Name);
        }

        [Fact]
        public void Can_set_table_name()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>()
                .ToTable("Custardizer")
                .Metadata;

            Assert.Equal("Custardizer", entityType.Relational().TableName);
        }

        [Fact]
        public void Can_set_table_name_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var entityType = modelBuilder
                .Entity("Customer")
                .ToTable("Custardizer")
                .Metadata;

            Assert.Equal("Custardizer", entityType.Relational().TableName);
        }

        protected virtual ModelBuilder CreateConventionModelBuilder()
        {
            return SqliteTestHelpers.Instance.CreateConventionBuilder();
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public IEnumerable<Order> Orders { get; set; }
        }

        private class Order
        {
            public Customer Customer { get; set; }
            public OrderDetails Details { get; set; }
        }

        private class OrderDetails
        {
            public Order Order { get; set; }
        }
    }
}
