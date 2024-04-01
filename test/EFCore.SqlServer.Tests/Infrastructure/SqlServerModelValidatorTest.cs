// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class SqlServerModelValidatorTest : RelationalModelValidatorTest
{
    [ConditionalFact]
    public virtual void Passes_on_TPT_with_nested_owned_types()
    {
        var modelBuilder = base.CreateConventionModelBuilder();

        modelBuilder.Entity<BaseEntity>().UseTptMappingStrategy();
        modelBuilder.Entity<ChildA>();
        modelBuilder.Entity<ChildB>();
        modelBuilder.Entity<ChildC>();
        modelBuilder.Entity<ChildD>();

        Validate(modelBuilder);
    }

    public override void Detects_duplicate_columns_in_derived_types_with_different_types()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();

        modelBuilder.Entity<Cat>().Property(c => c.Type).HasColumnName("Type").IsRequired();
        modelBuilder.Entity<Dog>().Property(d => d.Type).HasColumnName("Type");

        VerifyError(
            RelationalStrings.DuplicateColumnNameDataTypeMismatch(
                nameof(Cat), nameof(Cat.Type), nameof(Dog), nameof(Dog.Type), nameof(Cat.Type), nameof(Animal), "nvarchar(max)",
                "int"), modelBuilder);
    }

    public override void Passes_for_ForeignKey_on_inherited_generated_composite_key_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Abstract>().Property<int>("SomeId").ValueGeneratedOnAdd();
        modelBuilder.Entity<Abstract>().Property<int>("SomeOtherId").ValueGeneratedOnAdd()
            .Metadata.SetValueGenerationStrategy(SqlServerValueGenerationStrategy.None);
        modelBuilder.Entity<Abstract>().HasAlternateKey("SomeId", "SomeOtherId");
        modelBuilder.Entity<Generic<int>>().HasOne<Abstract>().WithOne().HasForeignKey<Generic<int>>("SomeId");
        modelBuilder.Entity<Generic<string>>().Metadata.SetDiscriminatorValue("GenericString");

        Validate(modelBuilder);
    }

    public override void Detects_store_generated_PK_in_TPC()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Animal>(
            b =>
            {
                b.UseTpcMappingStrategy();
                b.Property(e => e.Id).ValueGeneratedOnAdd();
            });

        modelBuilder.Entity<Cat>();

        Validate(modelBuilder);

        var keyProperty = modelBuilder.Model.FindEntityType(typeof(Animal))!.FindProperty(nameof(Animal.Id))!;
        Assert.Equal(ValueGenerated.OnAdd, keyProperty.ValueGenerated);
        Assert.Equal(SqlServerValueGenerationStrategy.Sequence, keyProperty.GetValueGenerationStrategy());
    }

    [ConditionalFact]
    public virtual void Passes_for_duplicate_column_names_within_hierarchy_with_identity()
    {
        var modelBuilder = CreateConventionModelBuilder();
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

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_identity_seed()
    {
        var modelBuilder = CreateConventionModelBuilder();
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
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_identity_increment()
    {
        var modelBuilder = CreateConventionModelBuilder();
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
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_identity_seed_and_increment_on_owner()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().Property(a => a.Id).UseIdentityColumn(2, 3);
        modelBuilder.Entity<Cat>().OwnsOne(a => a.FavoritePerson);
        modelBuilder.Entity<Dog>().Ignore(d => d.FavoritePerson);

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_duplicate_column_names_with_HiLoSequence()
    {
        var modelBuilder = CreateConventionModelBuilder();
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

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_with_different_HiLoSequence_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
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
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_name_with_different_HiLoSequence_schema()
    {
        var modelBuilder = CreateConventionModelBuilder();
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
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_duplicate_column_names_with_KeySequence()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>(
            cb =>
            {
                cb.ToTable("Animal");
                cb.Property(c => c.Id).UseSequence();
            });
        modelBuilder.Entity<Dog>(
            db =>
            {
                db.ToTable("Animal");
                db.Property(d => d.Id).UseSequence();
                db.HasOne<Cat>().WithOne().HasForeignKey<Dog>(d => d.Id);
            });

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_with_different_KeySequence_name()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Cat>(
            cb =>
            {
                cb.ToTable("Animal");
                cb.Property(c => c.Id).HasColumnName("Id").UseSequence("foo");
            });
        modelBuilder.Entity<Dog>(
            db =>
            {
                db.ToTable("Animal");
                db.Property(d => d.Id).HasColumnName("Id").UseSequence("bar");
                db.HasOne<Cat>().WithOne().HasForeignKey<Dog>(d => d.Id);
            });

        VerifyError(
            RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                nameof(Cat), nameof(Cat.Id), nameof(Dog), nameof(Dog.Id), nameof(Cat.Id), nameof(Animal),
                "NEXT VALUE FOR [foo]",
                "NEXT VALUE FOR [bar]"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_name_with_different_KeySequence_schema()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Cat>(
            cb =>
            {
                cb.ToTable("Animal");
                cb.Property(c => c.Id).UseSequence("foo", "dbo");
            });
        modelBuilder.Entity<Dog>(
            db =>
            {
                db.ToTable("Animal");
                db.Property(d => d.Id).UseSequence("foo", "dba");
                db.HasOne<Cat>().WithOne().HasForeignKey<Dog>(d => d.Id);
            });

        VerifyError(
            RelationalStrings.DuplicateColumnNameDefaultSqlMismatch(
                nameof(Cat), nameof(Cat.Id), nameof(Dog), nameof(Dog.Id), nameof(Cat.Id), nameof(Animal),
                "NEXT VALUE FOR [dbo].[foo]",
                "NEXT VALUE FOR [dba].[foo]"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_value_generation_strategy()
    {
        var modelBuilder = CreateConventionModelBuilder();
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
                db.Property(d => d.Identity).UseHiLo().HasColumnName(nameof(Dog.Identity));
            });

        VerifyError(
            SqlServerStrings.DuplicateColumnNameValueGenerationStrategyMismatch(
                nameof(Cat), nameof(Cat.Identity), nameof(Dog), nameof(Dog.Identity), nameof(Cat.Identity), nameof(Animal)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_sparseness()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>(
            cb =>
            {
                cb.ToTable("Animal");
                cb.Property(c => c.Breed).HasColumnName(nameof(Cat.Breed)).IsSparse();
            });
        modelBuilder.Entity<Dog>(
            db =>
            {
                db.ToTable("Animal");
                db.Property(d => d.Breed).HasColumnName(nameof(Dog.Breed));
            });

        VerifyError(
            SqlServerStrings.DuplicateColumnSparsenessMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_incompatible_foreignKeys_within_hierarchy_when_one_name_configured_explicitly_for_sqlServer()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        var fk1 = modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
            .OnDelete(DeleteBehavior.Cascade).HasConstraintName("FK_Animal_Person_Name").Metadata;
        var fk2 = modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(d => d.Name).HasPrincipalKey(p => p.Name)
            .OnDelete(DeleteBehavior.SetNull).Metadata;

        Validate(modelBuilder);

        Assert.Equal("FK_Animal_Person_Name", fk1.GetConstraintName());
        Assert.Equal("FK_Animal_Person_Name1", fk2.GetConstraintName());
    }

    [ConditionalFact]
    public virtual void Passes_for_compatible_duplicate_convention_indexes_for_foreign_keys()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasOne<Person>().WithMany().HasForeignKey(c => c.Name).HasPrincipalKey(p => p.Name)
            .HasConstraintName("FK_Animal_Person_Name");
        modelBuilder.Entity<Dog>().HasOne<Person>().WithMany().HasForeignKey(d => d.Name).HasPrincipalKey(p => p.Name)
            .HasConstraintName("FK_Animal_Person_Name");

        var model = Validate(modelBuilder);

        Assert.Equal("IX_Animal_Name", model.FindEntityType(typeof(Cat)).GetDeclaredIndexes().Single().GetDatabaseName());
        Assert.Equal("IX_Animal_Name", model.FindEntityType(typeof(Dog)).GetDeclaredIndexes().Single().GetDatabaseName());
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_index_names_within_hierarchy_differently_clustered()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasIndex(c => c.Name).HasDatabaseName("IX_Animal_Name");
        modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsClustered().HasDatabaseName("IX_Animal_Name");

        VerifyError(
            SqlServerStrings.DuplicateIndexClusteredMismatch(
                "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                nameof(Animal), "IX_Animal_Name"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_index_names_within_hierarchy_different_fill_factor()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasIndex(c => c.Name).HasDatabaseName("IX_Animal_Name");
        modelBuilder.Entity<Dog>().HasIndex(d => d.Name).HasDatabaseName("IX_Animal_Name").HasFillFactor(30);

        VerifyError(
            SqlServerStrings.DuplicateIndexFillFactorMismatch(
                "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                nameof(Animal), "IX_Animal_Name"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_index_names_within_hierarchy_differently_online()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasIndex(c => c.Name).HasDatabaseName("IX_Animal_Name");
        modelBuilder.Entity<Dog>().HasIndex(d => d.Name).IsCreatedOnline().HasDatabaseName("IX_Animal_Name");

        VerifyError(
            SqlServerStrings.DuplicateIndexOnlineMismatch(
                "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                nameof(Animal), "IX_Animal_Name"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_index_names_within_hierarchy_different_sort_in_tempdb()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasIndex(c => c.Name).HasDatabaseName("IX_Animal_Name");
        modelBuilder.Entity<Dog>().HasIndex(d => d.Name).HasDatabaseName("IX_Animal_Name").SortInTempDb();

        VerifyError(
            SqlServerStrings.DuplicateIndexSortInTempDbMismatch(
                "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                nameof(Animal), "IX_Animal_Name"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_index_names_within_hierarchy_different_data_compression()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasIndex(c => c.Name).HasDatabaseName("IX_Animal_Name");
        modelBuilder.Entity<Dog>().HasIndex(d => d.Name).HasDatabaseName("IX_Animal_Name").UseDataCompression(DataCompressionType.Page);

        VerifyError(
            SqlServerStrings.DuplicateIndexDataCompressionMismatch(
                "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                nameof(Animal), "IX_Animal_Name"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_duplicate_index_names_within_hierarchy_with_different_different_include()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Cat>().HasIndex(c => c.Name).HasDatabaseName("IX_Animal_Name");
        modelBuilder.Entity<Dog>().HasIndex(d => d.Name).HasDatabaseName("IX_Animal_Name").IncludeProperties(nameof(Dog.Identity));

        VerifyError(
            SqlServerStrings.DuplicateIndexIncludedMismatch(
                "{'" + nameof(Dog.Name) + "'}", nameof(Dog),
                "{'" + nameof(Cat.Name) + "'}", nameof(Cat),
                nameof(Animal), "IX_Animal_Name",
                "{'Dog_Identity'}", "{}"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_missing_include_properties()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Dog>().Property(c => c.Type);
        modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).IncludeProperties(nameof(Dog.Type), "Tag");

        VerifyError(SqlServerStrings.IncludePropertyNotFound("Tag", "{'Name'}", nameof(Dog)), modelBuilder);
    }

    [ConditionalFact]
    public void Detects_duplicate_include_properties()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Dog>().Property(c => c.Type);
        modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).IncludeProperties(nameof(Dog.Type), nameof(Dog.Type));

        VerifyError(SqlServerStrings.IncludePropertyDuplicated(nameof(Dog), nameof(Dog.Type), "{'Name'}"), modelBuilder);
    }

    [ConditionalFact]
    public void Detects_indexed_include_properties()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Dog>().Property(c => c.Type);
        modelBuilder.Entity<Dog>().HasIndex(nameof(Dog.Name)).IncludeProperties(nameof(Dog.Name));

        VerifyError(SqlServerStrings.IncludePropertyInIndex(nameof(Dog), nameof(Dog.Name), "{'Name'}"), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_incompatible_memory_optimized_shared_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();

        modelBuilder.Entity<A>().ToTable("Table", tb => tb.IsMemoryOptimized());

        modelBuilder.Entity<B>().ToTable("Table");

        VerifyError(
            SqlServerStrings.IncompatibleTableMemoryOptimizedMismatch("Table", nameof(A), nameof(B), nameof(A), nameof(B)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_incompatible_sql_output_clause_shared_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();

        modelBuilder.Entity<A>().ToTable("Table", tb => tb.UseSqlOutputClause(false));
        modelBuilder.Entity<B>().ToTable("Table", tb => tb.UseSqlOutputClause());

        VerifyError(
            SqlServerStrings.IncompatibleSqlOutputClauseMismatch("Table", nameof(A), nameof(B), nameof(B), nameof(A)),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Passes_for_shared_table_with_only_one_entity_trigger_definition()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<Order>().ToTable("Table", tb => tb.HasTrigger("SomeTrigger"));
        modelBuilder.Entity<Order>().OwnsOne(o => o.OrderDetails).ToTable("Table");

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_incompatible_non_clustered_shared_key()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();

        modelBuilder.Entity<A>().ToTable("Table")
            .HasKey(a => a.Id).IsClustered();
        modelBuilder.Entity<B>().ToTable("Table")
            .HasKey(b => b.Id).IsClustered(false);

        VerifyError(
            SqlServerStrings.DuplicateKeyMismatchedClustering("{'Id'}", nameof(B), "{'Id'}", nameof(A), "Table", "PK_Table"),
            modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_decimal_keys()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .Property<decimal>("Price").HasPrecision(18, 2);
        modelBuilder.Entity<Animal>().HasKey("Price");

        VerifyWarning(
            SqlServerResources.LogDecimalTypeKey(new TestLogger<SqlServerLoggingDefinitions>())
                .GenerateMessage("Price", nameof(Animal)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_default_decimal_mapping()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().Property<decimal>("Price");

        VerifyWarning(
            SqlServerResources.LogDefaultDecimalTypeColumn(new TestLogger<SqlServerLoggingDefinitions>())
                .GenerateMessage("Price", nameof(Animal)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_default_nullable_decimal_mapping()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().Property<decimal?>("Price");

        VerifyWarning(
            SqlServerResources.LogDefaultDecimalTypeColumn(new TestLogger<SqlServerLoggingDefinitions>())
                .GenerateMessage("Price", nameof(Animal)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Does_not_warn_if_decimal_column_has_precision_and_scale()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .Property<decimal>("Price").HasPrecision(18, 2);

        VerifyLogDoesNotContain(
            SqlServerResources.LogDefaultDecimalTypeColumn(new TestLogger<SqlServerLoggingDefinitions>())
                .GenerateMessage("Price", nameof(Animal)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Does_not_warn_if_default_decimal_mapping_has_non_decimal_to_decimal_value_converter()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .Property<decimal>("Price")
            .HasConversion(new TestDecimalToLongConverter());

        VerifyLogDoesNotContain(
            SqlServerResources.LogDefaultDecimalTypeColumn(new TestLogger<SqlServerLoggingDefinitions>())
                .GenerateMessage("Price", nameof(Animal)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Warn_if_default_decimal_mapping_has_decimal_to_decimal_value_converter()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .Property<decimal>("Price")
            .HasConversion(new TestDecimalToDecimalConverter());

        VerifyWarning(
            SqlServerResources.LogDefaultDecimalTypeColumn(new TestLogger<SqlServerLoggingDefinitions>())
                .GenerateMessage("Price", nameof(Animal)), modelBuilder);
    }

    [ConditionalFact]
    public void Detects_byte_identity_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Dog>().Property(d => d.Id).ValueGeneratedNever();
        modelBuilder.Entity<Dog>().Property<byte>("Bite").UseIdentityColumn();

        VerifyWarning(
            SqlServerResources.LogByteIdentityColumn(new TestLogger<SqlServerLoggingDefinitions>())
                .GenerateMessage("Bite", nameof(Dog)), modelBuilder);
    }

    [ConditionalFact]
    public void Detects_nullable_byte_identity_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Dog>().Property(d => d.Id).ValueGeneratedNever();
        modelBuilder.Entity<Dog>().Property<byte?>("Bite").UseIdentityColumn();

        VerifyWarning(
            SqlServerResources.LogByteIdentityColumn(new TestLogger<SqlServerLoggingDefinitions>())
                .GenerateMessage("Bite", nameof(Dog)), modelBuilder);
    }

    [ConditionalFact]
    public void Detects_multiple_identity_properties()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Dog>().Property(d => d.Id).ValueGeneratedNever();

        modelBuilder.Entity<Dog>().Property(c => c.Type).UseIdentityColumn();
        modelBuilder.Entity<Dog>().Property<int?>("Tag").UseIdentityColumn();

        VerifyError(SqlServerStrings.MultipleIdentityColumns("'Dog.Tag', 'Dog.Type'", nameof(Dog)), modelBuilder);
    }

    [ConditionalFact]
    public void Passes_for_non_key_identity()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Dog>().Property(d => d.Id).ValueGeneratedNever();
        modelBuilder.Entity<Dog>().Property(c => c.Type).UseIdentityColumn();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public void Passes_for_non_key_identity_on_model()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.UseIdentityColumns();

        modelBuilder.Entity<Dog>().Property(c => c.Id).ValueGeneratedNever();
        modelBuilder.Entity<Dog>().Property(c => c.Type).ValueGeneratedOnAdd();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public void Passes_for_non_key_SequenceHiLo_on_model()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.UseHiLo();

        modelBuilder.Entity<Dog>().Property(c => c.Type).ValueGeneratedOnAdd();

        Validate(modelBuilder);
    }

    [ConditionalFact]
    public void Passes_for_non_key_KeySequence_on_model()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.UseKeySequences();

        modelBuilder.Entity<Dog>().Property(c => c.Type).ValueGeneratedOnAdd();

        Validate(modelBuilder);
    }

    [ConditionalTheory]
    [InlineData("DefaultValue", "DefaultValueSql")]
    [InlineData("DefaultValue", "ComputedColumnSql")]
    [InlineData("DefaultValueSql", "ComputedColumnSql")]
    public void Metadata_throws_when_setting_conflicting_serverGenerated_values(string firstConfiguration, string secondConfiguration)
    {
        var modelBuilder = CreateConventionModelBuilder();

        var propertyBuilder = modelBuilder.Entity<Dog>().Property<int?>("NullableInt");

        ConfigureProperty(propertyBuilder.Metadata, firstConfiguration, "1");
        ConfigureProperty(propertyBuilder.Metadata, secondConfiguration, "2");

        VerifyError(
            RelationalStrings.ConflictingColumnServerGeneration(firstConfiguration, "NullableInt", secondConfiguration),
            modelBuilder);
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
        var modelBuilder = CreateConventionModelBuilder();

        var propertyBuilder = modelBuilder.Entity<Dog>().Property<int>("Id");

        propertyBuilder.Metadata.SetValueGenerationStrategy(sqlServerValueGenerationStrategy);
        ConfigureProperty(propertyBuilder.Metadata, conflictingValueGenerationStrategy, "NEXT VALUE FOR [Id]");

        VerifyWarning(
            SqlServerResources.LogConflictingValueGenerationStrategies(new TestLogger<SqlServerLoggingDefinitions>())
                .GenerateMessage(sqlServerValueGenerationStrategy.ToString(), conflictingValueGenerationStrategy, "Id", nameof(Dog)),
            modelBuilder);
    }

    [ConditionalTheory]
    [InlineData(SqlServerValueGenerationStrategy.IdentityColumn)]
    [InlineData(SqlServerValueGenerationStrategy.SequenceHiLo)]
    public void SqlServerValueGenerationStrategy_warns_when_setting_conflicting_DefaultValue(
        SqlServerValueGenerationStrategy sqlServerValueGenerationStrategy)
    {
        var modelBuilder = CreateConventionModelBuilder();

        var propertyBuilder = modelBuilder.Entity<Dog>().Property<int>("Id");

        propertyBuilder.Metadata.SetValueGenerationStrategy(sqlServerValueGenerationStrategy);
        ConfigureProperty(propertyBuilder.Metadata, "DefaultValue", "2");

        VerifyWarnings(
            [
                SqlServerResources.LogConflictingValueGenerationStrategies(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage(sqlServerValueGenerationStrategy.ToString(), "DefaultValue", "Id", nameof(Dog)),
                RelationalResources.LogKeyHasDefaultValue(new TestLogger<SqlServerLoggingDefinitions>())
                    .GenerateMessage("Id", nameof(Dog))
            ],
            modelBuilder);
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

    [ConditionalFact]
    public void Temporal_can_only_be_specified_on_root_entities()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();
        modelBuilder.Entity<Dog>().ToTable(tb => tb.IsTemporal());

        VerifyError(SqlServerStrings.TemporalOnlyOnRoot(nameof(Dog)), modelBuilder);
    }

    [ConditionalFact]
    public void Temporal_enitty_must_have_period_start()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Dog>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<Dog>().Metadata.RemoveAnnotation(SqlServerAnnotationNames.TemporalPeriodStartPropertyName);

        VerifyError(SqlServerStrings.TemporalMustDefinePeriodProperties(nameof(Dog)), modelBuilder);
    }

    [ConditionalFact]
    public void Temporal_enitty_must_have_period_end()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Dog>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<Dog>().Metadata.RemoveAnnotation(SqlServerAnnotationNames.TemporalPeriodEndPropertyName);

        VerifyError(SqlServerStrings.TemporalMustDefinePeriodProperties(nameof(Dog)), modelBuilder);
    }

    [ConditionalFact]
    public void Temporal_enitty_without_expected_period_start_property()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Dog>().ToTable(tb => tb.IsTemporal(ttb => ttb.HasPeriodStart("Start")));
        modelBuilder.Entity<Dog>().Metadata.RemoveProperty("Start");

        VerifyError(SqlServerStrings.TemporalExpectedPeriodPropertyNotFound(nameof(Dog), "Start"), modelBuilder);
    }

    [ConditionalFact]
    public void Temporal_period_property_must_be_in_shadow_state()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Human>().ToTable(tb => tb.IsTemporal(ttb => ttb.HasPeriodStart("DateOfBirth")));

        VerifyError(SqlServerStrings.TemporalPeriodPropertyMustBeInShadowState(nameof(Human), "DateOfBirth"), modelBuilder);
    }

    [ConditionalFact]
    public void Temporal_period_property_must_be_non_nullable_datetime()
    {
        var modelBuilder1 = CreateConventionModelBuilder();
        modelBuilder1.Entity<Dog>().Property(typeof(DateTime?), "Start");

        modelBuilder1.Entity<Dog>().ToTable(tb => tb.IsTemporal(ttb => ttb.HasPeriodStart("Start")));

        VerifyError(
            SqlServerStrings.TemporalPeriodPropertyMustBeNonNullableDateTime(nameof(Dog), "Start", nameof(DateTime)), modelBuilder1);

        var modelBuilder2 = CreateConventionModelBuilder();
        modelBuilder2.Entity<Dog>().Property(typeof(int), "Start");

        modelBuilder2.Entity<Dog>().ToTable(tb => tb.IsTemporal(ttb => ttb.HasPeriodStart("Start")));

        VerifyError(
            SqlServerStrings.TemporalPeriodPropertyMustBeNonNullableDateTime(nameof(Dog), "Start", nameof(DateTime)), modelBuilder2);
    }

    [ConditionalFact]
    public void Temporal_period_property_must_be_mapped_to_datetime2()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Dog>().Property(typeof(DateTime), "Start").HasColumnType("datetime");
        modelBuilder.Entity<Dog>().ToTable(tb => tb.IsTemporal(ttb => ttb.HasPeriodStart("Start")));

        VerifyError(SqlServerStrings.TemporalPeriodPropertyMustBeMappedToDatetime2(nameof(Dog), "Start", "datetime2"), modelBuilder);
    }

    [ConditionalFact]
    public void Temporal_all_properties_mapped_to_period_column_must_have_value_generated_OnAddOrUpdate()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Dog>().ToTable(
            tb => tb.IsTemporal(
                ttb =>
                    ttb.HasPeriodStart("Start").HasColumnName("StartColumn").GetInfrastructure().ValueGeneratedNever()));

        VerifyError(
            SqlServerStrings.TemporalPropertyMappedToPeriodColumnMustBeValueGeneratedOnAddOrUpdate(
                nameof(Dog), "Start", nameof(ValueGenerated.OnAddOrUpdate)), modelBuilder);
    }

    [ConditionalFact]
    public void Temporal_period_property_cant_have_default_value()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Dog>().Property(typeof(DateTime), "Start").HasDefaultValue(new DateTime(2000, 1, 1));
        modelBuilder.Entity<Dog>().ToTable(tb => tb.IsTemporal(ttb => ttb.HasPeriodStart("Start")));

        VerifyError(SqlServerStrings.TemporalPeriodPropertyCantHaveDefaultValue(nameof(Dog), "Start"), modelBuilder);
    }

    [ConditionalFact]
    public void Temporal_doesnt_work_on_TPH()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().ToTable(tb => tb.IsTemporal());
        modelBuilder.Entity<Dog>().ToTable("Dogs");
        modelBuilder.Entity<Cat>().ToTable("Cats");

        VerifyError(SqlServerStrings.TemporalOnlySupportedForTPH(nameof(Animal)), modelBuilder);
    }

    [ConditionalFact]
    public void Temporal_doesnt_work_on_table_splitting_with_inconsistent_period_mappings()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Splitting1>().ToTable("Splitting", tb => tb.IsTemporal());
        modelBuilder.Entity<Splitting2>().ToTable("Splitting", tb => tb.IsTemporal());
        modelBuilder.Entity<Splitting1>().HasOne(x => x.Details).WithOne().HasForeignKey<Splitting2>(x => x.Id);

        VerifyError(
            SqlServerStrings.TemporalNotSupportedForTableSplittingWithInconsistentPeriodMapping(
                "start", "Splitting2", "PeriodStart", "Splitting2_PeriodStart", "PeriodStart"), modelBuilder);
    }

    [ConditionalFact]
    public void Temporal_doesnt_work_on_table_splitting_when_some_types_are_temporal_and_some_are_not()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Splitting1>().ToTable("Splitting");
        modelBuilder.Entity<Splitting2>().ToTable("Splitting", tb => tb.IsTemporal());
        modelBuilder.Entity<Splitting1>().HasOne(x => x.Details).WithOne().HasForeignKey<Splitting2>(x => x.Id);

        VerifyError(SqlServerStrings.TemporalAllEntitiesMappedToSameTableMustBeTemporal("Splitting1"), modelBuilder);
    }

    [ConditionalFact]
    public void Temporal_table_with_explicit_precision_on_period_columns_passes_validation()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Human>().ToTable(
            tb => tb.IsTemporal(
                ttb =>
                {
                    ttb.HasPeriodStart("Start").HasPrecision(2);
                    ttb.HasPeriodEnd("End").HasPrecision(2);
                }));

        Validate(modelBuilder);

        var entity = modelBuilder.Model.FindEntityType(typeof(Human));

        Assert.Equal(2, entity.FindProperty("Start").GetPrecision());
        Assert.Equal(2, entity.FindProperty("End").GetPrecision());
    }

    [ConditionalFact]
    public void Temporal_table_with_owned_with_explicit_precision_on_period_columns_passes_validation()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Owner>(
            b =>
            {
                b.ToTable(
                    tb => tb.IsTemporal(
                        ttb =>
                        {
                            ttb.HasPeriodStart("Start").HasColumnName("Start").HasPrecision(2);
                            ttb.HasPeriodEnd("End").HasColumnName("End").HasPrecision(2);
                        }));
                b.OwnsOne(x => x.Owned).ToTable(
                    tb =>
                        tb.IsTemporal(
                            ttb =>
                            {
                                ttb.HasPeriodStart("Start").HasColumnName("Start").HasPrecision(2);
                                ttb.HasPeriodEnd("End").HasColumnName("End").HasPrecision(2);
                            }));
            });

        Validate(modelBuilder);

        var ownerEntity = modelBuilder.Model.FindEntityType(typeof(Owner));
        var ownedEntity = modelBuilder.Model.FindEntityType(typeof(OwnedEntity));

        Assert.Equal(2, ownerEntity.FindProperty("Start").GetPrecision());
        Assert.Equal(2, ownerEntity.FindProperty("End").GetPrecision());
        Assert.Equal(2, ownedEntity.FindProperty("Start").GetPrecision());
        Assert.Equal(2, ownedEntity.FindProperty("End").GetPrecision());
    }

    public class Human
    {
        public int Id { get; set; }
        public DateTime DateOfBirth { get; set; }
    }

    public class Splitting1
    {
        public int Id { get; set; }
        public Splitting2 Details { get; set; }
    }

    public class Splitting2
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Detail { get; set; }
    }

    private class Cheese
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    protected override TestHelpers TestHelpers
        => SqlServerTestHelpers.Instance;
}
