// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
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

            Assert.Equal("Name", property.Sqlite().ColumnName);
            Assert.Equal("Name", ((IProperty)property).Sqlite().ColumnName);

            property.Relational().ColumnName = "Eman";

            Assert.Equal("Eman", property.Sqlite().ColumnName);
            Assert.Equal("Eman", ((IProperty)property).Sqlite().ColumnName);

            property.Sqlite().ColumnName = "MyNameIs";

            Assert.Equal("MyNameIs", property.Sqlite().ColumnName);
            Assert.Equal("MyNameIs", ((IProperty)property).Sqlite().ColumnName);

            property.Sqlite().ColumnName = null;

            Assert.Equal("Eman", property.Sqlite().ColumnName);
            Assert.Equal("Eman", ((IProperty)property).Sqlite().ColumnName);
        }

        [Fact]
        public void Can_get_and_set_table_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Equal("Customer", entityType.Sqlite().TableName);
            Assert.Equal("Customer", ((IEntityType)entityType).Sqlite().TableName);

            entityType.Relational().TableName = "Customizer";

            Assert.Equal("Customizer", entityType.Sqlite().TableName);
            Assert.Equal("Customizer", ((IEntityType)entityType).Sqlite().TableName);

            entityType.Sqlite().TableName = "Custardizer";

            Assert.Equal("Custardizer", entityType.Sqlite().TableName);
            Assert.Equal("Custardizer", ((IEntityType)entityType).Sqlite().TableName);

            entityType.Sqlite().TableName = null;

            Assert.Equal("Customizer", entityType.Sqlite().TableName);
            Assert.Equal("Customizer", ((IEntityType)entityType).Sqlite().TableName);
        }

        [Fact]
        public void Can_get_schema_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Null(entityType.Sqlite().Schema);
            Assert.Null(((IEntityType)entityType).Sqlite().Schema);

            entityType.Relational().Schema = "db0";

            Assert.Equal("db0", entityType.Sqlite().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).Sqlite().Schema);
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

            Assert.Null(property.Sqlite().GeneratedValueSql);
            Assert.Null(((IProperty)property).Sqlite().GeneratedValueSql);

            property.Relational().GeneratedValueSql = "newsequentialid()";

            Assert.Equal("newsequentialid()", property.Sqlite().GeneratedValueSql);
            Assert.Equal("newsequentialid()", ((IProperty)property).Sqlite().GeneratedValueSql);

            property.Sqlite().GeneratedValueSql = "expressyourself()";

            Assert.Equal("expressyourself()", property.Sqlite().GeneratedValueSql);
            Assert.Equal("expressyourself()", ((IProperty)property).Sqlite().GeneratedValueSql);

            property.Sqlite().GeneratedValueSql = null;

            Assert.Equal("newsequentialid()", property.Sqlite().GeneratedValueSql);
            Assert.Equal("newsequentialid()", ((IProperty)property).Sqlite().GeneratedValueSql);
        }

        [Fact]
        public void Can_get_and_set_column_key_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var key = modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
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
                .HasKey(e => e.Id);

            var foreignKey = modelBuilder
                .Entity<Order>()
                .HasOne<Customer>()
                .WithOne()
                .HasForeignKey<Order>(e => e.CustomerId)
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
                .HasIndex(e => e.Id)
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
        public void Can_get_and_set_sequence()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;

            Assert.Null(model.Relational().FindSequence("Foo"));
            Assert.Null(model.Sqlite().FindSequence("Foo"));
            Assert.Null(((IModel)model).Sqlite().FindSequence("Foo"));

            var sequence = model.Sqlite().GetOrAddSequence("Foo");

            Assert.Null(model.Relational().FindSequence("Foo"));
            Assert.Equal("Foo", model.Sqlite().FindSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).Sqlite().FindSequence("Foo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);

            Assert.Null(model.Relational().FindSequence("Foo"));

            var sequence2 = model.Sqlite().FindSequence("Foo");

            sequence.StartValue = 1729;
            sequence.IncrementBy = 11;
            sequence.MinValue = 2001;
            sequence.MaxValue = 2010;
            sequence.ClrType = typeof(int);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);

            Assert.Equal(sequence2.Name, sequence.Name);
            Assert.Equal(sequence2.Schema, sequence.Schema);
            Assert.Equal(sequence2.IncrementBy, sequence.IncrementBy);
            Assert.Equal(sequence2.StartValue, sequence.StartValue);
            Assert.Equal(sequence2.MinValue, sequence.MinValue);
            Assert.Equal(sequence2.MaxValue, sequence.MaxValue);
            Assert.Same(sequence2.ClrType, sequence.ClrType);
        }

        [Fact]
        public void Can_get_multiple_sequences()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;

            model.Relational().GetOrAddSequence("Fibonacci");
            model.Sqlite().GetOrAddSequence("Golomb");

            var sequences = model.Sqlite().Sequences;

            Assert.Equal(2, sequences.Count);
            Assert.Contains(sequences, s => s.Name == "Fibonacci");
            Assert.Contains(sequences, s => s.Name == "Golomb");
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
