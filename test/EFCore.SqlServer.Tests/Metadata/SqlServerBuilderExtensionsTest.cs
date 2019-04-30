// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata
{
    public class SqlServerBuilderExtensionsTest
    {
        [Fact]
        public void Setting_column_default_value_does_not_set_identity_column()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasDefaultValue(1);

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Null(property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Setting_column_default_value_sql_does_not_set_identity_column()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasDefaultValueSql("1");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Null(property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [Fact]
        public void Can_set_index_filter()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .HasFilter("Generic expression")
                .HasFilter("SqlServer-specific expression");

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.Equal("SqlServer-specific expression", index.GetFilter());
        }

        [Fact]
        public void Can_set_MemoryOptimized()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .ForSqlServerIsMemoryOptimized();

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.True(entityType.GetSqlServerIsMemoryOptimized());

            modelBuilder
                .Entity<Customer>()
                .ForSqlServerIsMemoryOptimized(false);

            Assert.False(entityType.GetSqlServerIsMemoryOptimized());
        }

        [Fact]
        public void Can_set_MemoryOptimized_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForSqlServerIsMemoryOptimized();

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.True(entityType.GetSqlServerIsMemoryOptimized());

            modelBuilder
                .Entity(typeof(Customer))
                .ForSqlServerIsMemoryOptimized(false);

            Assert.False(entityType.GetSqlServerIsMemoryOptimized());
        }

        [Fact]
        public void Can_set_index_clustering()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .ForSqlServerIsClustered();

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.True(index.GetSqlServerIsClustered().Value);
        }

        [Fact]
        public void Can_set_key_clustering()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasKey(e => e.Id)
                .ForSqlServerIsClustered();

            var key = modelBuilder.Model.FindEntityType(typeof(Customer)).FindPrimaryKey();

            Assert.True(key.GetSqlServerIsClustered().Value);
        }

        [Fact]
        public void Can_set_index_include()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Name)
                .ForSqlServerInclude(e => e.Offset);

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.NotNull(index.GetSqlServerIncludeProperties());
            Assert.Collection(
                index.GetSqlServerIncludeProperties(),
                c => Assert.Equal(nameof(Customer.Offset), c));
        }

        [Fact]
        public void Can_set_index_include_after_unique_using_generic_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Name)
                .IsUnique()
                .ForSqlServerInclude(e => e.Offset);

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.True(index.IsUnique);
            Assert.NotNull(index.GetSqlServerIncludeProperties());
            Assert.Collection(
                index.GetSqlServerIncludeProperties(),
                c => Assert.Equal(nameof(Customer.Offset), c));
        }

        [Fact]
        public void Can_set_index_include_after_annotation_using_generic_builder()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Name)
                .HasAnnotation("Test:ShouldBeTrue", true)
                .ForSqlServerInclude(e => e.Offset);

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            var annotation = index.FindAnnotation("Test:ShouldBeTrue");

            Assert.NotNull(annotation);
            Assert.True(annotation.Value as bool?);

            Assert.NotNull(index.GetSqlServerIncludeProperties());
            Assert.Collection(
                index.GetSqlServerIncludeProperties(),
                c => Assert.Equal(nameof(Customer.Offset), c));
        }

        [Fact]
        public void Can_set_index_include_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Name)
                .ForSqlServerInclude(nameof(Customer.Offset));

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.NotNull(index.GetSqlServerIncludeProperties());
            Assert.Collection(
                index.GetSqlServerIncludeProperties(),
                c => Assert.Equal(nameof(Customer.Offset), c));
        }

        [Fact]
        public void Can_set_index_online()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Name)
                .ForSqlServerIsCreatedOnline();

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.True(index.GetSqlServerIsCreatedOnline());
        }

        [Fact]
        public void Can_set_index_online_non_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Name)
                .ForSqlServerIsCreatedOnline();

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.True(index.GetSqlServerIsCreatedOnline());
        }

        [Fact]
        public void Can_set_sequences_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForSqlServerUseSequenceHiLo();

            var relationalExtensions = modelBuilder.Model;
            var sqlServerExtensions = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.GetSqlServerValueGenerationStrategy());
            Assert.Equal(SqlServerModelExtensions.DefaultHiLoSequenceName, sqlServerExtensions.GetSqlServerHiLoSequenceName());
            Assert.Null(sqlServerExtensions.GetSqlServerHiLoSequenceSchema());

            Assert.NotNull(relationalExtensions.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
            Assert.NotNull(sqlServerExtensions.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForSqlServerUseSequenceHiLo("Snook");

            var relationalExtensions = modelBuilder.Model;
            var sqlServerExtensions = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.GetSqlServerValueGenerationStrategy());
            Assert.Equal("Snook", sqlServerExtensions.GetSqlServerHiLoSequenceName());
            Assert.Null(sqlServerExtensions.GetSqlServerHiLoSequenceSchema());

            Assert.NotNull(relationalExtensions.FindSequence("Snook"));

            var sequence = sqlServerExtensions.FindSequence("Snook");

            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForSqlServerUseSequenceHiLo("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model;
            var sqlServerExtensions = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.GetSqlServerValueGenerationStrategy());
            Assert.Equal("Snook", sqlServerExtensions.GetSqlServerHiLoSequenceName());
            Assert.Equal("Tasty", sqlServerExtensions.GetSqlServerHiLoSequenceSchema());

            Assert.NotNull(relationalExtensions.FindSequence("Snook", "Tasty"));

            var sequence = sqlServerExtensions.FindSequence("Snook", "Tasty");
            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            modelBuilder.ForSqlServerUseSequenceHiLo("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model;
            var sqlServerExtensions = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.GetSqlServerValueGenerationStrategy());
            Assert.Equal("Snook", sqlServerExtensions.GetSqlServerHiLoSequenceName());
            Assert.Equal("Tasty", sqlServerExtensions.GetSqlServerHiLoSequenceSchema());

            ValidateSchemaNamedSpecificSequence(relationalExtensions.FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            modelBuilder.ForSqlServerUseSequenceHiLo("Snook", "Tasty");

            var relationalExtensions = modelBuilder.Model;
            var sqlServerExtensions = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.GetSqlServerValueGenerationStrategy());
            Assert.Equal("Snook", sqlServerExtensions.GetSqlServerHiLoSequenceName());
            Assert.Equal("Tasty", sqlServerExtensions.GetSqlServerHiLoSequenceSchema());

            ValidateSchemaNamedSpecificSequence(relationalExtensions.FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.FindSequence("Snook", "Tasty"));
        }

        private static void ValidateSchemaNamedSpecificSequence(ISequence sequence)
        {
            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(11, sequence.IncrementBy);
            Assert.Equal(1729, sequence.StartValue);
            Assert.Equal(111, sequence.MinValue);
            Assert.Equal(2222, sequence.MaxValue);
            Assert.Same(typeof(int), sequence.ClrType);
        }

        [Fact]
        public void Can_set_identities_for_model()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder.ForSqlServerUseIdentityColumns();

            var model = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, model.GetSqlServerValueGenerationStrategy());
            Assert.Equal(SqlServerModelExtensions.DefaultHiLoSequenceName, model.GetSqlServerHiLoSequenceName());
            Assert.Null(model.GetSqlServerHiLoSequenceSchema());

            Assert.Null(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
        }

        [Fact]
        public void Setting_SqlServer_identities_for_model_is_lower_priority_than_relational_default_values()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>(
                    eb =>
                    {
                        eb.Property(e => e.Id).HasDefaultValue(1);
                        eb.Property(e => e.Name).HasComputedColumnSql("Default");
                        eb.Property(e => e.Offset).HasDefaultValueSql("Now");
                    });

            modelBuilder.ForSqlServerUseIdentityColumns();

            var model = modelBuilder.Model;
            var idProperty = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));
            Assert.Null(idProperty.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, idProperty.ValueGenerated);
            Assert.Equal(1, idProperty.GetDefaultValue());
            Assert.Equal(1, idProperty.GetDefaultValue());

            var nameProperty = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Name));
            Assert.Null(nameProperty.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAddOrUpdate, nameProperty.ValueGenerated);
            Assert.Equal("Default", nameProperty.GetComputedColumnSql());
            Assert.Equal("Default", nameProperty.GetComputedColumnSql());

            var offsetProperty = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Offset));
            Assert.Null(offsetProperty.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, offsetProperty.ValueGenerated);
            Assert.Equal("Now", offsetProperty.GetDefaultValueSql());
            Assert.Equal("Now", offsetProperty.GetDefaultValueSql());
        }

        [Fact]
        public void Can_set_sequence_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo();

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal(SqlServerModelExtensions.DefaultHiLoSequenceName, property.GetSqlServerHiLoSequenceName());

            Assert.NotNull(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
            Assert.NotNull(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
        }

        [Fact]
        public void Can_set_sequences_with_name_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo("Snook");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.GetSqlServerHiLoSequenceName());
            Assert.Null(property.GetSqlServerHiLoSequenceSchema());

            Assert.NotNull(model.FindSequence("Snook"));

            var sequence = model.FindSequence("Snook");

            Assert.Equal("Snook", sequence.Name);
            Assert.Null(sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_set_sequences_with_schema_and_name_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.GetSqlServerHiLoSequenceName());
            Assert.Equal("Tasty", property.GetSqlServerHiLoSequenceSchema());

            Assert.NotNull(model.FindSequence("Snook", "Tasty"));

            var sequence = model.FindSequence("Snook", "Tasty");
            Assert.Equal("Snook", sequence.Name);
            Assert.Equal("Tasty", sequence.Schema);
            Assert.Equal(10, sequence.IncrementBy);
            Assert.Equal(1, sequence.StartValue);
            Assert.Null(sequence.MinValue);
            Assert.Null(sequence.MaxValue);
            Assert.Same(typeof(long), sequence.ClrType);
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.GetSqlServerHiLoSequenceName());
            Assert.Equal("Tasty", property.GetSqlServerHiLoSequenceSchema());

            ValidateSchemaNamedSpecificSequence(model.FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_relational_sequence_for_property_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty", b => b.IncrementsBy(11).StartsAt(1729).HasMin(111).HasMax(2222))
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.GetSqlServerHiLoSequenceName());
            Assert.Equal("Tasty", property.GetSqlServerHiLoSequenceSchema());

            ValidateSchemaNamedSpecificSequence(model.FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.GetSqlServerHiLoSequenceName());
            Assert.Equal("Tasty", property.GetSqlServerHiLoSequenceSchema());

            ValidateSchemaNamedSpecificSequence(model.FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_use_of_existing_SQL_sequence_for_property_using_nested_closure()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>(
                    "Snook", "Tasty", b =>
                    {
                        b.IncrementsBy(11)
                            .StartsAt(1729)
                            .HasMin(111)
                            .HasMax(2222);
                    })
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServerUseSequenceHiLo("Snook", "Tasty");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.GetSqlServerHiLoSequenceName());
            Assert.Equal("Tasty", property.GetSqlServerHiLoSequenceSchema());

            ValidateSchemaNamedSpecificSequence(model.FindSequence("Snook", "Tasty"));
        }

        [Fact]
        public void Can_set_identities_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServerUseIdentityColumn();

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal(1, property.GetSqlServerIdentitySeed());
            Assert.Equal(1, property.GetSqlServerIdentityIncrement());
            Assert.Null(property.GetSqlServerHiLoSequenceName());

            Assert.Null(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
            Assert.Null(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
        }

        [Fact]
        public void Can_set_identities_with_seed_and_identity_for_property()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .ForSqlServerUseIdentityColumn(100, 5);

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, property.GetSqlServerValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal(100, property.GetSqlServerIdentitySeed());
            Assert.Equal(5, property.GetSqlServerIdentityIncrement());
            Assert.Null(property.GetSqlServerHiLoSequenceName());

            Assert.Null(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
            Assert.Null(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
        }

        [Fact]
        public void SqlServer_entity_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .ForSqlServerIsMemoryOptimized());
        }

        [Fact]
        public void SqlServer_entity_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .ForSqlServerIsMemoryOptimized();
        }

        [Fact]
        public void SqlServer_property_methods_dont_break_out_of_the_generics()
        {
            var modelBuilder = CreateConventionModelBuilder();

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .ForSqlServerUseSequenceHiLo());

            AssertIsGeneric(
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .ForSqlServerUseIdentityColumn());
        }

        [Fact]
        public void SqlServer_property_methods_have_non_generic_overloads()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(int), "Id")
                .ForSqlServerUseSequenceHiLo();

            modelBuilder
                .Entity(typeof(Customer))
                .Property(typeof(int), "Id")
                .ForSqlServerUseIdentityColumn();
        }

        [Fact]
        public void Can_write_index_filter_with_where_clauses_generic()
        {
            var modelBuilder = CreateConventionModelBuilder();

            var returnedBuilder = modelBuilder
                .Entity<Customer>()
                .HasIndex(e => e.Id)
                .HasFilter("[Id] % 2 = 0");

            AssertIsGeneric(returnedBuilder);
            Assert.IsType<IndexBuilder<Customer>>(returnedBuilder);

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();
            Assert.Equal("[Id] % 2 = 0", index.GetFilter());
        }

        private void AssertIsGeneric(EntityTypeBuilder<Customer> _)
        {
        }

        private void AssertIsGeneric(PropertyBuilder<int> _)
        {
        }

        private void AssertIsGeneric(IndexBuilder<Customer> _)
        {
        }

        protected virtual ModelBuilder CreateConventionModelBuilder()
            => SqlServerTestHelpers.Instance.CreateConventionBuilder();

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTimeOffset Offset { get; set; }

            public IEnumerable<Order> Orders { get; set; }
        }

        private class Order
        {
            public int OrderId { get; set; }

            public int CustomerId { get; set; }
            public Customer Customer { get; set; }

            public OrderDetails Details { get; set; }
        }

        private class OrderDetails
        {
            public int Id { get; set; }

            public int OrderId { get; set; }
            public Order Order { get; set; }
        }
    }
}
