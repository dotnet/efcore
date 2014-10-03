// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Metadata.Tests
{
    public class RelationalMetadataExtensionsTest
    {
        [Fact]
        public void Can_get_and_set_column_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal("Name", property.Relational().Column);
            Assert.Equal("Name", ((IProperty)property).Relational().Column);

            property.Relational().Column = "Eman";

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", ((IProperty)property).Name);
            Assert.Equal("Eman", property.Relational().Column);
            Assert.Equal("Eman", ((IProperty)property).Relational().Column);

            property.Relational().Column = null;

            Assert.Equal("Name", property.Relational().Column);
            Assert.Equal("Name", ((IProperty)property).Relational().Column);
        }

        [Fact]
        public void Can_get_and_set_table_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Equal("Customer", entityType.Relational().Table);
            Assert.Equal("Customer", ((IEntityType)entityType).Relational().Table);

            entityType.Relational().Table = "Customizer";

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customer", ((IEntityType)entityType).SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).Relational().Table);

            entityType.Relational().Table = null;

            Assert.Equal("Customer", entityType.Relational().Table);
            Assert.Equal("Customer", ((IEntityType)entityType).Relational().Table);
        }

        [Fact]
        public void Can_get_and_set_schema_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Null(entityType.Relational().Schema);
            Assert.Null(((IEntityType)entityType).Relational().Schema);

            entityType.Relational().Schema = "db0";

            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).Relational().Schema);

            entityType.Relational().Schema = null;

            Assert.Null(entityType.Relational().Schema);
            Assert.Null(((IEntityType)entityType).Relational().Schema);
        }

        [Fact]
        public void Can_get_and_set_column_type()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().ColumnType);
            Assert.Null(((IProperty)property).Relational().ColumnType);

            property.Relational().ColumnType = "nvarchar(max)";

            Assert.Equal("nvarchar(max)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).Relational().ColumnType);

            property.Relational().ColumnType = null;

            Assert.Null(property.Relational().ColumnType);
            Assert.Null(((IProperty)property).Relational().ColumnType);
        }

        [Fact]
        public void Can_get_and_set_column_default_expression()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().DefaultExpression);
            Assert.Null(((IProperty)property).Relational().DefaultExpression);

            property.Relational().DefaultExpression = "newsequentialid()";

            Assert.Equal("newsequentialid()", property.Relational().DefaultExpression);
            Assert.Equal("newsequentialid()", ((IProperty)property).Relational().DefaultExpression);

            property.Relational().DefaultExpression = null;

            Assert.Null(property.Relational().DefaultExpression);
            Assert.Null(((IProperty)property).Relational().DefaultExpression);
        }

        [Fact]
        public void Can_get_and_set_column_key_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var key = modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .Metadata;

            Assert.Null(key.Relational().Name);
            Assert.Null(((IKey)key).Relational().Name);

            key.Relational().Name = "PrimaryKey";

            Assert.Equal("PrimaryKey", key.Relational().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).Relational().Name);

            key.Relational().Name = null;

            Assert.Null(key.Relational().Name);
            Assert.Null(((IKey)key).Relational().Name);
        }

        [Fact]
        public void Can_get_and_set_column_foreign_key_name()
        {
            var modelBuilder = new BasicModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id);

            var foreignKey = modelBuilder
                .Entity<Order>()
                .ForeignKey<Customer>(e => e.CustomerId)
                .Metadata;

            Assert.Null(foreignKey.Relational().Name);
            Assert.Null(((IForeignKey)foreignKey).Relational().Name);

            foreignKey.Relational().Name = "FK";

            Assert.Equal("FK", foreignKey.Relational().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).Relational().Name);

            foreignKey.Relational().Name = null;

            Assert.Null(foreignKey.Relational().Name);
            Assert.Null(((IForeignKey)foreignKey).Relational().Name);
        }

        [Fact]
        public void Can_get_and_set_index_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var index = modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .Metadata;

            Assert.Null(index.Relational().Name);
            Assert.Null(((IIndex)index).Relational().Name);

            index.Relational().Name = "MyIndex";

            Assert.Equal("MyIndex", index.Relational().Name);
            Assert.Equal("MyIndex", ((IIndex)index).Relational().Name);

            index.Relational().Name = null;

            Assert.Null(index.Relational().Name);
            Assert.Null(((IIndex)index).Relational().Name);
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
