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
        [ConditionalFact]
        public void Setting_column_default_value_does_not_set_identity_column()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasDefaultValue(1);

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Equal(SqlServerValueGenerationStrategy.None, property.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [ConditionalFact]
        public void Setting_column_default_value_sql_does_not_set_identity_column()
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .Entity<Customer>()
                .Property(e => e.Id)
                .HasDefaultValueSql("1");

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));

            Assert.Equal(SqlServerValueGenerationStrategy.None, property.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
        }

        [ConditionalFact]
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

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_MemoryOptimized(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .ForSqlServerIsMemoryOptimized();
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .IsMemoryOptimized();
            }

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.True(entityType.IsMemoryOptimized());

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .ForSqlServerIsMemoryOptimized(false);
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .IsMemoryOptimized(false);
            }

            Assert.False(entityType.IsMemoryOptimized());
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_MemoryOptimized_non_generic(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity(typeof(Customer))
                    .ForSqlServerIsMemoryOptimized();
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity(typeof(Customer))
                    .IsMemoryOptimized();
            }

            var entityType = modelBuilder.Model.FindEntityType(typeof(Customer));

            Assert.True(entityType.IsMemoryOptimized());

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity(typeof(Customer))
                    .ForSqlServerIsMemoryOptimized(false);
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity(typeof(Customer))
                    .IsMemoryOptimized(false);
            }

            Assert.False(entityType.IsMemoryOptimized());
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_index_clustering(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Id)
                    .ForSqlServerIsClustered();
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Id)
                    .IsClustered();
            }

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.True(index.IsClustered().Value);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_key_clustering(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .HasKey(e => e.Id)
                    .ForSqlServerIsClustered();
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .HasKey(e => e.Id)
                    .IsClustered();
            }

            var key = modelBuilder.Model.FindEntityType(typeof(Customer)).FindPrimaryKey();

            Assert.True(key.IsClustered().Value);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_index_include(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Name)
                    .ForSqlServerInclude(e => e.Offset);
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Name)
                    .IncludeProperties(e => e.Offset);
            }

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.NotNull(index.GetIncludeProperties());
            Assert.Collection(
                index.GetIncludeProperties(),
                c => Assert.Equal(nameof(Customer.Offset), c));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_index_include_after_unique_using_generic_builder(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Name)
                    .IsUnique()
                    .ForSqlServerInclude(e => e.Offset);
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Name)
                    .IsUnique()
                    .IncludeProperties(e => e.Offset);
            }

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.True(index.IsUnique);
            Assert.NotNull(index.GetIncludeProperties());
            Assert.Collection(
                index.GetIncludeProperties(),
                c => Assert.Equal(nameof(Customer.Offset), c));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_index_include_after_annotation_using_generic_builder(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Name)
                    .HasAnnotation("Test:ShouldBeTrue", true)
                    .ForSqlServerInclude(e => e.Offset);
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Name)
                    .HasAnnotation("Test:ShouldBeTrue", true)
                    .IncludeProperties(e => e.Offset);
            }

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            var annotation = index.FindAnnotation("Test:ShouldBeTrue");

            Assert.NotNull(annotation);
            Assert.True(annotation.Value as bool?);

            Assert.NotNull(index.GetIncludeProperties());
            Assert.Collection(
                index.GetIncludeProperties(),
                c => Assert.Equal(nameof(Customer.Offset), c));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_index_include_non_generic(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Name)
                    .ForSqlServerInclude(nameof(Customer.Offset));
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Name)
                    .IncludeProperties(nameof(Customer.Offset));
            }

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.NotNull(index.GetIncludeProperties());
            Assert.Collection(
                index.GetIncludeProperties(),
                c => Assert.Equal(nameof(Customer.Offset), c));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_index_online(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Name)
                    .ForSqlServerIsCreatedOnline();
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Name)
                    .IsCreatedOnline();
            }

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.True(index.IsCreatedOnline());
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_index_online_non_generic(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Name)
                    .ForSqlServerIsCreatedOnline();
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .HasIndex(e => e.Name)
                    .IsCreatedOnline();
            }

            var index = modelBuilder.Model.FindEntityType(typeof(Customer)).GetIndexes().Single();

            Assert.True(index.IsCreatedOnline());
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_sequences_for_model(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.ForSqlServerUseSequenceHiLo();
#pragma warning restore 618
            }
            else
            {
                modelBuilder.UseHiLo();
            }

            var relationalExtensions = modelBuilder.Model;
            var sqlServerExtensions = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.GetValueGenerationStrategy());
            Assert.Equal(SqlServerModelExtensions.DefaultHiLoSequenceName, sqlServerExtensions.GetHiLoSequenceName());
            Assert.Null(sqlServerExtensions.GetHiLoSequenceSchema());

            Assert.NotNull(relationalExtensions.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
            Assert.NotNull(sqlServerExtensions.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_sequences_with_name_for_model(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.ForSqlServerUseSequenceHiLo("Snook");
#pragma warning restore 618
            }
            else
            {
                modelBuilder.UseHiLo("Snook");
            }

            var relationalExtensions = modelBuilder.Model;
            var sqlServerExtensions = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.GetValueGenerationStrategy());
            Assert.Equal("Snook", sqlServerExtensions.GetHiLoSequenceName());
            Assert.Null(sqlServerExtensions.GetHiLoSequenceSchema());

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

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_sequences_with_schema_and_name_for_model(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.ForSqlServerUseSequenceHiLo("Snook", "Tasty");
#pragma warning restore 618
            }
            else
            {
                modelBuilder.UseHiLo("Snook", "Tasty");
            }

            var relationalExtensions = modelBuilder.Model;
            var sqlServerExtensions = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.GetValueGenerationStrategy());
            Assert.Equal("Snook", sqlServerExtensions.GetHiLoSequenceName());
            Assert.Equal("Tasty", sqlServerExtensions.GetHiLoSequenceSchema());

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

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_use_of_existing_relational_sequence_for_model(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.ForSqlServerUseSequenceHiLo("Snook", "Tasty");
#pragma warning restore 618
            }
            else
            {
                modelBuilder.UseHiLo("Snook", "Tasty");
            }

            var relationalExtensions = modelBuilder.Model;
            var sqlServerExtensions = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.GetValueGenerationStrategy());
            Assert.Equal("Snook", sqlServerExtensions.GetHiLoSequenceName());
            Assert.Equal("Tasty", sqlServerExtensions.GetHiLoSequenceSchema());

            ValidateSchemaNamedSpecificSequence(relationalExtensions.FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(sqlServerExtensions.FindSequence("Snook", "Tasty"));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_use_of_existing_SQL_sequence_for_model(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.ForSqlServerUseSequenceHiLo("Snook", "Tasty");
#pragma warning restore 618
            }
            else
            {
                modelBuilder.UseHiLo("Snook", "Tasty");
            }

            var relationalExtensions = modelBuilder.Model;
            var sqlServerExtensions = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, sqlServerExtensions.GetValueGenerationStrategy());
            Assert.Equal("Snook", sqlServerExtensions.GetHiLoSequenceName());
            Assert.Equal("Tasty", sqlServerExtensions.GetHiLoSequenceSchema());

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

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_identities_for_model(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.ForSqlServerUseIdentityColumns();
#pragma warning restore 618
            }
            else
            {
                modelBuilder.UseIdentityColumns();
            }

            var model = modelBuilder.Model;

            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, model.GetValueGenerationStrategy());
            Assert.Equal(SqlServerModelExtensions.DefaultHiLoSequenceName, model.GetHiLoSequenceName());
            Assert.Null(model.GetHiLoSequenceSchema());

            Assert.Null(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Setting_SqlServer_identities_for_model_is_lower_priority_than_relational_default_values(bool obsolete)
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

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.ForSqlServerUseIdentityColumns();
#pragma warning restore 618
            }
            else
            {
                modelBuilder.UseIdentityColumns();
            }

            var model = modelBuilder.Model;
            var idProperty = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Id));
            Assert.Equal(SqlServerValueGenerationStrategy.None, idProperty.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, idProperty.ValueGenerated);
            Assert.Equal(1, idProperty.GetDefaultValue());
            Assert.Equal(1, idProperty.GetDefaultValue());

            var nameProperty = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Name));
            Assert.Equal(SqlServerValueGenerationStrategy.None, nameProperty.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAddOrUpdate, nameProperty.ValueGenerated);
            Assert.Equal("Default", nameProperty.GetComputedColumnSql());
            Assert.Equal("Default", nameProperty.GetComputedColumnSql());

            var offsetProperty = model.FindEntityType(typeof(Customer)).FindProperty(nameof(Customer.Offset));
            Assert.Equal(SqlServerValueGenerationStrategy.None, offsetProperty.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, offsetProperty.ValueGenerated);
            Assert.Equal("Now", offsetProperty.GetDefaultValueSql());
            Assert.Equal("Now", offsetProperty.GetDefaultValueSql());
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_sequence_for_property(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .ForSqlServerUseSequenceHiLo();
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseHiLo();
            }

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal(SqlServerModelExtensions.DefaultHiLoSequenceName, property.GetHiLoSequenceName());

            Assert.NotNull(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
            Assert.NotNull(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_sequences_with_name_for_property(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .ForSqlServerUseSequenceHiLo("Snook");
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseHiLo("Snook");
            }

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.GetHiLoSequenceName());
            Assert.Null(property.GetHiLoSequenceSchema());

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

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_sequences_with_schema_and_name_for_property(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .ForSqlServerUseSequenceHiLo("Snook", "Tasty");
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseHiLo("Snook", "Tasty");
            }

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.GetHiLoSequenceName());
            Assert.Equal("Tasty", property.GetHiLoSequenceSchema());

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

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_use_of_existing_relational_sequence_for_property(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .ForSqlServerUseSequenceHiLo("Snook", "Tasty");
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseHiLo("Snook", "Tasty");
            }

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.GetHiLoSequenceName());
            Assert.Equal("Tasty", property.GetHiLoSequenceSchema());

            ValidateSchemaNamedSpecificSequence(model.FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.FindSequence("Snook", "Tasty"));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_use_of_existing_relational_sequence_for_property_using_nested_closure(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .HasSequence<int>("Snook", "Tasty", b => b.IncrementsBy(11).StartsAt(1729).HasMin(111).HasMax(2222))
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .ForSqlServerUseSequenceHiLo("Snook", "Tasty");
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .HasSequence<int>("Snook", "Tasty", b => b.IncrementsBy(11).StartsAt(1729).HasMin(111).HasMax(2222))
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseHiLo("Snook", "Tasty");
            }

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.GetHiLoSequenceName());
            Assert.Equal("Tasty", property.GetHiLoSequenceSchema());

            ValidateSchemaNamedSpecificSequence(model.FindSequence("Snook", "Tasty"));
            ValidateSchemaNamedSpecificSequence(model.FindSequence("Snook", "Tasty"));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_use_of_existing_SQL_sequence_for_property(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            modelBuilder
                .HasSequence<int>("Snook", "Tasty")
                .IncrementsBy(11)
                .StartsAt(1729)
                .HasMin(111)
                .HasMax(2222);

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .ForSqlServerUseSequenceHiLo("Snook", "Tasty");
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseHiLo("Snook", "Tasty");
            }

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.GetHiLoSequenceName());
            Assert.Equal("Tasty", property.GetHiLoSequenceSchema());

            ValidateSchemaNamedSpecificSequence(model.FindSequence("Snook", "Tasty"));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_use_of_existing_SQL_sequence_for_property_using_nested_closure(bool obsolete)
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
                    });

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .ForSqlServerUseSequenceHiLo("Snook", "Tasty");
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseHiLo("Snook", "Tasty");
            }

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.SequenceHiLo, property.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal("Snook", property.GetHiLoSequenceName());
            Assert.Equal("Tasty", property.GetHiLoSequenceSchema());

            ValidateSchemaNamedSpecificSequence(model.FindSequence("Snook", "Tasty"));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_identities_for_property(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseSqlServerIdentityColumn();
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseIdentityColumn();
            }

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, property.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal(1, property.GetIdentitySeed());
            Assert.Equal(1, property.GetIdentityIncrement());
            Assert.Null(property.GetHiLoSequenceName());

            Assert.Null(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
            Assert.Null(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Can_set_identities_with_seed_and_identity_for_property(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseSqlServerIdentityColumn(100, 5);
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity<Customer>()
                    .Property(e => e.Id)
                    .UseIdentityColumn(100, 5);
            }

            var model = modelBuilder.Model;
            var property = model.FindEntityType(typeof(Customer)).FindProperty("Id");

            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, property.GetValueGenerationStrategy());
            Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
            Assert.Equal(100, property.GetIdentitySeed());
            Assert.Equal(5, property.GetIdentityIncrement());
            Assert.Null(property.GetHiLoSequenceName());

            Assert.Null(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
            Assert.Null(model.FindSequence(SqlServerModelExtensions.DefaultHiLoSequenceName));
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void SqlServer_entity_methods_dont_break_out_of_the_generics(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                AssertIsGeneric(
                    modelBuilder
                        .Entity<Customer>()
                        .ForSqlServerIsMemoryOptimized());
#pragma warning restore 618
            }
            else
            {
                AssertIsGeneric(
                    modelBuilder
                        .Entity<Customer>()
                        .IsMemoryOptimized());
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void SqlServer_entity_methods_have_non_generic_overloads(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity(typeof(Customer))
                    .ForSqlServerIsMemoryOptimized();
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity(typeof(Customer))
                    .IsMemoryOptimized();
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void SqlServer_property_methods_dont_break_out_of_the_generics(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                AssertIsGeneric(
                    modelBuilder
                        .Entity<Customer>()
                        .Property(e => e.Id)
                        .ForSqlServerUseSequenceHiLo());

                AssertIsGeneric(
                    modelBuilder
                        .Entity<Customer>()
                        .Property(e => e.Id)
                        .UseSqlServerIdentityColumn());
#pragma warning restore 618
            }
            else
            {
                AssertIsGeneric(
                    modelBuilder
                        .Entity<Customer>()
                        .Property(e => e.Id)
                        .UseHiLo());

                AssertIsGeneric(
                    modelBuilder
                        .Entity<Customer>()
                        .Property(e => e.Id)
                        .UseIdentityColumn());
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void SqlServer_property_methods_have_non_generic_overloads(bool obsolete)
        {
            var modelBuilder = CreateConventionModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder
                    .Entity(typeof(Customer))
                    .Property(typeof(int), "Id")
                    .ForSqlServerUseSequenceHiLo();

                modelBuilder
                    .Entity(typeof(Customer))
                    .Property(typeof(int), "Id")
                    .UseSqlServerIdentityColumn();
#pragma warning restore 618
            }
            else
            {
                modelBuilder
                    .Entity(typeof(Customer))
                    .Property(typeof(int), "Id")
                    .UseHiLo();

                modelBuilder
                    .Entity(typeof(Customer))
                    .Property(typeof(int), "Id")
                    .UseIdentityColumn();
            }
        }

        [ConditionalFact]
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
