// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Sqlite.Metadata
{
    public class SqliteMetadataExtensionsTest
    {
        [Fact]
        public void Can_get_and_set_column_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal("Name", property.Sqlite().Column);
            Assert.Equal("Name", ((IProperty)property).Sqlite().Column);

            property.Relational().Column = "Eman";

            Assert.Equal("Eman", property.Sqlite().Column);
            Assert.Equal("Eman", ((IProperty)property).Sqlite().Column);

            property.Sqlite().Column = "MyNameIs";

            Assert.Equal("MyNameIs", property.Sqlite().Column);
            Assert.Equal("MyNameIs", ((IProperty)property).Sqlite().Column);

            property.Sqlite().Column = null;

            Assert.Equal("Eman", property.Sqlite().Column);
            Assert.Equal("Eman", ((IProperty)property).Sqlite().Column);
        }

        [Fact]
        public void Can_get_and_set_table_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Equal("Customer", entityType.Sqlite().Table);
            Assert.Equal("Customer", ((IEntityType)entityType).Sqlite().Table);

            entityType.Relational().Table = "Customizer";

            Assert.Equal("Customizer", entityType.Sqlite().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).Sqlite().Table);

            entityType.Sqlite().Table = "Custardizer";

            Assert.Equal("Custardizer", entityType.Sqlite().Table);
            Assert.Equal("Custardizer", ((IEntityType)entityType).Sqlite().Table);

            entityType.Sqlite().Table = null;

            Assert.Equal("Customizer", entityType.Sqlite().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).Sqlite().Table);
        }

        [Fact]
        public void Cant_get_schema_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Null(entityType.Sqlite().Schema);
            Assert.Null(((IEntityType)entityType).Sqlite().Schema);

            entityType.Relational().Schema = "db0";

            Assert.Null(entityType.Sqlite().Schema);
            Assert.Null(((IEntityType)entityType).Sqlite().Schema);
        }

        [Fact]
        public void Can_get_and_set_column_type()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Sqlite().ColumnType);
            Assert.Null(((IProperty)property).Sqlite().ColumnType);

            property.Relational().ColumnType = "nvarchar(max)";

            Assert.Equal("nvarchar(max)", property.Sqlite().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).Sqlite().ColumnType);

            property.Sqlite().ColumnType = "nvarchar(verstappen)";

            Assert.Equal("nvarchar(verstappen)", property.Sqlite().ColumnType);
            Assert.Equal("nvarchar(verstappen)", ((IProperty)property).Sqlite().ColumnType);

            property.Sqlite().ColumnType = null;

            Assert.Equal("nvarchar(max)", property.Sqlite().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).Sqlite().ColumnType);
        }

        [Fact]
        public void Can_get_and_set_column_default_expression()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Sqlite().DefaultValueSql);
            Assert.Null(((IProperty)property).Sqlite().DefaultValueSql);

            property.Relational().DefaultValueSql = "newsequentialid()";

            Assert.Equal("newsequentialid()", property.Sqlite().DefaultValueSql);
            Assert.Equal("newsequentialid()", ((IProperty)property).Sqlite().DefaultValueSql);

            property.Sqlite().DefaultValueSql = "expressyourself()";

            Assert.Equal("expressyourself()", property.Sqlite().DefaultValueSql);
            Assert.Equal("expressyourself()", ((IProperty)property).Sqlite().DefaultValueSql);

            property.Sqlite().DefaultValueSql = null;

            Assert.Equal("newsequentialid()", property.Sqlite().DefaultValueSql);
            Assert.Equal("newsequentialid()", ((IProperty)property).Sqlite().DefaultValueSql);
        }

        [Fact]
        public void Can_get_and_set_column_key_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var key = modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .Metadata;

            Assert.Equal("PK_Customer", key.Sqlite().Name);
            Assert.Equal("PK_Customer", ((IKey)key).Sqlite().Name);

            key.Relational().Name = "PrimaryKey";

            Assert.Equal("PrimaryKey", key.Sqlite().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).Sqlite().Name);

            key.Sqlite().Name = "PrimarySchool";

            Assert.Equal("PrimarySchool", key.Sqlite().Name);
            Assert.Equal("PrimarySchool", ((IKey)key).Sqlite().Name);

            key.Sqlite().Name = null;

            Assert.Equal("PrimaryKey", key.Sqlite().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).Sqlite().Name);
        }

        [Fact]
        public void Can_get_and_set_column_foreign_key_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id);

            var foreignKey = modelBuilder
                .Entity<Order>()
                .Reference<Customer>()
                .InverseReference()
                .ForeignKey<Order>(e => e.CustomerId)
                .Metadata;

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.Sqlite().Name);
            Assert.Equal("FK_Order_Customer_CustomerId", ((IForeignKey)foreignKey).Sqlite().Name);

            foreignKey.Relational().Name = "FK";

            Assert.Equal("FK", foreignKey.Sqlite().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).Sqlite().Name);

            foreignKey.Sqlite().Name = "KFC";

            Assert.Equal("KFC", foreignKey.Sqlite().Name);
            Assert.Equal("KFC", ((IForeignKey)foreignKey).Sqlite().Name);

            foreignKey.Sqlite().Name = null;

            Assert.Equal("FK", foreignKey.Sqlite().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).Sqlite().Name);
        }

        [Fact]
        public void Can_get_and_set_index_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var index = modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .Metadata;

            Assert.Equal("IX_Customer_Id", index.Sqlite().Name);
            Assert.Equal("IX_Customer_Id", ((IIndex)index).Sqlite().Name);

            index.Relational().Name = "MyIndex";

            Assert.Equal("MyIndex", index.Sqlite().Name);
            Assert.Equal("MyIndex", ((IIndex)index).Sqlite().Name);

            index.Sqlite().Name = "DexKnows";

            Assert.Equal("DexKnows", index.Sqlite().Name);
            Assert.Equal("DexKnows", ((IIndex)index).Sqlite().Name);

            index.Sqlite().Name = null;

            Assert.Equal("MyIndex", index.Sqlite().Name);
            Assert.Equal("MyIndex", ((IIndex)index).Sqlite().Name);
        }

        [Fact]
        public void Cant_get_sequence()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;

            Assert.Null(model.Sqlite().TryGetSequence("Foo"));
            Assert.Null(((IModel)model).Sqlite().TryGetSequence("Foo"));

            var sequence = model.Relational().GetOrAddSequence("Foo");

            Assert.Null(model.Sqlite().TryGetSequence("Foo"));
            Assert.Null(((IModel)model).Sqlite().TryGetSequence("Foo"));
        }

        [Fact]
        public void Cant_get_multiple_sequences()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;

            model.Relational().AddOrReplaceSequence(new Sequence("Fibonacci"));

            Assert.Empty(model.Sqlite().Sequences);
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        private class Order
        {
            public int CustomerId { get; set; }
        }
    }
}
