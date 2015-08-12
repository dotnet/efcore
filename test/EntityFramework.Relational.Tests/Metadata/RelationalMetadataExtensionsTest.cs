// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Metadata.Conventions;
using Microsoft.Data.Entity.Relational.Internal;
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

            Assert.Equal("Name", property.Relational().ColumnName);

            property.Relational().ColumnName = "Eman";

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", ((IProperty)property).Name);
            Assert.Equal("Eman", property.Relational().ColumnName);

            property.Relational().ColumnName = null;

            Assert.Equal("Name", property.Relational().ColumnName);
        }

        [Fact]
        public void Can_get_and_set_table_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Equal("Customer", entityType.Relational().TableName);

            entityType.Relational().TableName = "Customizer";

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customer", ((IEntityType)entityType).DisplayName());
            Assert.Equal("Customizer", entityType.Relational().TableName);

            entityType.Relational().TableName = null;

            Assert.Equal("Customer", entityType.Relational().TableName);
        }

        [Fact]
        public void Can_get_and_set_schema_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Null(entityType.Relational().Schema);

            entityType.Relational().Schema = "db0";

            Assert.Equal("db0", entityType.Relational().Schema);

            entityType.Relational().Schema = null;

            Assert.Null(entityType.Relational().Schema);
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

            property.Relational().ColumnType = "nvarchar(max)";

            Assert.Equal("nvarchar(max)", property.Relational().ColumnType);

            property.Relational().ColumnType = null;

            Assert.Null(property.Relational().ColumnType);
        }

        [Fact]
        public void Can_get_and_set_column_default_expression()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().GeneratedValueSql);

            property.Relational().GeneratedValueSql = "newsequentialid()";

            Assert.Equal("newsequentialid()", property.Relational().GeneratedValueSql);

            property.Relational().GeneratedValueSql = null;

            Assert.Null(property.Relational().GeneratedValueSql);
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

            var guid = new Guid("{3FDFC4F5-AEAB-4D72-9C96-201E004349FA}");

            property.Relational().DefaultValue = guid;

            Assert.Equal(guid, property.Relational().DefaultValue);

            property.Relational().DefaultValue = null;

            Assert.Null(property.Relational().DefaultValue);
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

            key.Relational().Name = "PrimaryKey";

            Assert.Equal("PrimaryKey", key.Relational().Name);

            key.Relational().Name = null;

            Assert.Equal("PK_Customer", key.Relational().Name);
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

            foreignKey.Relational().Name = "FK";

            Assert.Equal("FK", foreignKey.Relational().Name);

            foreignKey.Relational().Name = null;

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.Relational().Name);
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

            index.Relational().Name = "MyIndex";

            Assert.Equal("MyIndex", index.Relational().Name);

            index.Relational().Name = null;

            Assert.Equal("IX_Customer_Id", index.Relational().Name);
        }

        [Fact]
        public void Can_get_and_set_discriminator()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Null(entityType.Relational().DiscriminatorProperty);

            var property = entityType.AddProperty("D");

            entityType.Relational().DiscriminatorProperty = property;

            Assert.Same(property, entityType.Relational().DiscriminatorProperty);

            entityType.Relational().DiscriminatorProperty = null;

            Assert.Null(entityType.Relational().DiscriminatorProperty);
        }

        [Fact]
        public void Setting_discriminator_on_non_root_type_throws()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;
            var property = entityType.AddProperty("D");

            var derivedType = modelBuilder
                .Entity<SpecialCustomer>()
                .Metadata;
            derivedType.BaseType = entityType;

            Assert.Equal(Strings.DiscriminatorPropertyMustBeOnRoot(typeof(SpecialCustomer).FullName),
                Assert.Throws<InvalidOperationException>(() => derivedType.Relational().DiscriminatorProperty = property).Message);
        }

        [Fact]
        public void Setting_discriminator_from_different_entity_type_throws()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            var otherType = modelBuilder
                .Entity<SpecialCustomer>()
                .Metadata;

            var property = entityType.AddProperty("D");

            Assert.Equal(Strings.DiscriminatorPropertyNotFound("D", typeof(SpecialCustomer).FullName),
                Assert.Throws<InvalidOperationException>(() => otherType.Relational().DiscriminatorProperty = property).Message);
        }

        [Fact]
        public void Can_get_and_set_discriminator_value()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            var property = entityType.AddProperty("D");
            property.ClrType = typeof(string);
            entityType.Relational().DiscriminatorProperty = property;

            Assert.Null(entityType.Relational().DiscriminatorValue);

            entityType.Relational().DiscriminatorValue = "V";

            Assert.Equal("V", entityType.Relational().DiscriminatorValue);

            entityType.Relational().DiscriminatorValue = null;

            Assert.Null(entityType.Relational().DiscriminatorValue);
        }

        [Fact]
        public void Setting_discriminator_value_when_discriminator_not_set_throws()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;
            
            Assert.Equal(Strings.NoDiscriminatorForValue("Customer", "Customer"),
                Assert.Throws<InvalidOperationException>(() =>
                    entityType.Relational().DiscriminatorValue = "V").Message);
        }

        [Fact]
        public void Setting_incompatible_discriminator_value_throws()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            var property = entityType.AddProperty("D");
            property.ClrType = typeof(int);
            entityType.Relational().DiscriminatorProperty = property;

            Assert.Equal(Strings.DiscriminitatorValueIncompatible("V", "D", typeof(int)),
                Assert.Throws<InvalidOperationException>(() =>
                    entityType.Relational().DiscriminatorValue = "V").Message);

            entityType.Relational().DiscriminatorValue = null;
        }

        [Fact]
        public void Can_get_and_set_sequence()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;
            var extensions = model.Relational();

            Assert.Null(extensions.FindSequence("Foo"));
            Assert.Null(model.Relational().FindSequence("Foo"));

            var sequence = extensions.GetOrAddSequence("Foo");

            Assert.Equal("Foo", extensions.FindSequence("Foo").Name);
            Assert.Equal("Foo", model.Relational().FindSequence("Foo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);

            var sequence2 = extensions.GetOrAddSequence("Foo");

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
        public void Can_get_and_set_sequence_with_schema_name()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());
            var model = modelBuilder.Model;
            var extensions = model.Relational();

            Assert.Null(extensions.FindSequence("Foo", "Smoo"));
            Assert.Null(model.Relational().FindSequence("Foo", "Smoo"));

            var sequence = extensions.GetOrAddSequence("Foo", "Smoo");

            Assert.Equal("Foo", extensions.FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", model.Relational().FindSequence("Foo", "Smoo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);

            var sequence2 = extensions.GetOrAddSequence("Foo", "Smoo");

            sequence.StartValue = 1729;
            sequence.IncrementBy = 11;
            sequence.MinValue = 2001;
            sequence.MaxValue = 2010;
            sequence.ClrType = typeof(int);

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
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
            var extensions = model.Relational();

            extensions.GetOrAddSequence("Fibonacci");
            extensions.GetOrAddSequence("Golomb");

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

        private class SpecialCustomer : Customer
        {
        }

        private class Order
        {
            public int OrderId { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
