// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class OptimisticConcurrencyULongSqlServerTest(F1ULongSqlServerFixture fixture) : OptimisticConcurrencySqlServerTestBase<F1ULongSqlServerFixture, ulong>(fixture)
{
    [ConditionalFact]
    public async Task ULong_row_version_can_handle_empty_array_from_the_database()
    {
        await using var context = CreateF1Context();

        await context
            .Set<F1ULongSqlServerFixture.OptimisticParent>()
            .Select(
                x => new
                {
                    x.Id,
                    Child = new
                    {
                        Id = x.OptionalChild == null ? Guid.Empty : x.OptionalChild.Id,
                        Version = x.OptionalChild == null ? 0 : x.OptionalChild.Version
                    }
                }
            ).ToArrayAsync();
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPH_and_owned_types(bool updateOwnedFirst)
        => Row_version_with_owned_types<SuperFan, ulong>(updateOwnedFirst, Mapping.Tph, "ULongVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPT_and_owned_types(bool updateOwnedFirst)
        => Row_version_with_owned_types<SuperFanTpt, ulong>(updateOwnedFirst, Mapping.Tpt, "ULongVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPC_and_owned_types(bool updateOwnedFirst)
        => Row_version_with_owned_types<SuperFanTpc, ulong>(updateOwnedFirst, Mapping.Tpc, "ULongVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPH_and_table_splitting(bool updateDependentFirst)
        => Row_version_with_table_splitting<StreetCircuit, City, ulong>(updateDependentFirst, Mapping.Tph, "ULongVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPT_and_table_splitting(bool updateDependentFirst)
        => Row_version_with_table_splitting<StreetCircuitTpt, CityTpt, ulong>(updateDependentFirst, Mapping.Tpt, "ULongVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPC_and_table_splitting(bool updateDependentFirst)
        => Row_version_with_table_splitting<StreetCircuitTpc, CityTpc, ulong>(updateDependentFirst, Mapping.Tpc, "ULongVersion");
}

public class OptimisticConcurrencySqlServerTest(F1SqlServerFixture fixture) : OptimisticConcurrencySqlServerTestBase<F1SqlServerFixture, byte[]>(fixture)
{
    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Row_version_with_TPH_and_owned_types(bool updateOwnedFirst)
        => Row_version_with_owned_types<SuperFan, List<byte>>(updateOwnedFirst, Mapping.Tph, "BinaryVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Row_version_with_TPT_and_owned_types(bool updateOwnedFirst)
        => Row_version_with_owned_types<SuperFanTpt, List<byte>>(updateOwnedFirst, Mapping.Tpt, "BinaryVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Row_version_with_TPC_and_owned_types(bool updateOwnedFirst)
        => Row_version_with_owned_types<SuperFanTpc, List<byte>>(updateOwnedFirst, Mapping.Tpc, "BinaryVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPH_and_table_splitting(bool updateDependentFirst)
        => Row_version_with_table_splitting<StreetCircuit, City, List<byte>>(updateDependentFirst, Mapping.Tph, "BinaryVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPT_and_table_splitting(bool updateDependentFirst)
        => Row_version_with_table_splitting<StreetCircuitTpt, CityTpt, List<byte>>(updateDependentFirst, Mapping.Tpt, "BinaryVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPC_and_table_splitting(bool updateDependentFirst)
        => Row_version_with_table_splitting<StreetCircuitTpc, CityTpc, List<byte>>(updateDependentFirst, Mapping.Tpc, "BinaryVersion");
}

public abstract class OptimisticConcurrencySqlServerTestBase<TFixture, TRowVersion>
    : OptimisticConcurrencyRelationalTestBase<TFixture, TRowVersion>
    where TFixture : F1RelationalFixture<TRowVersion>, new()
{
    protected OptimisticConcurrencySqlServerTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    protected enum Mapping
    {
        Tph,
        Tpt,
        Tpc
    }

    protected async Task Row_version_with_owned_types<TEntity, TVersion>(bool updateOwnedFirst, Mapping mapping, string propertyName)
        where TEntity : class, ISuperFan
    {
        await using var c = CreateF1Context();

        await c.Database.CreateExecutionStrategy().ExecuteAsync(
            c, async context =>
            {
                var synthesizedPropertyName = $"_TableSharingConcurrencyTokenConvention_{propertyName}";

                await using var transaction = BeginTransaction(context.Database);

                var fan = context.Set<TEntity>().Single(e => e.Name == "Alice");

                var fanEntry = c.Entry(fan);
                var swagEntry = fanEntry.Reference(s => s.Swag).TargetEntry!;
                var fanVersion1 = fanEntry.Property<TVersion>(propertyName).CurrentValue;
                var swagVersion1 = default(TVersion);

                if (mapping != Mapping.Tpt) // Issue #22060
                {
                    swagVersion1 = swagEntry.Property<TVersion>(synthesizedPropertyName).CurrentValue;

                    Assert.Equal(fanVersion1, swagVersion1);
                }

                await using var innerContext = CreateF1Context();
                UseTransaction(innerContext.Database, transaction);
                var fanInner = innerContext.Set<TEntity>().Single(e => e.Name == "Alice");

                if (updateOwnedFirst)
                {
                    fan.Swag.Stuff += "+";
                    fanInner.Swag.Stuff += "-";
                }
                else
                {
                    fanInner.Name += "-";
                    fan.Name += "+";
                }

                await innerContext.SaveChangesAsync();

                if (updateOwnedFirst && mapping == Mapping.Tpt) // Issue #22060
                {
                    await context.SaveChangesAsync();
                    return;
                }

                await Assert.ThrowsAnyAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());

                await fanEntry.ReloadAsync();
                await swagEntry.ReloadAsync();

                await context.SaveChangesAsync();

                var fanVersion2 = fanEntry.Property<TVersion>(propertyName).CurrentValue;
                Assert.NotEqual(fanVersion1, fanVersion2);

                var swagVersion2 = default(TVersion);
                if (mapping != Mapping.Tpt) // Issue #22060
                {
                    swagVersion2 = swagEntry.Property<TVersion>(synthesizedPropertyName).CurrentValue;
                    Assert.Equal(fanVersion2, swagVersion2);
                    Assert.NotEqual(swagVersion1, swagVersion2);
                }

                await innerContext.Entry(fanInner).ReloadAsync();
                await innerContext.Entry(fanInner.Swag).ReloadAsync();

                if (updateOwnedFirst)
                {
                    fanInner.Name += "-";
                    fan.Name += "+";
                }
                else
                {
                    fan.Swag.Stuff += "+";
                    fanInner.Swag.Stuff += "-";
                }

                await innerContext.SaveChangesAsync();

                if (!updateOwnedFirst && mapping == Mapping.Tpt) // Issue #22060
                {
                    await context.SaveChangesAsync();
                    return;
                }

                await Assert.ThrowsAnyAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());

                await fanEntry.ReloadAsync();
                await swagEntry.ReloadAsync();

                await context.SaveChangesAsync();

                var fanVersion3 = fanEntry.Property<TVersion>(propertyName).CurrentValue;
                Assert.NotEqual(fanVersion2, fanVersion3);

                if (mapping != Mapping.Tpt) // Issue #22060
                {
                    var swagVersion3 = swagEntry.Property<TVersion>(synthesizedPropertyName).CurrentValue;
                    Assert.Equal(fanVersion3, swagVersion3);
                    Assert.NotEqual(swagVersion2, swagVersion3);
                }
            });
    }

    protected async Task Row_version_with_table_splitting<TEntity, TCity, TVersion>(
        bool updateDependentFirst,
        Mapping mapping,
        string propertyName)
        where TEntity : class, IStreetCircuit<TCity>
        where TCity : class, ICity
    {
        await using var c = CreateF1Context();

        await c.Database.CreateExecutionStrategy().ExecuteAsync(
            c, async context =>
            {
                var synthesizedPropertyName = $"_TableSharingConcurrencyTokenConvention_{propertyName}";

                await using var transaction = BeginTransaction(context.Database);

                var circuit = context.Set<TEntity>().Include(e => e.City).Single(e => e.Name == "Monaco");

                var circuitEntry = c.Entry(circuit);
                var cityEntry = circuitEntry.Reference(s => s.City).TargetEntry!;
                var circuitVersion1 = circuitEntry.Property<TVersion>(propertyName).CurrentValue;
                var cityVersion1 = default(TVersion);

                if (mapping != Mapping.Tpt) // Issue #22060
                {
                    cityVersion1 = cityEntry.Property<TVersion>(synthesizedPropertyName).CurrentValue;

                    Assert.Equal(circuitVersion1, cityVersion1);
                }

                await using var innerContext = CreateF1Context();
                UseTransaction(innerContext.Database, transaction);
                var circuitInner = innerContext.Set<TEntity>().Include(e => e.City).Single(e => e.Name == "Monaco");

                if (updateDependentFirst)
                {
                    circuit.City.Name += "+";
                    circuitInner.City.Name += "-";
                }
                else
                {
                    circuit.Name += "+";
                    circuitInner.Name += "-";
                }

                await innerContext.SaveChangesAsync();

                if (updateDependentFirst && mapping == Mapping.Tpt) // Issue #22060
                {
                    await context.SaveChangesAsync();
                    return;
                }

                await Assert.ThrowsAnyAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());

                await circuitEntry.ReloadAsync();
                await cityEntry.ReloadAsync();

                await context.SaveChangesAsync();

                var circuitVersion2 = circuitEntry.Property<TVersion>(propertyName).CurrentValue;
                Assert.NotEqual(circuitVersion1, circuitVersion2);

                var cityVersion2 = default(TVersion);
                if (mapping != Mapping.Tpt) // Issue #22060
                {
                    cityVersion2 = cityEntry.Property<TVersion>(synthesizedPropertyName).CurrentValue;
                    Assert.Equal(circuitVersion2, cityVersion2);
                    Assert.NotEqual(cityVersion1, cityVersion2);
                }

                await innerContext.Entry(circuitInner).ReloadAsync();
                await innerContext.Entry(circuitInner.City).ReloadAsync();

                if (updateDependentFirst)
                {
                    circuit.Name += "+";
                    circuitInner.Name += "-";
                }
                else
                {
                    circuit.City.Name += "+";
                    circuitInner.City.Name += "-";
                }

                await innerContext.SaveChangesAsync();

                if (!updateDependentFirst && mapping == Mapping.Tpt) // Issue #22060
                {
                    await context.SaveChangesAsync();
                    return;
                }

                await Assert.ThrowsAnyAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());

                await circuitEntry.ReloadAsync();
                await cityEntry.ReloadAsync();

                await context.SaveChangesAsync();

                var circuitVersion3 = circuitEntry.Property<TVersion>(propertyName).CurrentValue;
                Assert.NotEqual(circuitVersion2, circuitVersion3);

                if (mapping != Mapping.Tpt) // Issue #22060
                {
                    var cityVersion3 = cityEntry.Property<TVersion>(synthesizedPropertyName).CurrentValue;
                    Assert.Equal(circuitVersion3, cityVersion3);
                    Assert.NotEqual(cityVersion2, cityVersion3);
                }
            });
    }

    [ConditionalFact]
    public async Task Modifying_concurrency_token_only_is_noop()
    {
        using var c = CreateF1Context();
        await c.Database.CreateExecutionStrategy().ExecuteAsync(
            c, async context =>
            {
                using var transaction = context.Database.BeginTransaction();
                var driver = context.Drivers.Single(d => d.CarNumber == 1);
                driver.Podiums = StorePodiums;
                var firstVersion = context.Entry(driver).Property<TRowVersion>("Version").CurrentValue;
                await context.SaveChangesAsync();

                using var innerContext = CreateF1Context();
                innerContext.Database.UseTransaction(transaction.GetDbTransaction());
                driver = innerContext.Drivers.Single(d => d.CarNumber == 1);
                Assert.NotEqual(firstVersion, innerContext.Entry(driver).Property<TRowVersion>("Version").CurrentValue);
                Assert.Equal(StorePodiums, driver.Podiums);

                var secondVersion = innerContext.Entry(driver).Property<TRowVersion>("Version").CurrentValue;
                innerContext.Entry(driver).Property<TRowVersion>("Version").CurrentValue = firstVersion;
                await innerContext.SaveChangesAsync();
                using var validationContext = CreateF1Context();
                validationContext.Database.UseTransaction(transaction.GetDbTransaction());
                driver = validationContext.Drivers.Single(d => d.CarNumber == 1);
                Assert.Equal(secondVersion, validationContext.Entry(driver).Property<TRowVersion>("Version").CurrentValue);
                Assert.Equal(StorePodiums, driver.Podiums);
            });
    }

    [ConditionalFact]
    public async Task Database_concurrency_token_value_is_updated_for_all_sharing_entities()
    {
        using var c = CreateF1Context();
        await c.Database.CreateExecutionStrategy().ExecuteAsync(
            c, async context =>
            {
                using var transaction = context.Database.BeginTransaction();
                var sponsor = context.Set<TitleSponsor>().Single();
                var sponsorEntry = c.Entry(sponsor);
                var detailsEntry = sponsorEntry.Reference(s => s.Details).TargetEntry;
                var sponsorVersion = sponsorEntry.Property<TRowVersion>("Version").CurrentValue;
                var detailsVersion = detailsEntry.Property<TRowVersion>("Version").CurrentValue;

                Assert.Null(sponsorEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue);
                sponsorEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue = 1;

                sponsor.Name = "Telecom";

                Assert.Equal(sponsorVersion, detailsVersion);

                await context.SaveChangesAsync();

                var newSponsorVersion = sponsorEntry.Property<TRowVersion>("Version").CurrentValue;
                var newDetailsVersion = detailsEntry.Property<TRowVersion>("Version").CurrentValue;

                Assert.Equal(newSponsorVersion, newDetailsVersion);
                Assert.NotEqual(sponsorVersion, newSponsorVersion);

                Assert.Equal(1, sponsorEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue);
                Assert.Equal(1, detailsEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue);
            });
    }

    [ConditionalFact]
    public async Task Original_concurrency_token_value_is_used_when_replacing_owned_instance()
    {
        using var c = CreateF1Context();
        await c.Database.CreateExecutionStrategy().ExecuteAsync(
            c, async context =>
            {
                using var transaction = context.Database.BeginTransaction();
                var sponsor = context.Set<TitleSponsor>().Single();
                var sponsorEntry = c.Entry(sponsor);
                var sponsorVersion = sponsorEntry.Property<TRowVersion>("Version").CurrentValue;

                Assert.Null(sponsorEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue);
                sponsorEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue = 1;

                sponsor.Details = new SponsorDetails { Days = 11, Space = 51m };

                context.ChangeTracker.DetectChanges();

                var detailsEntry = sponsorEntry.Reference(s => s.Details).TargetEntry;
                detailsEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue = 1;

                await context.SaveChangesAsync();

                var newSponsorVersion = sponsorEntry.Property<TRowVersion>("Version").CurrentValue;
                var newDetailsVersion = detailsEntry.Property<TRowVersion>("Version").CurrentValue;

                Assert.Equal(newSponsorVersion, newDetailsVersion);
                Assert.NotEqual(sponsorVersion, newSponsorVersion);

                Assert.Equal(1, sponsorEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue);
                Assert.Equal(1, detailsEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue);
            });
    }

    public override void Property_entry_original_value_is_set()
    {
        base.Property_entry_original_value_is_set();

        AssertSql(
            """
SELECT TOP(1) [e].[Id], [e].[EngineSupplierId], [e].[Name], [e].[StorageLocation_Latitude], [e].[StorageLocation_Longitude]
FROM [Engines] AS [e]
ORDER BY [e].[Id]
""",
            //
            """
@p1='1'
@p2='Mercedes' (Size = 450)
@p0='FO 108X' (Size = 4000)
@p3='ChangedEngine' (Size = 4000)
@p4='47.64491' (Nullable = true)
@p5='-122.128101' (Nullable = true)

SET IMPLICIT_TRANSACTIONS OFF;
SET NOCOUNT ON;
UPDATE [Engines] SET [Name] = @p0
OUTPUT 1
WHERE [Id] = @p1 AND [EngineSupplierId] = @p2 AND [Name] = @p3 AND [StorageLocation_Latitude] = @p4 AND [StorageLocation_Longitude] = @p5;
""");
    }

    private void AssertSql(params string[] expected)
        => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());
}
