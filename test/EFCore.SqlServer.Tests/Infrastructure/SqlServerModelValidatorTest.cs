// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    public class SqlServerModelValidatorTest : RelationalModelValidatorTest
    {
        public override void Detects_duplicate_column_names()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            GenerateMapping(modelBuilder.Entity<Animal>().Property(b => b.Id).HasColumnName("Name").Metadata);
            GenerateMapping(modelBuilder.Entity<Animal>().Property(d => d.Name).IsRequired().HasColumnName("Name").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Animal), nameof(Animal.Id),
                    nameof(Animal), nameof(Animal.Name), "Name", nameof(Animal), "int", "nvarchar(max)"),
                modelBuilder.Model);
        }

        public override void Detects_incompatible_shared_columns_with_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();
            modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().Property(a => a.P0).HasColumnName(nameof(A.P0));
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(A), nameof(A.P0), nameof(B), nameof(B.P0), nameof(B.P0), "Table", "someInt", "int"), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_duplicate_column_names_within_hierarchy_with_identity()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().Property(a => a.Id).ValueGeneratedNever();
            modelBuilder.Entity<Cat>(
                cb =>
                {
                    cb.Property(c => c.Identity).UseIdentityColumn(2, 3).HasColumnName(nameof(Cat.Identity));
                });
            modelBuilder.Entity<Dog>(
                db =>
                {
                    db.Property(d => d.Identity).UseIdentityColumn(2, 3).HasColumnName(nameof(Dog.Identity));
                });

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_identity_seed()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(
                cb =>
                {
                    cb.Property(c => c.Identity).UseIdentityColumn().HasColumnName(nameof(Cat.Identity));
                });
            modelBuilder.Entity<Dog>(
                db =>
                {
                    db.Property(d => d.Identity).UseIdentityColumn(2).HasColumnName(nameof(Dog.Identity));
                });

            VerifyError(
                SqlServerStrings.DuplicateColumnIdentitySeedMismatch(
                    nameof(Cat), nameof(Cat.Identity), nameof(Dog), nameof(Dog.Identity), nameof(Cat.Identity), nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_identity_increment()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(
                cb =>
                {
                    cb.Property(c => c.Identity).UseIdentityColumn().HasColumnName(nameof(Cat.Identity));
                });
            modelBuilder.Entity<Dog>(
                db =>
                {
                    db.Property(d => d.Identity).UseIdentityColumn(increment: 2).HasColumnName(nameof(Dog.Identity));
                });

            VerifyError(
                SqlServerStrings.DuplicateColumnIdentityIncrementMismatch(
                    nameof(Cat), nameof(Cat.Identity), nameof(Dog), nameof(Dog.Identity), nameof(Cat.Identity), nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_identity_seed_and_increment_on_owner()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().Property(a => a.Id).UseIdentityColumn(2, 3);
            modelBuilder.Entity<Cat>().OwnsOne(a => a.FavoritePerson);
            modelBuilder.Entity<Dog>();

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_duplicate_column_names_with_HiLoSequence()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Cat>(
                cb =>
                {
                    cb.ToTable("Animal");
                    cb.Property(c => c.Id).UseHiLo();
                });
            modelBuilder.Entity<Dog>(
                db =>
                {
                    db.ToTable("Animal");
                    db.Property(d => d.Id).UseHiLo();
                    db.HasOne<Cat>().WithOne().HasForeignKey<Dog>(d => d.Id);
                });

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_with_different_HiLoSequence_name()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Cat>(
                cb =>
                {
                    cb.ToTable("Animal");
                    cb.Property(c => c.Id).UseHiLo("foo");
                });
            modelBuilder.Entity<Dog>(
                db =>
                {
                    db.ToTable("Animal");
                    db.Property(d => d.Id).UseHiLo();
                    db.HasOne<Cat>().WithOne().HasForeignKey<Dog>(d => d.Id);
                });

            VerifyError(
                SqlServerStrings.DuplicateColumnSequenceMismatch(
                    nameof(Cat), nameof(Cat.Id), nameof(Dog), nameof(Dog.Id), nameof(Cat.Id), nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_name_with_different_HiLoSequence_schema()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Cat>(
                cb =>
                {
                    cb.ToTable("Animal");
                    cb.Property(c => c.Id).UseHiLo("foo", "dbo");
                });
            modelBuilder.Entity<Dog>(
                db =>
                {
                    db.ToTable("Animal");
                    db.Property(d => d.Id).UseHiLo("foo", "dba");
                    db.HasOne<Cat>().WithOne().HasForeignKey<Dog>(d => d.Id);
                });

            VerifyError(
                SqlServerStrings.DuplicateColumnSequenceMismatch(
                    nameof(Cat), nameof(Cat.Id), nameof(Dog), nameof(Dog.Id), nameof(Cat.Id), nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_value_generation_strategy()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(
                cb =>
                {
                    cb.Property(c => c.Id).ValueGeneratedNever();
                    cb.Property(c => c.Identity).UseIdentityColumn().HasColumnName(nameof(Cat.Identity));
                });
            modelBuilder.Entity<Dog>(
                db =>
                {
                    db.Property(d => d.Identity).ValueGeneratedNever().HasColumnName(nameof(Dog.Identity));
                });

            VerifyError(
                SqlServerStrings.DuplicateColumnNameValueGenerationStrategyMismatch(
                    nameof(Cat), nameof(Cat.Identity), nameof(Dog), nameof(Dog.Identity), nameof(Cat.Identity), nameof(Animal)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Passes_for_incompatible_foreignKeys_within_hierarchy_when_one_name_configured_explicitly_for_sqlServer()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            var fk1 = modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
                .OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_Animal_Person_Name").Metadata;
            var fk2 = modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(d => d.Name).HasPrincipalKey(p => p.Name)
                .OnDelete(DeleteBehavior.SetNull).Metadata;

            Validate(modelBuilder.Model);

            Assert.Equal("FK_Animal_Person_Name", fk1.GetConstraintName());
            Assert.Equal("FK_Animal_Person_Name1", fk2.GetConstraintName());
        }

        [ConditionalFact]
        public virtual void Passes_for_compatible_duplicate_convention_indexes_for_foreign_keys()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
                .HasConstraintName("FK_Animal_Person_Name");
            modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(d => d.Name).HasPrincipalKey(p => p.Name)
                .HasConstraintName("FK_Animal_Person_Name");

            var model = Validate(modelBuilder.Model);

            Assert.Equal("IX_Animal_Name", model.FindEntityType(typeof(Cat)).GetDeclaredIndexes().Single().GetDatabaseName());
            Assert.Equal("IX_Animal_Name", model.FindEntityType(typeof(Dog)).GetDeclaredIndexes().Single().GetDatabaseName());
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_index_names_within_hierarchy_differently_clustered()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().HasIndex(c => c.Name).HasDatabaseName("IX_Animal_Name");
            modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsClustered().HasDatabaseName("IX_Animal_Name");

            VerifyError(
                SqlServerStrings.DuplicateIndexClusteredMismatch(
                    "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                    "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                    nameof(Animal), "IX_Animal_Name"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_index_names_within_hierarchy_differently_online()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().HasIndex(c => c.Name).HasDatabaseName("IX_Animal_Name");
            modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsCreatedOnline().HasDatabaseName("IX_Animal_Name");

            VerifyError(
                SqlServerStrings.DuplicateIndexOnlineMismatch(
                    "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                    "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                    nameof(Animal), "IX_Animal_Name"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_index_names_within_hierarchy_with_different_different_include()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>().HasIndex(c => c.Name).HasDatabaseName("IX_Animal_Name");
            modelBuilder.Entity<Dog>().HasIndex(d => d.Name).HasDatabaseName("IX_Animal_Name").IncludeProperties(nameof(Dog.Identity));

            VerifyError(
                SqlServerStrings.DuplicateIndexIncludedMismatch(
                    "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                    "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                    nameof(Animal), "IX_Animal_Name",
                    "{'Dog_Identity'}", "{}"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_missing_include_properties()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(c => c.Type);
            modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).IncludeProperties(nameof(Dog.Type), "Tag");

            VerifyError(SqlServerStrings.IncludePropertyNotFound(nameof(Dog), "Tag"), modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_duplicate_include_properties()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(c => c.Type);
            modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).IncludeProperties(nameof(Dog.Type), nameof(Dog.Type));

            VerifyError(SqlServerStrings.IncludePropertyDuplicated(nameof(Dog), nameof(Dog.Type)), modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_indexed_include_properties()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(c => c.Type);
            modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).IncludeProperties(nameof(Dog.Name));

            VerifyError(SqlServerStrings.IncludePropertyInIndex(nameof(Dog), nameof(Dog.Name)), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_incompatible_memory_optimized_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();

            modelBuilder.Entity<A>().ToTable("Table").IsMemoryOptimized();

            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                SqlServerStrings.IncompatibleTableMemoryOptimizedMismatch("Table", nameof(A), nameof(B), nameof(A), nameof(B)),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_incompatible_non_clustered_shared_key()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();

            modelBuilder.Entity<A>().ToTable("Table")
                .HasKey(a => a.Id).IsClustered();
            modelBuilder.Entity<B>().ToTable("Table")
                .HasKey(b => b.Id).IsClustered(false);

            VerifyError(
                SqlServerStrings.DuplicateKeyMismatchedClustering("{'Id'}", nameof(B), "{'Id'}", nameof(A), "Table", "PK_Table"),
                modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_decimal_keys()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>()
                .Property<decimal>("Price").HasPrecision(18, 2);
            modelBuilder.Entity<Animal>().HasKey("Price");

            VerifyWarning(
                SqlServerResources.LogDecimalTypeKey(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage("Price", nameof(Animal)), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_default_decimal_mapping()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().Property<decimal>("Price");

            VerifyWarning(
                SqlServerResources.LogDefaultDecimalTypeColumn(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage("Price", nameof(Animal)), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_default_nullable_decimal_mapping()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>().Property<decimal?>("Price");

            VerifyWarning(
                SqlServerResources.LogDefaultDecimalTypeColumn(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage("Price", nameof(Animal)), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Does_not_warn_if_decimal_column_has_precision_and_scale()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>()
                .Property<decimal>("Price").HasPrecision(18, 2);

            VerifyLogDoesNotContain(
                SqlServerResources.LogDefaultDecimalTypeColumn(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage("Price", nameof(Animal)), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Does_not_warn_if_default_decimal_mapping_has_non_decimal_to_decimal_value_converter()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>()
                .Property<decimal>("Price")
                .HasConversion(new TestDecimalToLongConverter());

            VerifyLogDoesNotContain(
                SqlServerResources.LogDefaultDecimalTypeColumn(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage("Price", nameof(Animal)), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Warn_if_default_decimal_mapping_has_decimal_to_decimal_value_converter()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>()
                .Property<decimal>("Price")
                .HasConversion(new TestDecimalToDecimalConverter());

            VerifyWarning(
                SqlServerResources.LogDefaultDecimalTypeColumn(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage("Price", nameof(Animal)), modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_byte_identity_column()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(d => d.Id).ValueGeneratedNever();
            modelBuilder.Entity<Dog>().Property<byte>("Bite").UseIdentityColumn();

            VerifyWarning(
                SqlServerResources.LogByteIdentityColumn(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage("Bite", nameof(Dog)), modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_nullable_byte_identity_column()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(d => d.Id).ValueGeneratedNever();
            modelBuilder.Entity<Dog>().Property<byte?>("Bite").UseIdentityColumn();

            VerifyWarning(
                SqlServerResources.LogByteIdentityColumn(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage("Bite", nameof(Dog)), modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_multiple_identity_properties()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(d => d.Id).ValueGeneratedNever();

            modelBuilder.Entity<Dog>().Property(c => c.Type).UseIdentityColumn();
            modelBuilder.Entity<Dog>().Property<int?>("Tag").UseIdentityColumn();

            VerifyError(SqlServerStrings.MultipleIdentityColumns("'Dog.Tag', 'Dog.Type'", nameof(Dog)), modelBuilder.Model);
        }

        [ConditionalFact]
        public void Passes_for_non_key_identity()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(d => d.Id).ValueGeneratedNever();
            modelBuilder.Entity<Dog>().Property(c => c.Type).UseIdentityColumn();

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public void Passes_for_non_key_identity_on_model()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.UseIdentityColumns();

            modelBuilder.Entity<Dog>().Property(c => c.Id).ValueGeneratedNever();
            modelBuilder.Entity<Dog>().Property(c => c.Type).ValueGeneratedOnAdd();

            Validate(modelBuilder.Model);
        }

        [ConditionalFact]
        public void Detects_non_key_SequenceHiLo()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(c => c.Type).UseHiLo();

            VerifyError(SqlServerStrings.NonKeyValueGeneration(nameof(Dog.Type), nameof(Dog)), modelBuilder.Model);
        }

        [ConditionalFact]
        public void Passes_for_non_key_SequenceHiLo_on_model()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.UseHiLo();

            modelBuilder.Entity<Dog>().Property(c => c.Type).ValueGeneratedOnAdd();

            Validate(modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData("DefaultValue", "DefaultValueSql")]
        [InlineData("DefaultValue", "ComputedColumnSql")]
        [InlineData("DefaultValueSql", "ComputedColumnSql")]
        public void Metadata_throws_when_setting_conflicting_serverGenerated_values(string firstConfiguration, string secondConfiguration)
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var propertyBuilder = modelBuilder.Entity<Dog>().Property<int?>("NullableInt");

            ConfigureProperty(propertyBuilder.Metadata, firstConfiguration, "1");
            ConfigureProperty(propertyBuilder.Metadata, secondConfiguration, "2");

            VerifyError(
                RelationalStrings.ConflictingColumnServerGeneration(firstConfiguration, "NullableInt", secondConfiguration),
                modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(SqlServerValueGenerationStrategy.IdentityColumn, "DefaultValueSql")]
        [InlineData(SqlServerValueGenerationStrategy.IdentityColumn, "ComputedColumnSql")]
        [InlineData(SqlServerValueGenerationStrategy.SequenceHiLo, "DefaultValueSql")]
        [InlineData(SqlServerValueGenerationStrategy.SequenceHiLo, "ComputedColumnSql")]
        public void SqlServerValueGenerationStrategy_warns_when_setting_conflicting_value_generation_strategies(
            SqlServerValueGenerationStrategy sqlServerValueGenerationStrategy,
            string conflictingValueGenerationStrategy)
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var propertyBuilder = modelBuilder.Entity<Dog>().Property<int>("Id");

            propertyBuilder.Metadata.SetValueGenerationStrategy(sqlServerValueGenerationStrategy);
            ConfigureProperty(propertyBuilder.Metadata, conflictingValueGenerationStrategy, "NEXT VALUE FOR [Id]");

            VerifyWarning(
                SqlServerResources.LogConflictingValueGenerationStrategies(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage(sqlServerValueGenerationStrategy.ToString(), conflictingValueGenerationStrategy, "Id", nameof(Dog)),
                modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(SqlServerValueGenerationStrategy.IdentityColumn)]
        [InlineData(SqlServerValueGenerationStrategy.SequenceHiLo)]
        public void SqlServerValueGenerationStrategy_warns_when_setting_conflicting_DefaultValue(
            SqlServerValueGenerationStrategy sqlServerValueGenerationStrategy)
        {
            var modelBuilder = CreateConventionalModelBuilder();

            var propertyBuilder = modelBuilder.Entity<Dog>().Property<int>("Id");

            propertyBuilder.Metadata.SetValueGenerationStrategy(sqlServerValueGenerationStrategy);
            ConfigureProperty(propertyBuilder.Metadata, "DefaultValue", "2");

            VerifyWarnings(
                new[]
                {
                    SqlServerResources.LogConflictingValueGenerationStrategies(new TestLogger<SqlServerLoggingDefinitions>())
                        .GenerateMessage(sqlServerValueGenerationStrategy.ToString(), "DefaultValue", "Id", nameof(Dog)),
                    RelationalResources.LogKeyHasDefaultValue(new TestLogger<SqlServerLoggingDefinitions>())
                        .GenerateMessage("Id", nameof(Dog))
                },
                modelBuilder.Model);
        }

        protected virtual void ConfigureProperty(IMutableProperty property, string configuration, string value)
        {
            switch (configuration)
            {
                case "DefaultValue":
                    property.SetDefaultValue(int.Parse(value));
                    break;
                case "DefaultValueSql":
                    property.SetDefaultValueSql(value);
                    break;
                case "ComputedColumnSql":
                    property.SetComputedColumnSql(value);
                    break;
                case "SqlServerValueGenerationStrategy":
                    property.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.IdentityColumn);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private static void GenerateMapping(IMutableProperty property)
            => property.SetTypeMapping(
                new SqlServerTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())
                    .FindMapping(property));

        private class Cheese
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        protected override TestHelpers TestHelpers
            => SqlServerTestHelpers.Instance;
    }
}
