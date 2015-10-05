// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata.Conventions;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Metadata.Builders
{
    public class SqliteBuilderExtensionsTest
    {
        [Fact]
        public void Can_set_column_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqliteHasColumnName("MyNameIs")
                .Metadata;

            Assert.Equal("MyNameIs", property.Sqlite().ColumnName);
        }

        [Fact]
        public void Can_set_column_name_non_generic()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Name")
                .ForSqliteHasColumnName("MyNameIs")
                .Metadata;

            Assert.Equal("MyNameIs", property.Sqlite().ColumnName);
        }

        [Fact]
        public void Can_set_column_type()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqliteHasColumnType("nvarchar(DA)")
                .Metadata;

            Assert.Equal("nvarchar(DA)", property.Sqlite().ColumnType);
        }

        [Fact]
        public void Can_set_column_type_non_generic()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Name")
                .ForSqliteHasColumnType("nvarchar(DA)")
                .Metadata;

            Assert.Equal("nvarchar(DA)", property.Sqlite().ColumnType);
        }

        [Fact]
        public void Can_set_column_default_expression()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .ForSqliteHasDefaultValueSql("VanillaCoke")
                .Metadata;

            Assert.Equal("VanillaCoke", property.Sqlite().GeneratedValueSql);
        }

        [Fact]
        public void Can_set_column_default_expression_non_generic()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Name")
                .ForSqliteHasDefaultValueSql("VanillaCoke")
                .Metadata;

            Assert.Equal("VanillaCoke", property.Sqlite().GeneratedValueSql);
        }

        [Fact]
        public void Can_set_key_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var key = modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .ForSqliteHasName("LemonSupreme")
                .Metadata;

            Assert.Equal("LemonSupreme", key.Sqlite().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var foreignKey = modelBuilder
                .Entity<Customer>().HasMany(e => e.Orders).WithOne(e => e.Customer)
                .ForSqliteHasConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Sqlite().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_non_generic()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var foreignKey = modelBuilder
                .Entity<Customer>().HasMany(typeof(Order)).WithOne()
                .ForSqliteHasConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Sqlite().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var foreignKey = modelBuilder
                .Entity<Order>().HasOne(e => e.Customer).WithMany(e => e.Orders)
                .ForSqliteHasConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Sqlite().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_non_generic()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var foreignKey = modelBuilder
                .Entity<Order>().HasOne(typeof(Customer)).WithMany()
                .ForSqliteHasConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Sqlite().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var foreignKey = modelBuilder
                .Entity<Order>().HasOne(e => e.Details).WithOne(e => e.Order)
                .ForSqliteHasConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Sqlite().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_non_generic()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var foreignKey = modelBuilder
                .Entity<Order>().HasOne(typeof(OrderDetails)).WithOne()
                .ForSqliteHasConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Sqlite().Name);
        }

        [Fact]
        public void Can_set_index_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var index = modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .ForSqliteHasName("Dexter")
                .Metadata;

            Assert.Equal("Dexter", index.Sqlite().Name);
        }

        [Fact]
        public void Can_set_table_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .ForSqliteToTable("Custardizer")
                .Metadata;

            Assert.Equal("Custardizer", entityType.Sqlite().TableName);
        }

        [Fact]
        public void Can_set_table_name_non_generic()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity("Customer")
                .ForSqliteToTable("Custardizer")
                .Metadata;

            Assert.Equal("Custardizer", entityType.Sqlite().TableName);
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
