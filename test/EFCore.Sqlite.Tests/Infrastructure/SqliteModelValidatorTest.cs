// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Infrastructure;

public class SqliteModelValidatorTest : RelationalModelValidatorTest
{
    [ConditionalFact]
    public virtual void Detects_duplicate_column_names_within_hierarchy_with_different_srid()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>();

        modelBuilder.Entity<Cat>().Property(c => c.Breed).HasColumnName("Breed").HasSrid(30);
        modelBuilder.Entity<Dog>().Property(d => d.Breed).HasColumnName("Breed").HasSrid(15);

        VerifyError(
            SqliteStrings.DuplicateColumnNameSridMismatch(
                nameof(Cat), nameof(Cat.Breed), nameof(Dog), nameof(Dog.Breed), nameof(Cat.Breed), nameof(Animal)), modelBuilder);
    }

    [ConditionalFact]
    public void Detects_schemas()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>().ToTable("Animals", "pet").Ignore(a => a.FavoritePerson);

        VerifyWarning(
            SqliteResources.LogSchemaConfigured(new TestLogger<SqliteLoggingDefinitions>()).GenerateMessage("Animal", "pet"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_sequences()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.HasSequence("Fibonacci");

        VerifyWarning(
            SqliteResources.LogSequenceConfigured(new TestLogger<SqliteLoggingDefinitions>()).GenerateMessage("Fibonacci"),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_insert_stored_procedures()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Person>()
            .InsertUsingStoredProcedure(
                "Person_Insert",
                spb => spb
                    .HasParameter(w => w.Id, pb => pb.IsOutput())
                    .HasParameter(w => w.Name)
                    .HasParameter(w => w.FavoriteBreed));

        VerifyError(SqliteStrings.StoredProceduresNotSupported(nameof(Person)), modelBuilder);
    }

    [ConditionalFact]
    public void Detects_update_stored_procedures()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Person>()
            .UpdateUsingStoredProcedure(
                "Person_Update",
                spb => spb
                    .HasOriginalValueParameter(w => w.Id)
                    .HasParameter(w => w.Name)
                    .HasParameter(w => w.FavoriteBreed));

        VerifyError(SqliteStrings.StoredProceduresNotSupported(nameof(Person)), modelBuilder);
    }

    [ConditionalFact]
    public void Detects_delete_stored_procedures()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Person>()
            .DeleteUsingStoredProcedure("Person_Delete", spb => spb.HasOriginalValueParameter(w => w.Id));

        VerifyError(SqliteStrings.StoredProceduresNotSupported(nameof(Person)), modelBuilder);
    }

    [ConditionalFact]
    public virtual void Detects_incompatible_sql_returning_clause_shared_table()
    {
        var modelBuilder = CreateConventionModelBuilder();

        modelBuilder.Entity<A>().HasOne<B>().WithOne().HasForeignKey<A>(a => a.Id).HasPrincipalKey<B>(b => b.Id).IsRequired();

        modelBuilder.Entity<A>().ToTable("Table", tb => tb.UseSqlReturningClause(false));
        modelBuilder.Entity<B>().ToTable("Table", tb => tb.UseSqlReturningClause());

        VerifyError(
            SqliteStrings.IncompatibleSqlReturningClauseMismatch("Table", nameof(A), nameof(B), nameof(B), nameof(A)),
            modelBuilder);
    }

    public override void Passes_for_stored_procedure_without_parameter_for_insert_non_save_property()
    {
        var exception =
            Assert.Throws<InvalidOperationException>(()
                => base.Passes_for_stored_procedure_without_parameter_for_insert_non_save_property());

        Assert.Equal(SqliteStrings.StoredProceduresNotSupported(nameof(Animal)), exception.Message);
    }

    public override void Passes_for_stored_procedure_without_parameter_for_update_non_save_property()
    {
        var exception =
            Assert.Throws<InvalidOperationException>(()
                => base.Passes_for_stored_procedure_without_parameter_for_update_non_save_property());

        Assert.Equal(SqliteStrings.StoredProceduresNotSupported(nameof(Animal)), exception.Message);
    }

    public override void Passes_on_valid_UsingDeleteStoredProcedure_in_TPT()
    {
        var exception =
            Assert.Throws<InvalidOperationException>(() => base.Passes_on_valid_UsingDeleteStoredProcedure_in_TPT());

        Assert.Equal(SqliteStrings.StoredProceduresNotSupported(nameof(Animal)), exception.Message);
    }

    public override void Passes_on_derived_entity_type_mapped_to_a_stored_procedure_in_TPT()
    {
        var exception =
            Assert.Throws<InvalidOperationException>(() => base.Passes_on_derived_entity_type_mapped_to_a_stored_procedure_in_TPT());

        Assert.Equal(SqliteStrings.StoredProceduresNotSupported(nameof(Cat)), exception.Message);
    }

    public override void Passes_on_derived_entity_type_not_mapped_to_a_stored_procedure_in_TPT()
    {
        var exception =
            Assert.Throws<InvalidOperationException>(() => base.Passes_on_derived_entity_type_not_mapped_to_a_stored_procedure_in_TPT());

        Assert.Equal(SqliteStrings.StoredProceduresNotSupported(nameof(Animal)), exception.Message);
    }

    public override void Detects_unmapped_concurrency_token()
    {
        var exception =
            Assert.Throws<InvalidOperationException>(() => base.Detects_unmapped_concurrency_token());

        Assert.Equal(SqliteStrings.StoredProceduresNotSupported(nameof(Animal)), exception.Message);
    }

    public override void Detects_non_generated_update_stored_procedure_result_columns_in_TPT()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_missing_generated_stored_procedure_parameters_in_TPT()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_missing_generated_stored_procedure_parameters_in_TPC()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_multiple_entity_types_mapped_to_the_same_stored_procedure()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_current_value_parameter_on_delete_stored_procedure()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_InsertUsingStoredProcedure_without_a_name()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_non_key_delete_stored_procedure_params_in_TPH()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_stored_procedure_input_parameter_for_update_non_save_property()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_unmatched_stored_procedure_result_columns_in_TPC()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_rows_affected_with_result_columns()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_original_value_parameter_on_insert_stored_procedure()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_non_generated_insert_stored_procedure_result_columns_in_TPH()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_unmatched_stored_procedure_result_columns_in_TPH()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_generated_properties_mapped_to_result_and_parameter()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_missing_stored_procedure_parameters_for_abstract_properties_in_TPT()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_unmatched_stored_procedure_result_columns_in_TPT()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_delete_stored_procedure_result_columns_in_TPH()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_unmatched_stored_procedure_parameters_in_TPT()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_generated_properties_mapped_to_original_and_current_parameter()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_tableless_entity_type_mapped_to_some_stored_procedures()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_duplicate_parameter()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_missing_generated_stored_procedure_parameters()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_non_generated_insert_stored_procedure_output_parameter_in_TPC()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_unmatched_stored_procedure_parameters_in_TPC()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_unmatched_stored_procedure_parameters_in_TPH()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_keyless_entity_type_mapped_to_a_stored_procedure()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_non_generated_update_stored_procedure_input_output_parameter()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_stored_procedure_input_parameter_for_insert_non_save_property()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_missing_generated_stored_procedure_parameters_in_TPH()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_rows_affected_on_insert_stored_procedure()
        => VerifyStoredProceduresNotSupported();

    public override void Detects_duplicate_result_column()
        => VerifyStoredProceduresNotSupported();

    private void VerifyStoredProceduresNotSupported()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Animal>()
            .InsertUsingStoredProcedure("Insert", s => s.HasParameter(a => a.Id));

        VerifyError(SqliteStrings.StoredProceduresNotSupported(nameof(Animal)), modelBuilder);
    }

    [ConditionalFact]
    public void Detects_conflicting_autoincrement_and_default_value_sql()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Person>()
            .Property(e => e.Id)
            .UseAutoincrement()
            .HasDefaultValueSql("42");

        VerifyWarning(
            SqliteResources.LogConflictingValueGenerationStrategies(
                new TestLogger<SqliteLoggingDefinitions>()).GenerateMessage("Autoincrement", "DefaultValueSql", "Id", nameof(Person)),
            modelBuilder);
    }

    [ConditionalFact]
    public void Detects_conflicting_autoincrement_and_computed_column()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<Person>()
            .Property(e => e.Id)
            .UseAutoincrement()
            .HasComputedColumnSql("42");

        VerifyWarning(
            SqliteResources.LogConflictingValueGenerationStrategies(
                new TestLogger<SqliteLoggingDefinitions>()).GenerateMessage("Autoincrement", "ComputedColumnSql", "Id", nameof(Person)),
            modelBuilder);
    }

    public override void Store_generated_in_composite_key()
    {
        var modelBuilder = CreateConventionModelBuilder();
        modelBuilder.Entity<CarbonComposite>(b =>
        {
            b.HasKey(e => new { e.Id1, e.Id2 });
            b.Property(e => e.Id2).ValueGeneratedOnAdd();
        });

        VerifyWarning(
            SqliteResources.LogCompositeKeyWithValueGeneration(
                new TestLogger<SqliteLoggingDefinitions>()).GenerateMessage(nameof(CarbonComposite), "{'Id1', 'Id2'}"),
            modelBuilder);
    }

    protected override TestHelpers TestHelpers
        => SqliteTestHelpers.Instance;
}
