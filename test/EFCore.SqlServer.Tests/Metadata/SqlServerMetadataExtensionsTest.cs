// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SqlServerMetadataExtensionsTest
    {
        [Fact]
        public void Can_get_and_set_column_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal("Name", property.SqlServer().ColumnName);
            Assert.Equal("Name", ((IProperty)property).SqlServer().ColumnName);

            property.Relational().ColumnName = "Eman";

            Assert.Equal("Name", property.Name);
            Assert.Equal("Eman", property.Relational().ColumnName);
            Assert.Equal("Eman", property.SqlServer().ColumnName);
            Assert.Equal("Eman", ((IProperty)property).SqlServer().ColumnName);

            property.SqlServer().ColumnName = "MyNameIs";

            Assert.Equal("Name", property.Name);
            Assert.Equal("MyNameIs", property.Relational().ColumnName);
            Assert.Equal("MyNameIs", property.SqlServer().ColumnName);
            Assert.Equal("MyNameIs", ((IProperty)property).SqlServer().ColumnName);

            property.SqlServer().ColumnName = null;

            Assert.Equal("Name", property.Name);
            Assert.Equal("Name", property.Relational().ColumnName);
            Assert.Equal("Name", property.SqlServer().ColumnName);
            Assert.Equal("Name", ((IProperty)property).SqlServer().ColumnName);
        }

        [Fact]
        public void Can_get_and_set_table_name()
        {
            var modelBuilder = GetModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Equal("Customer", entityType.SqlServer().TableName);
            Assert.Equal("Customer", ((IEntityType)entityType).SqlServer().TableName);

            entityType.Relational().TableName = "Customizer";

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customizer", entityType.Relational().TableName);
            Assert.Equal("Customizer", entityType.SqlServer().TableName);
            Assert.Equal("Customizer", ((IEntityType)entityType).SqlServer().TableName);

            entityType.SqlServer().TableName = "Custardizer";

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Custardizer", entityType.Relational().TableName);
            Assert.Equal("Custardizer", entityType.SqlServer().TableName);
            Assert.Equal("Custardizer", ((IEntityType)entityType).SqlServer().TableName);

            entityType.SqlServer().TableName = null;

            Assert.Equal("Customer", entityType.DisplayName());
            Assert.Equal("Customer", entityType.Relational().TableName);
            Assert.Equal("Customer", entityType.SqlServer().TableName);
            Assert.Equal("Customer", ((IEntityType)entityType).SqlServer().TableName);
        }

        [Fact]
        public void Can_get_and_set_schema_name()
        {
            var modelBuilder = GetModelBuilder();

            var entityType = modelBuilder
                .Entity<Customer>()
                .Metadata;

            Assert.Null(entityType.Relational().Schema);
            Assert.Null(entityType.SqlServer().Schema);
            Assert.Null(((IEntityType)entityType).SqlServer().Schema);

            entityType.Relational().Schema = "db0";

            Assert.Equal("db0", entityType.Relational().Schema);
            Assert.Equal("db0", entityType.SqlServer().Schema);
            Assert.Equal("db0", ((IEntityType)entityType).SqlServer().Schema);

            entityType.SqlServer().Schema = "dbOh";

            Assert.Equal("dbOh", entityType.Relational().Schema);
            Assert.Equal("dbOh", entityType.SqlServer().Schema);
            Assert.Equal("dbOh", ((IEntityType)entityType).SqlServer().Schema);

            entityType.SqlServer().Schema = null;

            Assert.Null(entityType.Relational().Schema);
            Assert.Null(entityType.SqlServer().Schema);
            Assert.Null(((IEntityType)entityType).SqlServer().Schema);
        }

        [Fact]
        public void Can_get_and_set_column_type()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().ColumnType);
            Assert.Null(property.SqlServer().ColumnType);
            Assert.Null(((IProperty)property).SqlServer().ColumnType);

            property.Relational().ColumnType = "nvarchar(max)";

            Assert.Equal("nvarchar(max)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(max)", property.SqlServer().ColumnType);
            Assert.Equal("nvarchar(max)", ((IProperty)property).SqlServer().ColumnType);

            property.SqlServer().ColumnType = "nvarchar(verstappen)";

            Assert.Equal("nvarchar(verstappen)", property.Relational().ColumnType);
            Assert.Equal("nvarchar(verstappen)", property.SqlServer().ColumnType);
            Assert.Equal("nvarchar(verstappen)", ((IProperty)property).SqlServer().ColumnType);

            property.SqlServer().ColumnType = null;

            Assert.Null(property.Relational().ColumnType);
            Assert.Null(property.SqlServer().ColumnType);
            Assert.Null(((IProperty)property).SqlServer().ColumnType);
        }

        [Fact]
        public void Can_get_and_set_column_default_expression()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().DefaultValueSql);
            Assert.Null(property.SqlServer().DefaultValueSql);
            Assert.Null(((IProperty)property).SqlServer().DefaultValueSql);

            property.Relational().DefaultValueSql = "newsequentialid()";

            Assert.Equal("newsequentialid()", property.Relational().DefaultValueSql);
            Assert.Equal("newsequentialid()", property.SqlServer().DefaultValueSql);
            Assert.Equal("newsequentialid()", ((IProperty)property).SqlServer().DefaultValueSql);

            property.SqlServer().DefaultValueSql = "expressyourself()";

            Assert.Equal("expressyourself()", property.Relational().DefaultValueSql);
            Assert.Equal("expressyourself()", property.SqlServer().DefaultValueSql);
            Assert.Equal("expressyourself()", ((IProperty)property).SqlServer().DefaultValueSql);

            property.SqlServer().DefaultValueSql = null;

            Assert.Null(property.Relational().DefaultValueSql);
            Assert.Null(property.SqlServer().DefaultValueSql);
            Assert.Null(((IProperty)property).SqlServer().DefaultValueSql);
        }

        [Fact]
        public void Can_get_and_set_column_computed_expression()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Null(property.Relational().ComputedColumnSql);
            Assert.Null(property.SqlServer().ComputedColumnSql);
            Assert.Null(((IProperty)property).SqlServer().ComputedColumnSql);

            property.Relational().ComputedColumnSql = "newsequentialid()";

            Assert.Equal("newsequentialid()", property.Relational().ComputedColumnSql);
            Assert.Equal("newsequentialid()", property.SqlServer().ComputedColumnSql);
            Assert.Equal("newsequentialid()", ((IProperty)property).SqlServer().ComputedColumnSql);

            property.SqlServer().ComputedColumnSql = "expressyourself()";

            Assert.Equal("expressyourself()", property.Relational().ComputedColumnSql);
            Assert.Equal("expressyourself()", property.SqlServer().ComputedColumnSql);
            Assert.Equal("expressyourself()", ((IProperty)property).SqlServer().ComputedColumnSql);

            property.SqlServer().ComputedColumnSql = null;

            Assert.Null(property.Relational().ComputedColumnSql);
            Assert.Null(property.SqlServer().ComputedColumnSql);
            Assert.Null(((IProperty)property).SqlServer().ComputedColumnSql);
        }

        [Fact]
        public void Can_get_and_set_column_default_value()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.ByteArray)
                .Metadata;

            Assert.Null(property.Relational().DefaultValue);
            Assert.Null(property.SqlServer().DefaultValue);
            Assert.Null(((IProperty)property).SqlServer().DefaultValue);

            property.Relational().DefaultValue = new byte[] { 69, 70, 32, 82, 79, 67, 75, 83 };

            Assert.Equal(new byte[] { 69, 70, 32, 82, 79, 67, 75, 83 }, property.Relational().DefaultValue);
            Assert.Equal(new byte[] { 69, 70, 32, 82, 79, 67, 75, 83 }, property.SqlServer().DefaultValue);
            Assert.Equal(new byte[] { 69, 70, 32, 82, 79, 67, 75, 83 }, ((IProperty)property).SqlServer().DefaultValue);

            property.SqlServer().DefaultValue = new byte[] { 69, 70, 32, 83, 79, 67, 75, 83 };

            Assert.Equal(new byte[] { 69, 70, 32, 83, 79, 67, 75, 83 }, property.Relational().DefaultValue);
            Assert.Equal(new byte[] { 69, 70, 32, 83, 79, 67, 75, 83 }, property.SqlServer().DefaultValue);
            Assert.Equal(new byte[] { 69, 70, 32, 83, 79, 67, 75, 83 }, ((IProperty)property).SqlServer().DefaultValue);

            property.SqlServer().DefaultValue = null;

            Assert.Null(property.Relational().DefaultValue);
            Assert.Null(property.SqlServer().DefaultValue);
            Assert.Null(((IProperty)property).SqlServer().DefaultValue);
        }

        [Theory]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValue), nameof(RelationalPropertyAnnotations.DefaultValueSql))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValue), nameof(RelationalPropertyAnnotations.ComputedColumnSql))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValue), nameof(SqlServerPropertyAnnotations.ValueGenerationStrategy))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValueSql), nameof(RelationalPropertyAnnotations.DefaultValue))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValueSql), nameof(RelationalPropertyAnnotations.ComputedColumnSql))]
        [InlineData(nameof(RelationalPropertyAnnotations.DefaultValueSql), nameof(SqlServerPropertyAnnotations.ValueGenerationStrategy))]
        [InlineData(nameof(RelationalPropertyAnnotations.ComputedColumnSql), nameof(RelationalPropertyAnnotations.DefaultValue))]
        [InlineData(nameof(RelationalPropertyAnnotations.ComputedColumnSql), nameof(RelationalPropertyAnnotations.DefaultValueSql))]
        [InlineData(nameof(RelationalPropertyAnnotations.ComputedColumnSql), nameof(SqlServerPropertyAnnotations.ValueGenerationStrategy))]
        [InlineData(nameof(SqlServerPropertyAnnotations.ValueGenerationStrategy), nameof(RelationalPropertyAnnotations.DefaultValue))]
        [InlineData(nameof(SqlServerPropertyAnnotations.ValueGenerationStrategy), nameof(RelationalPropertyAnnotations.DefaultValueSql))]
        [InlineData(nameof(SqlServerPropertyAnnotations.ValueGenerationStrategy), nameof(RelationalPropertyAnnotations.ComputedColumnSql))]
        public void Metadata_throws_when_setting_conflicting_serverGenerated_values(string firstConfiguration, string secondConfiguration)
        {
            var modelBuilder = GetModelBuilder();

            var propertyBuilder = modelBuilder
                .Entity<Customer>()
                .Property(e => e.NullableInt);

            ConfigureProperty(propertyBuilder.Metadata, firstConfiguration, "1");

            Assert.Equal(
                RelationalStrings.ConflictingColumnServerGeneration(secondConfiguration, nameof(Customer.NullableInt), firstConfiguration),
                Assert.Throws<InvalidOperationException>(
                    () =>
                        ConfigureProperty(propertyBuilder.Metadata, secondConfiguration, "2")).Message);
        }

        protected virtual void ConfigureProperty(IMutableProperty property, string configuration, string value)
        {
            var propertyAnnotations = property.SqlServer();
            switch (configuration)
            {
                case nameof(RelationalPropertyAnnotations.DefaultValue):
                    propertyAnnotations.DefaultValue = int.Parse(value);
                    break;
                case nameof(RelationalPropertyAnnotations.DefaultValueSql):
                    propertyAnnotations.DefaultValueSql = value;
                    break;
                case nameof(RelationalPropertyAnnotations.ComputedColumnSql):
                    propertyAnnotations.ComputedColumnSql = value;
                    break;
                case nameof(SqlServerPropertyAnnotations.ValueGenerationStrategy):
                    propertyAnnotations.ValueGenerationStrategy = SqlServerValueGenerationStrategy.IdentityColumn;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        [Fact]
        public void Can_get_and_set_column_key_name()
        {
            var modelBuilder = GetModelBuilder();

            var key = modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .Metadata;

            Assert.Equal("PK_Customer", key.Relational().Name);
            Assert.Equal("PK_Customer", key.SqlServer().Name);
            Assert.Equal("PK_Customer", ((IKey)key).SqlServer().Name);

            key.Relational().Name = "PrimaryKey";

            Assert.Equal("PrimaryKey", key.Relational().Name);
            Assert.Equal("PrimaryKey", key.SqlServer().Name);
            Assert.Equal("PrimaryKey", ((IKey)key).SqlServer().Name);

            key.SqlServer().Name = "PrimarySchool";

            Assert.Equal("PrimarySchool", key.Relational().Name);
            Assert.Equal("PrimarySchool", key.SqlServer().Name);
            Assert.Equal("PrimarySchool", ((IKey)key).SqlServer().Name);

            key.SqlServer().Name = null;

            Assert.Equal("PK_Customer", key.Relational().Name);
            Assert.Equal("PK_Customer", key.SqlServer().Name);
            Assert.Equal("PK_Customer", ((IKey)key).SqlServer().Name);
        }

        [Fact]
        public void Can_get_and_set_column_foreign_key_name()
        {
            var modelBuilder = GetModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id);

            var foreignKey = modelBuilder
                .Entity<Order>()
                .HasOne<Customer>()
                .WithOne()
                .HasForeignKey<Order>(e => e.CustomerId)
                .Metadata;

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.Relational().Name);
            Assert.Equal("FK_Order_Customer_CustomerId", ((IForeignKey)foreignKey).Relational().Name);

            foreignKey.Relational().Name = "FK";

            Assert.Equal("FK", foreignKey.Relational().Name);
            Assert.Equal("FK", ((IForeignKey)foreignKey).Relational().Name);

            foreignKey.Relational().Name = "KFC";

            Assert.Equal("KFC", foreignKey.Relational().Name);
            Assert.Equal("KFC", ((IForeignKey)foreignKey).Relational().Name);

            foreignKey.Relational().Name = null;

            Assert.Equal("FK_Order_Customer_CustomerId", foreignKey.Relational().Name);
            Assert.Equal("FK_Order_Customer_CustomerId", ((IForeignKey)foreignKey).Relational().Name);
        }

        [Fact]
        public void Can_get_and_set_index_name()
        {
            var modelBuilder = GetModelBuilder();

            var index = modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .Metadata;

            Assert.Equal("IX_Customer_Id", index.Relational().Name);
            Assert.Equal("IX_Customer_Id", ((IIndex)index).Relational().Name);

            index.Relational().Name = "MyIndex";

            Assert.Equal("MyIndex", index.Relational().Name);
            Assert.Equal("MyIndex", ((IIndex)index).Relational().Name);

            index.SqlServer().Name = "DexKnows";

            Assert.Equal("DexKnows", index.Relational().Name);
            Assert.Equal("DexKnows", ((IIndex)index).Relational().Name);

            index.SqlServer().Name = null;

            Assert.Equal("IX_Customer_Id", index.Relational().Name);
            Assert.Equal("IX_Customer_Id", ((IIndex)index).Relational().Name);
        }

        [Fact]
        public void Can_get_and_set_index_filter()
        {
            var modelBuilder = new ModelBuilder(new ConventionSet());

            var index = modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .Metadata;

            Assert.Null(index.Relational().Filter);
            Assert.Null(index.SqlServer().Filter);
            Assert.Null(((IIndex)index).SqlServer().Filter);

            index.Relational().Name = "Generic expression";

            Assert.Equal("Generic expression", index.Relational().Name);
            Assert.Equal("Generic expression", index.SqlServer().Name);
            Assert.Equal("Generic expression", ((IIndex)index).SqlServer().Name);

            index.SqlServer().Name = "SqlServer-specific expression";

            Assert.Equal("SqlServer-specific expression", index.Relational().Name);
            Assert.Equal("SqlServer-specific expression", index.SqlServer().Name);
            Assert.Equal("SqlServer-specific expression", ((IIndex)index).SqlServer().Name);

            index.SqlServer().Name = null;

            Assert.Null(index.Relational().Filter);
            Assert.Null(index.SqlServer().Filter);
            Assert.Null(((IIndex)index).SqlServer().Filter);
        }

        [Fact]
        public void Can_get_and_set_index_clustering()
        {
            var modelBuilder = GetModelBuilder();

            var index = modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .Metadata;

            Assert.Null(index.SqlServer().IsClustered);
            Assert.Null(((IIndex)index).SqlServer().IsClustered);

            index.SqlServer().IsClustered = true;

            Assert.True(index.SqlServer().IsClustered.Value);
            Assert.True(((IIndex)index).SqlServer().IsClustered.Value);

            index.SqlServer().IsClustered = null;

            Assert.Null(index.SqlServer().IsClustered);
            Assert.Null(((IIndex)index).SqlServer().IsClustered);
        }

        [Fact]
        public void Can_get_and_set_key_clustering()
        {
            var modelBuilder = GetModelBuilder();

            var key = modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .Metadata;

            Assert.Null(key.SqlServer().IsClustered);
            Assert.Null(((IKey)key).SqlServer().IsClustered);

            key.SqlServer().IsClustered = true;

            Assert.True(key.SqlServer().IsClustered.Value);
            Assert.True(((IKey)key).SqlServer().IsClustered.Value);

            key.SqlServer().IsClustered = null;

            Assert.Null(key.SqlServer().IsClustered);
            Assert.Null(((IKey)key).SqlServer().IsClustered);
        }

        [Fact]
        public void Can_get_and_set_sequence()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.Relational().FindSequence("Foo"));
            Assert.Null(model.SqlServer().FindSequence("Foo"));
            Assert.Null(((IModel)model).SqlServer().FindSequence("Foo"));

            var sequence = model.SqlServer().GetOrAddSequence("Foo");

            Assert.Equal("Foo", model.Relational().FindSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).Relational().FindSequence("Foo").Name);
            Assert.Equal("Foo", model.SqlServer().FindSequence("Foo").Name);
            Assert.Equal("Foo", ((IModel)model).SqlServer().FindSequence("Foo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);

            Assert.NotNull(model.Relational().FindSequence("Foo"));

            var sequence2 = model.SqlServer().FindSequence("Foo");

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
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.Relational().FindSequence("Foo", "Smoo"));
            Assert.Null(model.SqlServer().FindSequence("Foo", "Smoo"));
            Assert.Null(((IModel)model).SqlServer().FindSequence("Foo", "Smoo"));

            var sequence = model.SqlServer().GetOrAddSequence("Foo", "Smoo");

            Assert.Equal("Foo", model.Relational().FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).Relational().FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", model.SqlServer().FindSequence("Foo", "Smoo").Name);
            Assert.Equal("Foo", ((IModel)model).SqlServer().FindSequence("Foo", "Smoo").Name);

            Assert.Equal("Foo", sequence.Name);
            Assert.Equal("Smoo", sequence.Schema);
            Assert.Equal(1, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);

            Assert.NotNull(model.Relational().FindSequence("Foo", "Smoo"));

            var sequence2 = model.SqlServer().FindSequence("Foo", "Smoo");

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
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            model.Relational().GetOrAddSequence("Fibonacci");
            model.SqlServer().GetOrAddSequence("Golomb");

            var sequences = model.SqlServer().Sequences;

            Assert.Equal(2, sequences.Count);
            Assert.Contains(sequences, s => s.Name == "Fibonacci");
            Assert.Contains(sequences, s => s.Name == "Golomb");
        }

        [Fact]
        public void Can_get_multiple_sequences_when_overridden()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            model.Relational().GetOrAddSequence("Fibonacci").StartValue = 1;
            model.SqlServer().GetOrAddSequence("Fibonacci").StartValue = 3;
            model.SqlServer().GetOrAddSequence("Golomb");

            var sequences = model.SqlServer().Sequences;

            Assert.Equal(2, sequences.Count);
            Assert.Contains(sequences, s => s.Name == "Golomb");

            var sequence = sequences.FirstOrDefault(s => s.Name == "Fibonacci");
            Assert.NotNull(sequence);
            Assert.Equal(3, sequence.StartValue);
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_model()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, model.SqlServer().ValueGenerationStrategy);

            model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, model.SqlServer().ValueGenerationStrategy);

            model.SqlServer().ValueGenerationStrategy = null;

            Assert.Null(model.SqlServer().ValueGenerationStrategy);
        }

        [Fact]
        public void Can_get_and_set_default_sequence_name_on_model()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.SqlServer().HiLoSequenceName);
            Assert.Null(((IModel)model).SqlServer().HiLoSequenceName);

            model.SqlServer().HiLoSequenceName = "Tasty.Snook";

            Assert.Equal("Tasty.Snook", model.SqlServer().HiLoSequenceName);
            Assert.Equal("Tasty.Snook", ((IModel)model).SqlServer().HiLoSequenceName);

            model.SqlServer().HiLoSequenceName = null;

            Assert.Null(model.SqlServer().HiLoSequenceName);
            Assert.Null(((IModel)model).SqlServer().HiLoSequenceName);
        }

        [Fact]
        public void Can_get_and_set_default_sequence_schema_on_model()
        {
            var modelBuilder = GetModelBuilder();
            var model = modelBuilder.Model;

            Assert.Null(model.SqlServer().HiLoSequenceSchema);
            Assert.Null(((IModel)model).SqlServer().HiLoSequenceSchema);

            model.SqlServer().HiLoSequenceSchema = "Tasty.Snook";

            Assert.Equal("Tasty.Snook", model.SqlServer().HiLoSequenceSchema);
            Assert.Equal("Tasty.Snook", ((IModel)model).SqlServer().HiLoSequenceSchema);

            model.SqlServer().HiLoSequenceSchema = null;

            Assert.Null(model.SqlServer().HiLoSequenceSchema);
            Assert.Null(((IModel)model).SqlServer().HiLoSequenceSchema);
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_property()
        {
            var modelBuilder = GetModelBuilder();
            modelBuilder.Model.SqlServer().ValueGenerationStrategy = null;

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.SqlServer().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, ((IProperty)property).SqlServer().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

            property.SqlServer().ValueGenerationStrategy = null;

            Assert.Null(property.SqlServer().ValueGenerationStrategy);
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Can_get_and_set_value_generation_on_nullable_property()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.NullableInt)
                .Metadata;

            Assert.Null(property.SqlServer().ValueGenerationStrategy);

            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.SqlServer().ValueGenerationStrategy);
            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, ((IProperty)property).SqlServer().ValueGenerationStrategy);

            property.SqlServer().ValueGenerationStrategy = null;

            Assert.Null(property.SqlServer().ValueGenerationStrategy);
        }

        [Fact]
        public void Throws_setting_sequence_generation_for_invalid_type()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal(
                SqlServerStrings.SequenceBadType("Name", nameof(Customer), "string"),
                Assert.Throws<ArgumentException>(
                    () => property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo).Message);
        }

        [Fact]
        public void Throws_setting_identity_generation_for_invalid_type()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Name)
                .Metadata;

            Assert.Equal(
                SqlServerStrings.IdentityBadType("Name", nameof(Customer), "string"),
                Assert.Throws<ArgumentException>(
                    () => property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.IdentityColumn).Message);
        }

        [Fact]
        public void Can_get_and_set_sequence_name_on_property()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.SqlServer().HiLoSequenceName);
            Assert.Null(((IProperty)property).SqlServer().HiLoSequenceName);

            property.SqlServer().HiLoSequenceName = "Snook";

            Assert.Equal("Snook", property.SqlServer().HiLoSequenceName);
            Assert.Equal("Snook", ((IProperty)property).SqlServer().HiLoSequenceName);

            property.SqlServer().HiLoSequenceName = null;

            Assert.Null(property.SqlServer().HiLoSequenceName);
            Assert.Null(((IProperty)property).SqlServer().HiLoSequenceName);
        }

        [Fact]
        public void Can_get_and_set_sequence_schema_on_property()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            Assert.Null(property.SqlServer().HiLoSequenceSchema);
            Assert.Null(((IProperty)property).SqlServer().HiLoSequenceSchema);

            property.SqlServer().HiLoSequenceSchema = "Tasty";

            Assert.Equal("Tasty", property.SqlServer().HiLoSequenceSchema);
            Assert.Equal("Tasty", ((IProperty)property).SqlServer().HiLoSequenceSchema);

            property.SqlServer().HiLoSequenceSchema = null;

            Assert.Null(property.SqlServer().HiLoSequenceSchema);
            Assert.Null(((IProperty)property).SqlServer().HiLoSequenceSchema);
        }

        [Fact]
        public void TryGetSequence_returns_null_if_property_is_not_configured_for_sequence_value_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .Metadata;

            modelBuilder.Model.SqlServer().GetOrAddSequence("DaneelOlivaw");

            Assert.Null(property.SqlServer().FindHiLoSequence());
            Assert.Null(((IProperty)property).SqlServer().FindHiLoSequence());

            property.SqlServer().HiLoSequenceName = "DaneelOlivaw";

            Assert.Null(property.SqlServer().FindHiLoSequence());
            Assert.Null(((IProperty)property).SqlServer().FindHiLoSequence());

            modelBuilder.Model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.IdentityColumn;

            Assert.Null(property.SqlServer().FindHiLoSequence());
            Assert.Null(((IProperty)property).SqlServer().FindHiLoSequence());

            modelBuilder.Model.SqlServer().ValueGenerationStrategy = null;
            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.IdentityColumn;

            Assert.Null(property.SqlServer().FindHiLoSequence());
            Assert.Null(((IProperty)property).SqlServer().FindHiLoSequence());
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.SqlServer().GetOrAddSequence("DaneelOlivaw");
            property.SqlServer().HiLoSequenceName = "DaneelOlivaw";
            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;

            Assert.Equal("DaneelOlivaw", property.SqlServer().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().FindHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_default_generation_and_model_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.SqlServer().GetOrAddSequence("DaneelOlivaw");
            modelBuilder.Model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;
            property.SqlServer().HiLoSequenceName = "DaneelOlivaw";

            Assert.Equal("DaneelOlivaw", property.SqlServer().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().FindHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.SqlServer().GetOrAddSequence("DaneelOlivaw");
            modelBuilder.Model.SqlServer().HiLoSequenceName = "DaneelOlivaw";
            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;

            Assert.Equal("DaneelOlivaw", property.SqlServer().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().FindHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_returns_sequence_property_is_marked_for_default_generation_and_model_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.SqlServer().GetOrAddSequence("DaneelOlivaw");
            modelBuilder.Model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;
            modelBuilder.Model.SqlServer().HiLoSequenceName = "DaneelOlivaw";

            Assert.Equal("DaneelOlivaw", property.SqlServer().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().FindHiLoSequence().Name);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.SqlServer().GetOrAddSequence("DaneelOlivaw", "R");
            property.SqlServer().HiLoSequenceName = "DaneelOlivaw";
            property.SqlServer().HiLoSequenceSchema = "R";
            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;

            Assert.Equal("DaneelOlivaw", property.SqlServer().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().FindHiLoSequence().Name);
            Assert.Equal("R", property.SqlServer().FindHiLoSequence().Schema);
            Assert.Equal("R", ((IProperty)property).SqlServer().FindHiLoSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.SqlServer().GetOrAddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;
            property.SqlServer().HiLoSequenceName = "DaneelOlivaw";
            property.SqlServer().HiLoSequenceSchema = "R";

            Assert.Equal("DaneelOlivaw", property.SqlServer().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().FindHiLoSequence().Name);
            Assert.Equal("R", property.SqlServer().FindHiLoSequence().Schema);
            Assert.Equal("R", ((IProperty)property).SqlServer().FindHiLoSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_property_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.SqlServer().GetOrAddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.SqlServer().HiLoSequenceName = "DaneelOlivaw";
            modelBuilder.Model.SqlServer().HiLoSequenceSchema = "R";
            property.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;

            Assert.Equal("DaneelOlivaw", property.SqlServer().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().FindHiLoSequence().Name);
            Assert.Equal("R", property.SqlServer().FindHiLoSequence().Schema);
            Assert.Equal("R", ((IProperty)property).SqlServer().FindHiLoSequence().Schema);
        }

        [Fact]
        public void TryGetSequence_with_schema_returns_sequence_model_is_marked_for_sequence_generation_and_model_has_name()
        {
            var modelBuilder = GetModelBuilder();

            var property = modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .Metadata;

            modelBuilder.Model.SqlServer().GetOrAddSequence("DaneelOlivaw", "R");
            modelBuilder.Model.SqlServer().ValueGenerationStrategy = SqlServerValueGenerationStrategy.SequenceHiLo;
            modelBuilder.Model.SqlServer().HiLoSequenceName = "DaneelOlivaw";
            modelBuilder.Model.SqlServer().HiLoSequenceSchema = "R";

            Assert.Equal("DaneelOlivaw", property.SqlServer().FindHiLoSequence().Name);
            Assert.Equal("DaneelOlivaw", ((IProperty)property).SqlServer().FindHiLoSequence().Name);
            Assert.Equal("R", property.SqlServer().FindHiLoSequence().Schema);
            Assert.Equal("R", ((IProperty)property).SqlServer().FindHiLoSequence().Schema);
        }

        private static ModelBuilder GetModelBuilder() => SqlServerTestHelpers.Instance.CreateConventionBuilder();

        private class Customer
        {
            public int Id { get; set; }
            public int? NullableInt { get; set; }
            public string Name { get; set; }
            public byte Byte { get; set; }
            public byte? NullableByte { get; set; }
            public byte[] ByteArray { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }
            public int CustomerId { get; set; }
        }
    }
}
