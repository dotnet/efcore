// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.StoredProcedureUpdateModel;

namespace Microsoft.EntityFrameworkCore.Update;

#nullable enable

public abstract class StoredProcedureUpdateFixtureBase : SharedStoreFixtureBase<StoredProcedureUpdateContext>
{
    protected override string StoreName
        => "StoredProcedureUpdateTest";

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.SharedTypeEntity<Entity>(nameof(StoredProcedureUpdateContext.WithOutputParameter))
            .InsertUsingStoredProcedure(
                nameof(StoredProcedureUpdateContext.WithOutputParameter) + "_Insert",
                spb => spb
                    .HasParameter(w => w.Name)
                    .HasParameter(w => w.Id, pb => pb.IsOutput()))
            .UpdateUsingStoredProcedure(
                nameof(StoredProcedureUpdateContext.WithOutputParameter) + "_Update",
                spb => spb
                    .HasOriginalValueParameter(w => w.Id)
                    .HasParameter(w => w.Name))
            .DeleteUsingStoredProcedure(
                nameof(StoredProcedureUpdateContext.WithOutputParameter) + "_Delete",
                spb => spb.HasOriginalValueParameter(w => w.Id));

        modelBuilder.SharedTypeEntity<Entity>(nameof(StoredProcedureUpdateContext.WithResultColumn))
            .InsertUsingStoredProcedure(
                nameof(StoredProcedureUpdateContext.WithResultColumn) + "_Insert", spb => spb
                    .HasParameter(w => w.Name)
                    .HasResultColumn(w => w.Id));

        modelBuilder.SharedTypeEntity<EntityWithAdditionalProperty>(
            nameof(StoredProcedureUpdateContext.WithTwoResultColumns),
            b =>
            {
                b.Property(w => w.AdditionalProperty).HasComputedColumnSql("8");

                b.InsertUsingStoredProcedure(
                    "WithTwoResultColumns_Insert", spb => spb
                        .HasParameter(w => w.Name)
                        .HasResultColumn(w => w.AdditionalProperty)
                        .HasResultColumn(w => w.Id));
            });

        modelBuilder.SharedTypeEntity<EntityWithAdditionalProperty>(
            nameof(StoredProcedureUpdateContext.WithOutputParameterAndResultColumn),
            b =>
            {
                b.Property(w => w.AdditionalProperty).HasComputedColumnSql("8");

                b.InsertUsingStoredProcedure(
                    "WithOutputParameterAndResultColumn_Insert", spb => spb
                        .HasParameter(w => w.Id, pb => pb.IsOutput())
                        .HasParameter(w => w.Name)
                        .HasResultColumn(w => w.AdditionalProperty));
            });

        modelBuilder.SharedTypeEntity<EntityWithAdditionalProperty>(
            nameof(StoredProcedureUpdateContext.WithOutputParameterAndRowsAffectedResultColumn),
            b =>
            {
                b.Property(w => w.AdditionalProperty).HasComputedColumnSql("8");

                b.UpdateUsingStoredProcedure(
                    nameof(StoredProcedureUpdateContext.WithOutputParameterAndRowsAffectedResultColumn) + "_Update",
                    spb => spb
                        .HasOriginalValueParameter(w => w.Id)
                        .HasParameter(w => w.Name)
                        .HasParameter(w => w.AdditionalProperty, pb => pb.IsOutput())
                        .HasRowsAffectedResultColumn());
            });

        modelBuilder.SharedTypeEntity<EntityWithAdditionalProperty>(nameof(StoredProcedureUpdateContext.WithTwoInputParameters))
            .UpdateUsingStoredProcedure(
                nameof(StoredProcedureUpdateContext.WithTwoInputParameters) + "_Update", spb => spb
                    .HasOriginalValueParameter(w => w.Id)
                    .HasParameter(w => w.Name)
                    .HasParameter(w => w.AdditionalProperty));

        modelBuilder.SharedTypeEntity<Entity>(nameof(StoredProcedureUpdateContext.WithRowsAffectedParameter))
            .UpdateUsingStoredProcedure(
                nameof(StoredProcedureUpdateContext.WithRowsAffectedParameter) + "_Update",
                spb => spb
                    .HasOriginalValueParameter(w => w.Id)
                    .HasParameter(w => w.Name)
                    .HasRowsAffectedParameter());

        modelBuilder.SharedTypeEntity<Entity>(nameof(StoredProcedureUpdateContext.WithRowsAffectedResultColumn))
            .UpdateUsingStoredProcedure(
                nameof(StoredProcedureUpdateContext.WithRowsAffectedResultColumn) + "_Update",
                spb => spb
                    .HasOriginalValueParameter(w => w.Id)
                    .HasParameter(w => w.Name)
                    .HasRowsAffectedResultColumn());

        modelBuilder.SharedTypeEntity<Entity>(nameof(StoredProcedureUpdateContext.WithRowsAffectedReturnValue))
            .UpdateUsingStoredProcedure(
                nameof(StoredProcedureUpdateContext.WithRowsAffectedReturnValue) + "_Update",
                spb => spb
                    .HasOriginalValueParameter(w => w.Id)
                    .HasParameter(w => w.Name)
                    .HasRowsAffectedReturnValue());

        modelBuilder.SharedTypeEntity<Entity>(
            nameof(StoredProcedureUpdateContext.WithStoreGeneratedConcurrencyTokenAsInOutParameter),
            b =>
            {
                ConfigureStoreGeneratedConcurrencyToken(b, "ConcurrencyToken");

                b.UpdateUsingStoredProcedure(
                    nameof(StoredProcedureUpdateContext.WithStoreGeneratedConcurrencyTokenAsInOutParameter) + "_Update",
                    spb => spb
                        .HasOriginalValueParameter(w => w.Id)
                        .HasOriginalValueParameter("ConcurrencyToken", pb => pb.IsInputOutput())
                        .HasParameter(w => w.Name)
                        .HasRowsAffectedParameter());
            });

        modelBuilder.SharedTypeEntity<Entity>(
            nameof(StoredProcedureUpdateContext.WithStoreGeneratedConcurrencyTokenAsTwoParameters),
            b =>
            {
                ConfigureStoreGeneratedConcurrencyToken(b, "ConcurrencyToken");

                b.UpdateUsingStoredProcedure(
                    nameof(StoredProcedureUpdateContext.WithStoreGeneratedConcurrencyTokenAsTwoParameters) + "_Update",
                    spb => spb
                        .HasOriginalValueParameter(w => w.Id)
                        .HasOriginalValueParameter("ConcurrencyToken", pb => pb.HasName("ConcurrencyTokenIn"))
                        .HasParameter(w => w.Name)
                        .HasParameter(
                            "ConcurrencyToken", pb => pb
                                .HasName("ConcurrencyTokenOut")
                                .IsOutput())
                        .HasRowsAffectedParameter());
            });

        modelBuilder.SharedTypeEntity<EntityWithAdditionalProperty>(
            nameof(StoredProcedureUpdateContext.WithUserManagedConcurrencyToken),
            b =>
            {
                b.Property(e => e.AdditionalProperty).IsConcurrencyToken();

                b.UpdateUsingStoredProcedure(
                    nameof(StoredProcedureUpdateContext.WithUserManagedConcurrencyToken) + "_Update",
                    spb => spb
                        .HasOriginalValueParameter(w => w.Id)
                        .HasOriginalValueParameter(w => w.AdditionalProperty, pb => pb.HasName("ConcurrencyTokenOriginal"))
                        .HasParameter(w => w.Name)
                        .HasParameter(w => w.AdditionalProperty, pb => pb.HasName("ConcurrencyTokenCurrent"))
                        .HasRowsAffectedParameter());
            });

        modelBuilder.SharedTypeEntity<Entity>(nameof(StoredProcedureUpdateContext.WithOriginalAndCurrentValueOnNonConcurrencyToken))
            .UpdateUsingStoredProcedure(
                nameof(StoredProcedureUpdateContext.WithOriginalAndCurrentValueOnNonConcurrencyToken) + "_Update",
                spb => spb
                    .HasOriginalValueParameter(w => w.Id)
                    .HasParameter(w => w.Name, pb => pb.HasName("NameCurrent"))
                    .HasOriginalValueParameter(w => w.Name, pb => pb.HasName("NameOriginal")));

        modelBuilder.SharedTypeEntity<Entity>(
            nameof(StoredProcedureUpdateContext.WithInputOrOutputParameter),
            b =>
            {
                b.Property(w => w.Name).IsRequired().ValueGeneratedOnAdd();

                b.InsertUsingStoredProcedure(
                    nameof(StoredProcedureUpdateContext.WithInputOrOutputParameter) + "_Insert",
                    spb => spb
                        .HasParameter(w => w.Id, pb => pb.IsOutput())
                        .HasParameter(w => w.Name, pb => pb.IsInputOutput()));
            });

        modelBuilder.Entity<TphChild1>();

        modelBuilder.Entity<TphChild2>(
            b =>
            {
                b.Property(w => w.Child2OutputParameterProperty).HasDefaultValue(8);
                b.Property(w => w.Child2ResultColumnProperty).HasDefaultValue(9);
            });

        modelBuilder.Entity<TphParent>(
            b =>
            {
                b.ToTable("Tph");

                b.InsertUsingStoredProcedure(
                    "Tph_Insert",
                    spb => spb
                        .HasParameter(w => w.Id, pb => pb.IsOutput())
                        .HasParameter("Discriminator")
                        .HasParameter(w => w.Name)
                        .HasParameter(nameof(TphChild1.Child1Property))
                        .HasParameter(nameof(TphChild2.Child2InputProperty))
                        .HasParameter(nameof(TphChild2.Child2OutputParameterProperty), o => o.IsOutput())
                        .HasResultColumn(nameof(TphChild2.Child2ResultColumnProperty)));
            });

        modelBuilder.Entity<TptParent>(
            b =>
            {
                b.UseTptMappingStrategy();

                b.InsertUsingStoredProcedure(
                    "TptParent_Insert",
                    spb => spb
                        .HasParameter(w => w.Id, pb => pb.IsOutput())
                        .HasParameter(w => w.Name));
            });

        // TODO: The following fails validation:
        // The entity type 'TptChild' is mapped to the stored procedure 'TptChild_Insert', however the store-generated properties {'Id'} are not mapped to any output parameter or result column.
        modelBuilder.Entity<TptChild>()
            .InsertUsingStoredProcedure(
                "TptChild_Insert",
                spb => spb
                    .HasParameter(w => w.Id)
                    .HasParameter(w => w.ChildProperty));

        modelBuilder.Entity<TptMixedParent>(
            b =>
            {
                b.UseTptMappingStrategy();

                b.InsertUsingStoredProcedure(
                    "TptMixedParent_Insert",
                    spb => spb
                        .HasParameter(w => w.Id, pb => pb.IsOutput())
                        .HasParameter(w => w.Name));
            });

        // No sproc mapping for TptMixedChild, use regular SQL

        modelBuilder.Entity<TpcParent>().UseTpcMappingStrategy();

        modelBuilder.Entity<TpcChild>()
            .UseTpcMappingStrategy()
            .InsertUsingStoredProcedure(
                "TpcChild_Insert",
                spb => spb
                    .HasParameter(w => w.Id, pb => pb.IsOutput())
                    .HasParameter(w => w.Name)
                    .HasParameter(w => w.ChildProperty));
    }

    /// <summary>
    ///     A method to be implement by the provider, to set up a store-generated concurrency token shadow property with the given name.
    /// </summary>
    protected abstract void ConfigureStoreGeneratedConcurrencyToken(EntityTypeBuilder entityTypeBuilder, string propertyName);

    public virtual void CleanData()
    {
        using var context = CreateContext();

        context.WithOutputParameter.RemoveRange(context.WithOutputParameter);
        context.WithResultColumn.RemoveRange(context.WithResultColumn);
        context.WithTwoResultColumns.RemoveRange(context.WithTwoResultColumns);
        context.WithOutputParameterAndResultColumn.RemoveRange(context.WithOutputParameterAndResultColumn);
        context.WithTwoInputParameters.RemoveRange(context.WithTwoInputParameters);
        context.WithRowsAffectedParameter.RemoveRange(context.WithRowsAffectedParameter);
        context.WithRowsAffectedResultColumn.RemoveRange(context.WithRowsAffectedResultColumn);
        context.WithRowsAffectedReturnValue.RemoveRange(context.WithRowsAffectedReturnValue);
        context.WithStoreGeneratedConcurrencyTokenAsInOutParameter.RemoveRange(context.WithStoreGeneratedConcurrencyTokenAsInOutParameter);
        context.WithStoreGeneratedConcurrencyTokenAsTwoParameters.RemoveRange(context.WithStoreGeneratedConcurrencyTokenAsTwoParameters);
        context.WithUserManagedConcurrencyToken.RemoveRange(context.WithUserManagedConcurrencyToken);
        context.WithOriginalAndCurrentValueOnNonConcurrencyToken.RemoveRange(context.WithOriginalAndCurrentValueOnNonConcurrencyToken);
        context.WithInputOrOutputParameter.RemoveRange(context.WithInputOrOutputParameter);
        context.TphParent.RemoveRange(context.TphParent);
        context.TphChild.RemoveRange(context.TphChild);
        context.TptParent.RemoveRange(context.TptParent);
        context.TptChild.RemoveRange(context.TptChild);
        context.TptMixedParent.RemoveRange(context.TptMixedParent);
        context.TptMixedChild.RemoveRange(context.TptMixedChild);
        context.TpcParent.RemoveRange(context.TpcParent);
        context.TpcChild.RemoveRange(context.TpcChild);

        context.SaveChanges();
    }

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;
}
