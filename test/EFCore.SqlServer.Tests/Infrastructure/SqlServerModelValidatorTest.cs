// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
            GenerateMapping(modelBuilder.Entity<Animal>().Property(d => d.Name).HasColumnName("Name").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Animal), nameof(Animal.Id),
                    nameof(Animal), nameof(Animal.Name), "Name", nameof(Animal), "int", "nvarchar(max)"),
                modelBuilder.Model);
        }

        public override void Detects_duplicate_columns_in_derived_types_with_different_types()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Type).HasColumnName("Type").Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(c => c.Type).HasColumnName("Type").Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    typeof(Cat).Name, "Type", typeof(Dog).Name, "Type", "Type", nameof(Animal), "nvarchar(max)", "int"),
                modelBuilder.Model);
        }

        public override void Detects_incompatible_shared_columns_with_shared_table()
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);
            modelBuilder.Entity<A>().Property(a => a.P0).HasColumnName(nameof(A.P0)).HasColumnType("someInt");
            modelBuilder.Entity<A>().ToTable("Table");
            modelBuilder.Entity<B>().Property(a => a.P0).HasColumnName(nameof(A.P0));
            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(A), nameof(A.P0), nameof(B), nameof(B.P0), nameof(B.P0), "Table", "someInt", "int"), modelBuilder.Model);
        }

        public override void Detects_duplicate_column_names_within_hierarchy_with_different_MaxLength()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasMaxLength(30).Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed").HasMaxLength(15).Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "nvarchar(30)",
                    "nvarchar(15)"), modelBuilder.Model);
        }

        [ConditionalFact]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_unicode()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();

            GenerateMapping(modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").IsUnicode(false).Metadata);
            GenerateMapping(modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed").IsUnicode().Metadata);

            VerifyError(
                RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                    nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal), "varchar(max)",
                    "nvarchar(max)"), modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_value_generation_strategy(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            modelBuilder.Entity<Cat>(
                cb =>
                {
                    if (obsolete)
                    {
#pragma warning disable 618
                        cb.Property(c => c.Identity).UseSqlServerIdentityColumn();
#pragma warning restore 618
                    }
                    else
                    {
                        cb.Property(c => c.Identity).UseIdentityColumn();
                    }

                    cb.Property(c => c.Identity).HasColumnName(nameof(Cat.Identity));
                });
            modelBuilder.Entity<Dog>(
                db =>
                {
                    db.Property(d => d.Identity).ValueGeneratedNever();
                    db.Property(c => c.Identity).HasColumnName(nameof(Dog.Identity));
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
        public virtual void Passes_for_incompatible_indexes_within_hierarchy_when_one_name_configured_explicitly_for_sqlServer()
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Animal>();
            var index1 = modelBuilder.Entity<Cat>().HasIndex(c => c.Name).IsUnique().HasName("IX_Animal_Name").Metadata;
            var index2 = modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsUnique(false).Metadata;

            Validate(modelBuilder.Model);

            Assert.Equal("IX_Animal_Name", index1.GetName());
            Assert.Equal("IX_Animal_Name1", index2.GetName());
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Detects_incompatible_memory_optimized_shared_table(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.Entity<A>().ToTable("Table").ForSqlServerIsMemoryOptimized();
#pragma warning restore 618
            }
            else
            {
                modelBuilder.Entity<A>().ToTable("Table").IsMemoryOptimized();
            }

            modelBuilder.Entity<B>().ToTable("Table");

            VerifyError(
                SqlServerStrings.IncompatibleTableMemoryOptimizedMismatch("Table", nameof(A), nameof(B), nameof(A), nameof(B)),
                modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Detects_incompatible_non_clustered_shared_key(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();

            modelBuilder.Entity<A>().HasOne<B>().WithOne().IsRequired().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id);

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.Entity<A>().ToTable("Table")
                    .HasKey(a => a.Id).ForSqlServerIsClustered();
                modelBuilder.Entity<B>().ToTable("Table")
                    .HasKey(b => b.Id).ForSqlServerIsClustered(false);
#pragma warning restore 618
            }
            else
            {
                modelBuilder.Entity<A>().ToTable("Table")
                    .HasKey(a => a.Id).IsClustered();
                modelBuilder.Entity<B>().ToTable("Table")
                    .HasKey(b => b.Id).IsClustered(false);
            }

            VerifyError(
                SqlServerStrings.DuplicateKeyMismatchedClustering("{'Id'}", nameof(B), "{'Id'}", nameof(A), "Table", "PK_Table"),
                modelBuilder.Model);
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

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Detects_byte_identity_column(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(d => d.Id).ValueGeneratedNever();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.Entity<Dog>().Property<byte>("Bite").UseSqlServerIdentityColumn();
#pragma warning restore 618
            }
            else
            {
                modelBuilder.Entity<Dog>().Property<byte>("Bite").UseIdentityColumn();
            }

            VerifyWarning(
                SqlServerResources.LogByteIdentityColumn(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage("Bite", nameof(Dog)), modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Detects_nullable_byte_identity_column(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(d => d.Id).ValueGeneratedNever();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.Entity<Dog>().Property<byte?>("Bite").UseSqlServerIdentityColumn();
#pragma warning restore 618
            }
            else
            {
                modelBuilder.Entity<Dog>().Property<byte?>("Bite").UseIdentityColumn();
            }

            VerifyWarning(
                SqlServerResources.LogByteIdentityColumn(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage("Bite", nameof(Dog)), modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Passes_for_non_key_identity(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(d => d.Id).ValueGeneratedNever();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.Entity<Dog>().Property(c => c.Type).UseSqlServerIdentityColumn();
#pragma warning restore 618
            }
            else
            {
                modelBuilder.Entity<Dog>().Property(c => c.Type).UseIdentityColumn();
            }

            Validate(modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Detects_multiple_identity_properties(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(d => d.Id).ValueGeneratedNever();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.Entity<Dog>().Property(c => c.Type).UseSqlServerIdentityColumn();
                modelBuilder.Entity<Dog>().Property<int?>("Tag").UseSqlServerIdentityColumn();
#pragma warning restore 618
            }
            else
            {
                modelBuilder.Entity<Dog>().Property(c => c.Type).UseIdentityColumn();
                modelBuilder.Entity<Dog>().Property<int?>("Tag").UseIdentityColumn();
            }

            VerifyError(SqlServerStrings.MultipleIdentityColumns("'Dog.Tag', 'Dog.Type'", nameof(Dog)), modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Detects_non_key_SequenceHiLo(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.Entity<Dog>().Property(c => c.Type).ForSqlServerUseSequenceHiLo();
#pragma warning restore 618
            }
            else
            {
                modelBuilder.Entity<Dog>().Property(c => c.Type).UseHiLo();
            }

            VerifyError(SqlServerStrings.NonKeyValueGeneration(nameof(Dog.Type), nameof(Dog)), modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Passes_for_non_key_identity_on_model(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();

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

            modelBuilder.Entity<Dog>().Property(c => c.Id).ValueGeneratedNever();
            modelBuilder.Entity<Dog>().Property(c => c.Type).ValueGeneratedOnAdd();

            Validate(modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Passes_for_non_key_SequenceHiLo_on_model(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();

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

            modelBuilder.Entity<Dog>().Property(c => c.Type).ValueGeneratedOnAdd();

            Validate(modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData("DefaultValue", "DefaultValueSql")]
        [InlineData("DefaultValue", "ComputedColumnSql")]
        [InlineData("DefaultValueSql", "ComputedColumnSql")]
        [InlineData("SqlServerValueGenerationStrategy", "DefaultValue")]
        [InlineData("SqlServerValueGenerationStrategy", "DefaultValueSql")]
        [InlineData("SqlServerValueGenerationStrategy", "ComputedColumnSql")]
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

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Detects_missing_include_properties(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(c => c.Type);

            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).ForSqlServerInclude(nameof(Dog.Type), "Tag");
#pragma warning restore 618
            }
            else
            {
                modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).IncludeProperties(nameof(Dog.Type), "Tag");
            }

            VerifyError(SqlServerStrings.IncludePropertyNotFound(nameof(Dog), "Tag"), modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Detects_duplicate_include_properties(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(c => c.Type);
            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).ForSqlServerInclude(nameof(Dog.Type), nameof(Dog.Type));
#pragma warning restore 618
            }
            else
            {
                modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).IncludeProperties(nameof(Dog.Type), nameof(Dog.Type));
            }

            VerifyError(SqlServerStrings.IncludePropertyDuplicated(nameof(Dog), nameof(Dog.Type)), modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Detects_indexed_include_properties(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(c => c.Type);
            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).ForSqlServerInclude(nameof(Dog.Name));
#pragma warning restore 618
            }
            else
            {
                modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).IncludeProperties(nameof(Dog.Name));
            }

            VerifyError(SqlServerStrings.IncludePropertyInIndex(nameof(Dog), nameof(Dog.Name)), modelBuilder.Model);
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public void Passes_for_online_index(bool obsolete)
        {
            var modelBuilder = CreateConventionalModelBuilder();
            modelBuilder.Entity<Dog>().Property(c => c.Type);
            if (obsolete)
            {
#pragma warning disable 618
                modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).ForSqlServerIsCreatedOnline();
#pragma warning restore 618
            }
            else
            {
                modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).IsCreatedOnline();
            }

            Validate(modelBuilder.Model);
        }

        private static void GenerateMapping(IMutableProperty property)
            => property[CoreAnnotationNames.TypeMapping] =
                new SqlServerTypeMappingSource(
                        TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                        TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())
                    .FindMapping(property);

        private class Cheese
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }

        protected override TestHelpers TestHelpers => SqlServerTestHelpers.Instance;
    }
}
