// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class F1CosmosFixture<TRowVersion> : F1FixtureBase<TRowVersion>
{
    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    public override TestHelpers TestHelpers
        => CosmosTestHelpers.Instance;

    protected override async Task<bool> ShouldSeedAsync(F1Context context)
    {
        try
        {
            await base.ShouldSeedAsync(context);
        }
        catch
        {
            // Recreating the containers without using CosmosClient causes cached metadata in CosmosClient to be out of sync
            // and causes the first query to fail. This is a workaround for that.
        }

        return await base.ShouldSeedAsync(context);
    }

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined));

    protected override void BuildModelExternal(ModelBuilder modelBuilder)
    {
        base.BuildModelExternal(modelBuilder);

        modelBuilder.HasDiscriminatorInJsonIds();

        modelBuilder.Entity<Engine>(
            b =>
            {
                b.Property(e => e.EngineSupplierId).IsConcurrencyToken(false);
                b.Property(e => e.Name).IsConcurrencyToken(false);
                b.OwnsOne(
                    e => e.StorageLocation, lb =>
                    {
                        lb.Property(l => l.Latitude).IsConcurrencyToken(false);
                        lb.Property(l => l.Longitude).IsConcurrencyToken(false);
                    });
            });

        modelBuilder.Entity<Chassis>().Property<string>("Version").IsETagConcurrency();
        modelBuilder.Entity<Driver>().Property<string>("Version").IsETagConcurrency();
        modelBuilder.Entity<Team>().Property<string>("Version").IsETagConcurrency();

        modelBuilder.Entity<Sponsor>(
            eb =>
            {
                eb.Property<string>("Version").IsETagConcurrency();
                eb.Property<int?>(Sponsor.ClientTokenPropertyName).IsConcurrencyToken(false);
            });

        modelBuilder.Entity<TitleSponsor>()
            .Ignore(s => s.Details)
            .OwnsOne(
                s => s.Details, eb =>
                {
                    eb.Property<string>("Version").IsETagConcurrency();
                    eb.Property<int?>(Sponsor.ClientTokenPropertyName).IsConcurrencyToken(false);
                });
    }
}
