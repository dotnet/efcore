// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
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
                .HasSqliteColumnName("MyNameIs")
                .Metadata;

            Assert.Equal("MyNameIs", property.Sqlite().Column);
        }

        [Fact]
        public void Can_set_column_name_non_generic()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Name")
                .HasSqliteColumnName("MyNameIs")
                .Metadata;

            Assert.Equal("MyNameIs", property.Sqlite().Column);
        }

        [Fact]
        public void Can_set_column_type()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .HasSqliteColumnType("nvarchar(DA)")
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
                .HasSqliteColumnType("nvarchar(DA)")
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
                .SqliteDefaultValueSql("VanillaCoke")
                .Metadata;

            Assert.Equal("VanillaCoke", property.Sqlite().DefaultValueSql);
        }

        [Fact]
        public void Can_set_column_default_expression_non_generic()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property<string>("Name")
                .SqliteDefaultValueSql("VanillaCoke")
                .Metadata;

            Assert.Equal("VanillaCoke", property.Sqlite().DefaultValueSql);
        }

        [Fact]
        public void Can_set_key_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var key = modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .SqliteKeyName("LemonSupreme")
                .Metadata;

            Assert.Equal("LemonSupreme", key.Sqlite().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var foreignKey = modelBuilder
                .Entity<Customer>().Collection(e => e.Orders).InverseReference(e => e.Customer)
                .SqliteConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Sqlite().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_many_non_generic()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var foreignKey = modelBuilder
                .Entity<Customer>().Collection(typeof(Order)).InverseReference()
                .SqliteConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Sqlite().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var foreignKey = modelBuilder
                .Entity<Order>().Reference(e => e.Customer).InverseCollection(e => e.Orders)
                .SqliteConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Sqlite().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_many_to_one_non_generic()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var foreignKey = modelBuilder
                .Entity<Order>().Reference(typeof(Customer)).InverseCollection()
                .SqliteConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Sqlite().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var foreignKey = modelBuilder
                .Entity<Order>().Reference(e => e.Details).InverseReference(e => e.Order)
                .SqliteConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Sqlite().Name);
        }

        [Fact]
        public void Can_set_foreign_key_name_for_one_to_one_non_generic()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var foreignKey = modelBuilder
                .Entity<Order>().Reference(typeof(OrderDetails)).InverseReference()
                .SqliteConstraintName("ChocolateLimes")
                .Metadata;

            Assert.Equal("ChocolateLimes", foreignKey.Sqlite().Name);
        }

        [Fact]
        public void Can_set_index_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var index = modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .SqliteIndexName("Dexter")
                .Metadata;

            Assert.Equal("Dexter", index.Sqlite().Name);
        }

        [Fact]
        public void Can_set_table_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .ToSqliteTable("Custardizer")
                .Metadata;

            Assert.Equal("Custardizer", entityType.Sqlite().Table);
        }

        [Fact]
        public void Can_set_table_name_non_generic()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity("Customer")
                .ToSqliteTable("Custardizer")
                .Metadata;

            Assert.Equal("Custardizer", entityType.Sqlite().Table);
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
