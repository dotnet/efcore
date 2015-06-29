// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Metadata.Conventions.Internal;
using Xunit;

namespace Microsoft.Data.Entity.Metadata.Tests
{
    public class RelationalMetadataExtensionsTest
    {
        [Fact]
        public void Can_get_and_set_column_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

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
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Equal("Customer", entityType.Relational().Table);
            Assert.Equal("Customer", ((IEntityType)entityType).Relational().Table);

            entityType.Relational().Table = "Customizer";

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customer", ((IEntityType)entityType).DisplayName());
            Assert.Equal("Customizer", entityType.Relational().Table);
            Assert.Equal("Customizer", ((IEntityType)entityType).Relational().Table);

            entityType.Relational().Table = null;

            Assert.Equal("Customer", entityType.Relational().Table);
            Assert.Equal("Customer", ((IEntityType)entityType).Relational().Table);
        }

        [Fact]
        public void Can_get_and_set_schema_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

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
        public void Can_get_and_set_column_order()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().ColumnOrder);
            Assert.Null(((IProperty)property).Relational().ColumnOrder);

            property.Relational().ColumnOrder = 2;

            Assert.Equal(2, property.Relational().ColumnOrder);
            Assert.Equal(2, ((IProperty)property).Relational().ColumnOrder);

            property.Relational().ColumnOrder = null;

            Assert.Null(property.Relational().ColumnOrder);
            Assert.Null(((IProperty)property).Relational().ColumnOrder);
        }

        [Fact]
        public void Can_get_and_set_column_type()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

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
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().DefaultValueSql);
            Assert.Null(((IProperty)property).Relational().DefaultValueSql);

            property.Relational().DefaultValueSql = "newsequentialid()";

            Assert.Equal("newsequentialid()", property.Relational().DefaultValueSql);
            Assert.Equal("newsequentialid()", ((IProperty)property).Relational().DefaultValueSql);

            property.Relational().DefaultValueSql = null;

            Assert.Null(property.Relational().DefaultValueSql);
            Assert.Null(((IProperty)property).Relational().DefaultValueSql);
        }

        [Fact]
        public void Can_get_and_set_column_default_value()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().DefaultValue);
            Assert.Null(((IProperty)property).Relational().DefaultValue);

            var guid = new Guid("{3FDFC4F5-AEAB-4D72-9C96-201E004349FA}");

            property.Relational().DefaultValue = guid;

            Assert.Equal(guid, property.Relational().DefaultValue);
            Assert.Equal(guid, ((IProperty)property).Relational().DefaultValue);

            property.Relational().DefaultValue = null;

            Assert.Null(property.Relational().DefaultValue);
            Assert.Null(((IProperty)property).Relational().DefaultValue);
        }

        [Fact]
        public void Can_get_and_set_column_key_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var key = modelBuilder
                .Entity<Customer>()
                .Key(e => e.Id)
                .Metadata;

            Assert.Equal("PK_Customer", key.Relational().Name);
            Assert.Equal("PK_Customer", ((IKey)key).Relational().Name);

            key.Relational().Name = "PrimaryKey";

            Assert.Equal("PrimaryKey", key.Relational().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).Relational().Name);

            key.Relational().Name = null;

            Assert.Equal("PK_Customer", key.Relational().Name);
            Assert.Equal("PK_Customer", ((IKey)key).Relational().Name);
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

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.Relational().Name);
            Assert.Equal("FK_Order_Customer_CustomerId", ((IForeignKey)foreignKey).Relational().Name);

            foreignKey.Relational().Name = "FK";

            Assert.Equal("FK", foreignKey.Relational().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).Relational().Name);

            foreignKey.Relational().Name = null;

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.Relational().Name);
            Assert.Equal("FK_Order_Customer_CustomerId", ((IForeignKey)foreignKey).Relational().Name);
        }

        [Fact]
        public void Can_get_and_set_index_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var index = modelBuilder
                .Entity<Customer>()
                .Index(e => e.Id)
                .Metadata;

            Assert.Equal("IX_Customer_Id", index.Relational().Name);
            Assert.Equal("IX_Customer_Id", ((IIndex)index).Relational().Name);

            index.Relational().Name = "MyIndex";

            Assert.Equal("MyIndex", index.Relational().Name);
            Assert.Equal("MyIndex", ((IIndex)index).Relational().Name);

            index.Relational().Name = null;

            Assert.Equal("IX_Customer_Id", index.Relational().Name);
            Assert.Equal("IX_Customer_Id", ((IIndex)index).Relational().Name);
        }

        [Fact]
        public void Can_get_and_set_sequence()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;
            var extensions = model.Relational();

            Assert.Null(extensions.TryGetSequence("Foo"));
            Assert.Null(((IModel)model).Relational().TryGetSequence("Foo"));

            var sequence = extensions.GetOrAddSequence("Foo");

            Assert.Equal("Foo", extensions.TryGetSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).Relational().TryGetSequence("Foo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.Type);

            extensions.AddOrReplaceSequence(new Sequence("Foo", null, 1729, 11, 2001, 2010, typeof(int)));

            sequence = extensions.GetOrAddSequence("Foo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Can_get_and_set_sequence_with_schema_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;
            var extensions = model.Relational();

            Assert.Null(extensions.TryGetSequence("Foo", "Smoo"));
            Assert.Null(((IModel)model).Relational().TryGetSequence("Foo", "Smoo"));

            var sequence = extensions.GetOrAddSequence("Foo", "Smoo");

            Assert.Equal("Foo", extensions.TryGetSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).Relational().TryGetSequence("Foo", "Smoo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.Type);

            extensions.AddOrReplaceSequence(new Sequence("Foo", "Smoo", 1729, 11, 2001, 2010, typeof(int)));

            sequence = extensions.GetOrAddSequence("Foo", "Smoo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Can_add_and_replace_sequence()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;
            var extensions = model.Relational();

            extensions.AddOrReplaceSequence(new Sequence("Foo"));

            Assert.Equal("Foo", extensions.TryGetSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).Relational().TryGetSequence("Foo").Name);

            var sequence = extensions.TryGetSequence("Foo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.Type);

            extensions.AddOrReplaceSequence(new Sequence("Foo", null, 1729, 11, 2001, 2010, typeof(int)));

            sequence = extensions.TryGetSequence("Foo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Can_add_and_replace_sequence_with_schema_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;
            var extensions = model.Relational();

            extensions.AddOrReplaceSequence(new Sequence("Foo", "Smoo"));

            Assert.Equal("Foo", extensions.TryGetSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).Relational().TryGetSequence("Foo", "Smoo").Name);

            var sequence = extensions.TryGetSequence("Foo", "Smoo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.Type);

            extensions.AddOrReplaceSequence(new Sequence("Foo", "Smoo", 1729, 11, 2001, 2010, typeof(int)));

            sequence = extensions.TryGetSequence("Foo", "Smoo");

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(2001, sequence.MinValue);
            Assert.Equal(2010, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.Type);
        }

        [Fact]
        public void Can_get_multiple_sequences()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;
            var extensions = model.Relational();

            extensions.AddOrReplaceSequence(new Sequence("Fibonacci"));
            extensions.AddOrReplaceSequence(new Sequence("Golomb"));

            var sequences = model.Relational().Sequences;

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
            public int OrderId { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
