// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.SqlServer.Tests.Metadata
{
    public class SqlServerMetadataExtensionsTest
    {
        [Fact]
        public void Can_get_and_set_column_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal("Name", property.SqlServer().Column);
            Assert.Equal("Name", ((IProperty)property).SqlServer().Column);

            property.Relational().Column = "Eman";

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", ((IProperty)property).Name);
            Assert.Equal("Eman", property.Relational().Column);
            Assert.Equal("Eman", ((IProperty)property).Relational().Column);
            Assert.Equal("Eman", property.SqlServer().Column);
            Assert.Equal("Eman", ((IProperty)property).SqlServer().Column);

            property.SqlServer().Column = "MyNameIs";

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", ((IProperty)property).Name);
            Assert.Equal("Eman", property.Relational().Column);
            Assert.Equal("Eman", ((IProperty)property).Relational().Column);
            Assert.Equal("MyNameIs", property.SqlServer().Column);
            Assert.Equal("MyNameIs", ((IProperty)property).SqlServer().Column);

            property.SqlServer().Column = null;

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", ((IProperty)property).Name);
            Assert.Equal("Eman", property.Relational().Column);
            Assert.Equal("Eman", ((IProperty)property).Relational().Column);
            Assert.Equal("Eman", property.SqlServer().Column);
            Assert.Equal("Eman", ((IProperty)property).SqlServer().Column);
        }

        [Fact]
        public void Can_get_and_set_table_name()
        {
            var modelBuilder = new BasicModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Equal("Customer", entityType.SqlServer().Table);
            Assert.Equal("Customer", ((IEntityType)entityType).SqlServer().Table);

            entityType.Relational().Table = "Customizer";

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customer", ((IEntityType)entityType).SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).Relational().Table);
            Assert.Equal("Customizer", entityType.SqlServer().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).SqlServer().Table);

            entityType.SqlServer().Table = "Custardizer";

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customer", ((IEntityType)entityType).SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).Relational().Table);
            Assert.Equal("Custardizer", entityType.SqlServer().Table);
            Assert.Equal("Custardizer", ((IEntityType)entityType).SqlServer().Table);

            entityType.SqlServer().Table = null;

            Assert.Equal("Customer", entityType.SimpleName);
            Assert.Equal("Customer", ((IEntityType)entityType).SimpleName);
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).Relational().Table);
            Assert.Equal("Customizer", entityType.SqlServer().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).SqlServer().Table);
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
            Assert.Null(entityType.SqlServer().Schema);
            Assert.Null(((IEntityType)entityType).SqlServer().Schema);

            entityType.Relational().Schema = "db0";

            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).Relational().Schema);
            Assert.Equal("db0", entityType.SqlServer().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).SqlServer().Schema);

            entityType.SqlServer().Schema = "dbOh";

            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
            Assert.Equal("dbOh", ((IEntityType)entityType).SqlServer().Schema);

            entityType.SqlServer().Schema = null;

            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).Relational().Schema);
            Assert.Equal("db0", entityType.SqlServer().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).SqlServer().Schema);
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
            Assert.Null(property.SqlServer().ColumnType);
            Assert.Null(((IProperty)property).SqlServer().ColumnType);

            property.Relational().ColumnType = "nvarchar(max)";

            Assert.Equal("nvarchar(max)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).Relational().ColumnType);
            Assert.Equal("nvarchar(max)", property.SqlServer().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).SqlServer().ColumnType);

            property.SqlServer().ColumnType = "nvarchar(verstappen)";

            Assert.Equal("nvarchar(max)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).Relational().ColumnType);
            Assert.Equal("nvarchar(verstappen)", property.SqlServer().ColumnType);
            Assert.Equal("nvarchar(verstappen)", ((IProperty)property).SqlServer().ColumnType);

            property.SqlServer().ColumnType = null;

            Assert.Equal("nvarchar(max)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).Relational().ColumnType);
            Assert.Equal("nvarchar(max)", property.SqlServer().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).SqlServer().ColumnType);
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
            Assert.Null(property.SqlServer().DefaultExpression);
            Assert.Null(((IProperty)property).SqlServer().DefaultExpression);

            property.Relational().DefaultExpression = "newsequentialid()";

            Assert.Equal("newsequentialid()", property.Relational().DefaultExpression);
            Assert.Equal("newsequentialid()", ((IProperty)property).Relational().DefaultExpression);
            Assert.Equal("newsequentialid()", property.SqlServer().DefaultExpression);
            Assert.Equal("newsequentialid()", ((IProperty)property).SqlServer().DefaultExpression);

            property.SqlServer().DefaultExpression = "expressyourself()";

            Assert.Equal("newsequentialid()", property.Relational().DefaultExpression);
            Assert.Equal("newsequentialid()", ((IProperty)property).Relational().DefaultExpression);
            Assert.Equal("expressyourself()", property.SqlServer().DefaultExpression);
            Assert.Equal("expressyourself()", ((IProperty)property).SqlServer().DefaultExpression);

            property.SqlServer().DefaultExpression = null;

            Assert.Equal("newsequentialid()", property.Relational().DefaultExpression);
            Assert.Equal("newsequentialid()", ((IProperty)property).Relational().DefaultExpression);
            Assert.Equal("newsequentialid()", property.SqlServer().DefaultExpression);
            Assert.Equal("newsequentialid()", ((IProperty)property).SqlServer().DefaultExpression);
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
            Assert.Null(key.SqlServer().Name);
            Assert.Null(((IKey)key).SqlServer().Name);

            key.Relational().Name = "PrimaryKey";

            Assert.Equal("PrimaryKey", key.Relational().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).Relational().Name);
            Assert.Equal("PrimaryKey", key.SqlServer().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).SqlServer().Name);

            key.SqlServer().Name = "PrimarySchool";

            Assert.Equal("PrimaryKey", key.Relational().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).Relational().Name);
            Assert.Equal("PrimarySchool", key.SqlServer().Name);
            Assert.Equal("PrimarySchool", ((IKey)key).SqlServer().Name);

            key.SqlServer().Name = null;

            Assert.Equal("PrimaryKey", key.Relational().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).Relational().Name);
            Assert.Equal("PrimaryKey", key.SqlServer().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).SqlServer().Name);
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
            Assert.Null(foreignKey.SqlServer().Name);
            Assert.Null(((IForeignKey)foreignKey).SqlServer().Name);

            foreignKey.Relational().Name = "FK";

            Assert.Equal("FK", foreignKey.Relational().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).Relational().Name);
            Assert.Equal("FK", foreignKey.SqlServer().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).SqlServer().Name);

            foreignKey.SqlServer().Name = "KFC";

            Assert.Equal("FK", foreignKey.Relational().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).Relational().Name);
            Assert.Equal("KFC", foreignKey.SqlServer().Name);
            Assert.Equal("KFC", ((IForeignKey)foreignKey).SqlServer().Name);

            foreignKey.SqlServer().Name = null;

            Assert.Equal("FK", foreignKey.Relational().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).Relational().Name);
            Assert.Equal("FK", foreignKey.SqlServer().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).SqlServer().Name);
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
            Assert.Null(index.SqlServer().Name);
            Assert.Null(((IIndex)index).SqlServer().Name);

            index.Relational().Name = "MyIndex";

            Assert.Equal("MyIndex", index.Relational().Name);
            Assert.Equal("MyIndex", ((IIndex)index).Relational().Name);
            Assert.Equal("MyIndex", index.SqlServer().Name);
            Assert.Equal("MyIndex", ((IIndex)index).SqlServer().Name);

            index.SqlServer().Name = "DexKnows";

            Assert.Equal("MyIndex", index.Relational().Name);
            Assert.Equal("MyIndex", ((IIndex)index).Relational().Name);
            Assert.Equal("DexKnows", index.SqlServer().Name);
            Assert.Equal("DexKnows", ((IIndex)index).SqlServer().Name);

            index.SqlServer().Name = null;

            Assert.Equal("MyIndex", index.Relational().Name);
            Assert.Equal("MyIndex", ((IIndex)index).Relational().Name);
            Assert.Equal("MyIndex", index.SqlServer().Name);
            Assert.Equal("MyIndex", ((IIndex)index).SqlServer().Name);
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
