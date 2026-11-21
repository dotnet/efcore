// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

/// <summary>
///     Tests for how index filters affect the creation and removal of foreign key indexes. See #24237.
/// </summary>
public class RelationalForeignKeyIndexConventionTest
{
    [Fact]
    public void Unfiltered_index_covers_foreign_key_index()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId });

        Assert.Equal(["PeriodId,CompanyId"], DescribeIndexes(modelBuilder));
    }

    [Fact]
    public void Filtered_index_does_not_cover_foreign_key_index()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId }).HasFilter("x");

        Assert.Equal(["PeriodId", "PeriodId,CompanyId WHERE x"], DescribeIndexes(modelBuilder));
    }

    [Fact]
    public void Foreign_key_index_is_restored_when_filter_is_added_to_existing_index()
    {
        var modelBuilder = CreateModelBuilder();
        var indexBuilder = modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId });

        Assert.Equal(["PeriodId,CompanyId"], DescribeIndexes(modelBuilder));

        indexBuilder.HasFilter("x");

        Assert.Equal(["PeriodId", "PeriodId,CompanyId WHERE x"], DescribeIndexes(modelBuilder));
    }

    [Fact]
    public void Foreign_key_index_is_removed_again_when_filter_is_removed()
    {
        var modelBuilder = CreateModelBuilder();
        var indexBuilder = modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId }).HasFilter("x");

        indexBuilder.HasFilter(null);

        Assert.Equal(["PeriodId,CompanyId"], DescribeIndexes(modelBuilder));
    }

    [Fact]
    public void Filtered_index_declared_before_foreign_key_does_not_prevent_foreign_key_index()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId }).HasFilter("x");
        modelBuilder.Entity<Invoice>().HasOne(i => i.Period).WithMany().HasForeignKey(i => i.PeriodId);

        Assert.Equal(["PeriodId", "PeriodId,CompanyId WHERE x"], DescribeIndexes(modelBuilder));
    }

    [Fact]
    public void Changing_the_filter_value_does_not_duplicate_the_foreign_key_index()
    {
        var modelBuilder = CreateModelBuilder();
        var indexBuilder = modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId }).HasFilter("x");

        indexBuilder.HasFilter("y");

        Assert.Equal(["PeriodId", "PeriodId,CompanyId WHERE y"], DescribeIndexes(modelBuilder));
    }

    [Fact]
    public void Filter_set_by_convention_still_covers_foreign_key_index()
    {
        var modelBuilder = CreateModelBuilder();
        var index = modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId }).Metadata;

        ((IConventionIndex)index).Builder.HasFilter("[CompanyId] IS NOT NULL");

        Assert.Equal(["PeriodId,CompanyId WHERE [CompanyId] IS NOT NULL"], DescribeIndexes(modelBuilder));
    }

    [Fact]
    public void Filter_set_from_data_annotation_does_not_cover_foreign_key_index()
    {
        var modelBuilder = CreateModelBuilder();
        var index = modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId }).Metadata;

        ((IConventionIndex)index).Builder.HasFilter("x", fromDataAnnotation: true);

        Assert.Equal(["PeriodId", "PeriodId,CompanyId WHERE x"], DescribeIndexes(modelBuilder));
    }

    [Fact]
    public void Filter_upgraded_from_convention_to_explicit_with_the_same_value_restores_foreign_key_index()
    {
        // Only the configuration source of the filter changes here, not its value, so no annotation change is reported.
        var modelBuilder = CreateModelBuilder();
        var indexBuilder = modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId });
        ((IConventionIndex)indexBuilder.Metadata).Builder.HasFilter("x");

        Assert.Equal(["PeriodId,CompanyId WHERE x"], DescribeIndexes(modelBuilder));

        indexBuilder.HasFilter("x");

        Assert.Equal(["PeriodId", "PeriodId,CompanyId WHERE x"], DescribeIndexes(modelBuilder, finalize: true));
    }

    [Fact]
    public void Foreign_key_index_is_not_created_when_another_unfiltered_index_covers_it()
    {
        var modelBuilder = CreateModelBuilder();
        var indexBuilder = modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId });
        modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.Id });

        indexBuilder.HasFilter("x");

        Assert.Equal(["PeriodId,CompanyId WHERE x", "PeriodId,Id"], DescribeIndexes(modelBuilder));
    }

    [Fact]
    public void Removing_filter_does_not_remove_explicitly_configured_foreign_key_index()
    {
        var modelBuilder = CreateModelBuilder();
        var indexBuilder = modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId }).HasFilter("x");
        modelBuilder.Entity<Invoice>().HasIndex(i => i.PeriodId);

        indexBuilder.HasFilter(null);

        Assert.Equal(["PeriodId", "PeriodId,CompanyId"], DescribeIndexes(modelBuilder));
    }

    [Fact]
    public void Unnamed_filtered_index_on_exactly_the_foreign_key_columns_is_the_foreign_key_index()
    {
        // An unnamed index on the same properties is the same index in the model, so there can't be a second one.
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Invoice>().HasIndex(i => i.PeriodId).HasFilter("x");

        Assert.Equal(["PeriodId WHERE x"], DescribeIndexes(modelBuilder));
    }

    // The filter can't be interpreted, and a separate index would have the same columns as the named one, so (as for an unnamed index)
    // the named index is the foreign key's index.
    [Fact]
    public void Named_filtered_index_on_exactly_the_foreign_key_columns_is_the_foreign_key_index()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Invoice>().HasIndex(["PeriodId"], "Named").HasFilter("x");

        var index = Assert.Single(modelBuilder.Model.FindEntityType(typeof(Invoice))!.GetIndexes());
        Assert.Equal("Named", index.Name);
        Assert.Equal("x", index.GetFilter());
    }

    [Fact]
    public void Named_unique_filtered_index_on_exactly_the_unique_foreign_key_columns_is_the_foreign_key_index()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Invoice>().HasOne(i => i.Period).WithOne().HasForeignKey<Invoice>(i => i.PeriodId);
        modelBuilder.Entity<Invoice>().HasIndex(["PeriodId"], "Named").IsUnique().HasFilter("x");

        var index = Assert.Single(modelBuilder.Model.FindEntityType(typeof(Invoice))!.GetIndexes());
        Assert.Equal("Named", index.Name);
        Assert.Equal("x", index.GetFilter());
        Assert.True(index.IsUnique);
    }

    // What `dotnet ef dbcontext scaffold` generates for the index that migrations create for an optional one-to-one relationship.
    [Fact]
    public void Named_explicitly_filtered_unique_index_on_the_foreign_key_columns_is_not_duplicated_and_keeps_its_name()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<OptionalInvoice>().HasIndex(i => i.PeriodId, "IX_Invoice_PeriodId").IsUnique()
            .HasFilter("([PeriodId] IS NOT NULL)");
        modelBuilder.Entity<OptionalInvoice>().HasOne(i => i.Period).WithOne().HasForeignKey<OptionalInvoice>(i => i.PeriodId);

        var index = Assert.Single(modelBuilder.FinalizeModel().FindEntityType(typeof(OptionalInvoice))!.GetIndexes());
        Assert.Equal("IX_Invoice_PeriodId", index.Name);
        Assert.Equal("IX_Invoice_PeriodId", index.GetDatabaseName());
    }

    [Fact]
    public void Filter_set_by_convention_covers_foreign_key_index_if_the_nullable_columns_are_foreign_key_columns()
    {
        var modelBuilder = CreateOptionalModelBuilder();
        var index = modelBuilder.Entity<OptionalInvoice>().HasIndex(i => new { i.PeriodId, i.RequiredId }).IsUnique().Metadata;

        ((IConventionIndex)index).Builder.HasFilter("[PeriodId] IS NOT NULL");

        Assert.Equal(["PeriodId,RequiredId WHERE [PeriodId] IS NOT NULL"], DescribeOptionalIndexes(modelBuilder));
    }

    // The filter excludes the rows where OptionalId is null, but a lookup on PeriodId needs them.
    [Fact]
    public void Filter_set_by_convention_does_not_cover_foreign_key_index_if_another_nullable_column_is_filtered()
    {
        var modelBuilder = CreateOptionalModelBuilder();
        var index = modelBuilder.Entity<OptionalInvoice>().HasIndex(i => new { i.PeriodId, i.OptionalId }).IsUnique().Metadata;

        Assert.Equal(["PeriodId,OptionalId"], DescribeOptionalIndexes(modelBuilder));

        ((IConventionIndex)index).Builder.HasFilter("[PeriodId] IS NOT NULL AND [OptionalId] IS NOT NULL");

        Assert.Equal(
            ["PeriodId", "PeriodId,OptionalId WHERE [PeriodId] IS NOT NULL AND [OptionalId] IS NOT NULL"],
            DescribeOptionalIndexes(modelBuilder));

        ((IConventionIndex)index).Builder.HasFilter(null);

        Assert.Equal(["PeriodId,OptionalId"], DescribeOptionalIndexes(modelBuilder));
    }

    // Raising the configuration source of a filter without changing its value isn't reported as a change.
    [Fact]
    public void Filter_upgraded_to_explicit_with_the_same_value_keeps_foreign_key_index_when_the_index_is_removed()
    {
        var modelBuilder = CreateOptionalModelBuilder();
        var indexBuilder = modelBuilder.Entity<OptionalInvoice>().HasIndex(i => new { i.PeriodId, i.OptionalId });
        ((IConventionIndex)indexBuilder.Metadata).Builder.HasFilter("x");

        indexBuilder.HasFilter("x");
        modelBuilder.Entity<OptionalInvoice>().Metadata.RemoveIndex(indexBuilder.Metadata);

        Assert.Equal(["PeriodId"], DescribeOptionalIndexes(modelBuilder, finalize: true));
    }

    [Fact]
    public void Filter_upgraded_to_explicit_with_the_same_value_restores_foreign_key_index_if_only_foreign_key_columns_are_nullable()
    {
        var modelBuilder = CreateOptionalModelBuilder();
        var indexBuilder = modelBuilder.Entity<OptionalInvoice>().HasIndex(i => new { i.PeriodId, i.RequiredId });
        ((IConventionIndex)indexBuilder.Metadata).Builder.HasFilter("x");

        Assert.Equal(["PeriodId,RequiredId WHERE x"], DescribeOptionalIndexes(modelBuilder));

        indexBuilder.HasFilter("x");

        Assert.Equal(["PeriodId", "PeriodId,RequiredId WHERE x"], DescribeOptionalIndexes(modelBuilder, finalize: true));
    }

    [Fact]
    public void Filtered_unique_index_does_not_cover_foreign_key_index_when_uniqueness_changes()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId }).HasFilter("x").IsUnique();

        Assert.Equal(["PeriodId", "PeriodId,CompanyId WHERE x"], DescribeIndexes(modelBuilder));
    }

    [Fact]
    public void Foreign_key_index_covered_only_by_filtered_index_is_kept_when_foreign_key_becomes_non_unique()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Invoice>().HasOne(i => i.Period).WithOne().HasForeignKey<Invoice>(i => i.PeriodId);
        modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId }).HasFilter("x");

        modelBuilder.Entity<Invoice>().HasOne(i => i.Period).WithMany().HasForeignKey(i => i.PeriodId);

        Assert.Equal(["PeriodId", "PeriodId,CompanyId WHERE x"], DescribeIndexes(modelBuilder));

        var entityType = modelBuilder.Model.FindEntityType(typeof(Invoice))!;
        Assert.False(entityType.FindIndex(entityType.FindProperty(nameof(Invoice.PeriodId))!)!.IsUnique);
    }

    [Fact]
    public void Removing_filtered_index_does_not_duplicate_foreign_key_index()
    {
        var modelBuilder = CreateModelBuilder();
        var index = modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId }).HasFilter("x").Metadata;

        modelBuilder.Entity<Invoice>().Metadata.RemoveIndex(index);

        Assert.Equal(["PeriodId"], DescribeIndexes(modelBuilder));
    }

    [Fact]
    public void Removing_unfiltered_index_restores_foreign_key_index()
    {
        var modelBuilder = CreateModelBuilder();
        var index = modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId }).Metadata;

        modelBuilder.Entity<Invoice>().Metadata.RemoveIndex(index);

        Assert.Equal(["PeriodId"], DescribeIndexes(modelBuilder));
    }

    [Fact]
    public void Filtered_index_on_base_type_does_not_cover_foreign_key_index_on_derived_type()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<BaseInvoice>();
        modelBuilder.Entity<DerivedInvoice>().HasOne(i => i.Period).WithMany().HasForeignKey(i => i.PeriodId);
        var indexBuilder = modelBuilder.Entity<BaseInvoice>().HasIndex(i => new { i.PeriodId, i.CompanyId });

        Assert.Empty(DerivedIndexes(modelBuilder));

        indexBuilder.HasFilter("x");

        Assert.Equal(["PeriodId"], DerivedIndexes(modelBuilder));

        indexBuilder.HasFilter(null);

        Assert.Empty(DerivedIndexes(modelBuilder));
    }

    [Fact]
    public void Foreign_key_index_is_kept_when_base_type_with_covering_filtered_index_is_set()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<BaseInvoice>().HasIndex(i => new { i.PeriodId, i.CompanyId }).HasFilter("x");
        modelBuilder.Entity<DerivedInvoice>().HasBaseType((Type?)null);
        modelBuilder.Entity<DerivedInvoice>().HasOne(i => i.Period).WithMany().HasForeignKey(i => i.PeriodId);

        Assert.Equal(["PeriodId"], DerivedIndexes(modelBuilder));

        modelBuilder.Entity<DerivedInvoice>().HasBaseType<BaseInvoice>();

        Assert.Equal(["PeriodId"], DerivedIndexes(modelBuilder));
    }

    [Fact]
    public void Redundant_index_is_not_logged_for_filtered_covering_index()
    {
        var logger = new TestLogger<DbLoggerCategory.Model, TestLoggingDefinitions> { EnabledFor = LogLevel.Debug };
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder(modelLogger: logger);
        modelBuilder.Entity<Invoice>().HasOne(i => i.Period).WithMany().HasForeignKey(i => i.PeriodId);
        modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId }).HasFilter("x");

        modelBuilder.FinalizeModel();

        Assert.Null(logger.LoggedAt);
    }

    [Fact]
    public void Redundant_index_is_logged_for_unfiltered_covering_index()
    {
        var logger = new TestLogger<DbLoggerCategory.Model, TestLoggingDefinitions> { EnabledFor = LogLevel.Debug };
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder(modelLogger: logger);
        modelBuilder.Entity<Invoice>().HasOne(i => i.Period).WithMany().HasForeignKey(i => i.PeriodId);
        modelBuilder.Entity<Invoice>().HasIndex(i => new { i.PeriodId, i.CompanyId });

        modelBuilder.FinalizeModel();

        Assert.Equal(CoreEventId.RedundantIndexRemoved, logger.LoggedEvent);
    }

    private static ModelBuilder CreateModelBuilder()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Invoice>().HasOne(i => i.Period).WithMany().HasForeignKey(i => i.PeriodId);
        return modelBuilder;
    }

    private static string[] DescribeIndexes(ModelBuilder modelBuilder, bool finalize = false)
    {
        IReadOnlyModel model = finalize ? modelBuilder.FinalizeModel() : modelBuilder.Model;

        return model.FindEntityType(typeof(Invoice))!.GetIndexes().Select(Describe).Order(StringComparer.Ordinal).ToArray();
    }

    private static ModelBuilder CreateOptionalModelBuilder()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<OptionalInvoice>().HasOne(i => i.Period).WithMany().HasForeignKey(i => i.PeriodId);
        return modelBuilder;
    }

    private static string[] DescribeOptionalIndexes(ModelBuilder modelBuilder, bool finalize = false)
    {
        IReadOnlyModel model = finalize ? modelBuilder.FinalizeModel() : modelBuilder.Model;

        return model.FindEntityType(typeof(OptionalInvoice))!.GetIndexes().Select(Describe).Order(StringComparer.Ordinal).ToArray();
    }

    private static string[] DerivedIndexes(ModelBuilder modelBuilder)
        => modelBuilder.Model.FindEntityType(typeof(DerivedInvoice))!.GetDeclaredIndexes().Select(Describe)
            .Order(StringComparer.Ordinal).ToArray();

    private static string Describe(IReadOnlyIndex index)
        => string.Join(",", index.Properties.Select(p => p.Name)) + (index.GetFilter() is { } filter ? " WHERE " + filter : "");

    private class Period
    {
        public int Id { get; set; }
    }

    private class Invoice
    {
        public int Id { get; set; }
        public int PeriodId { get; set; }
        public int CompanyId { get; set; }
        public Period? Period { get; set; }
    }

    private class OptionalInvoice
    {
        public int Id { get; set; }
        public int? PeriodId { get; set; }
        public int RequiredId { get; set; }
        public int? OptionalId { get; set; }
        public Period? Period { get; set; }
    }

    private class BaseInvoice
    {
        public int Id { get; set; }
        public int PeriodId { get; set; }
        public int CompanyId { get; set; }
    }

    private class DerivedInvoice : BaseInvoice
    {
        public Period? Period { get; set; }
    }
}
