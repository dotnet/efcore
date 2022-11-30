// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public class OptimisticConcurrencyULongSqlServerTest : OptimisticConcurrencySqlServerTestBase<F1ULongSqlServerFixture, ulong>
{
    public OptimisticConcurrencyULongSqlServerTest(F1ULongSqlServerFixture fixture)
        : base(fixture)
    {
    }

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
    public Task Ulong_row_version_with_TPH_and_owned_types(bool updateOwned)
        => Row_version_with_owned_types<SuperFan, ulong>(updateOwned, Mapping.Tph, "ULongVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPT_and_owned_types(bool updateOwned)
        => Row_version_with_owned_types<SuperFanTpt, ulong>(updateOwned, Mapping.Tpt, "ULongVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPC_and_owned_types(bool updateOwned)
        => Row_version_with_owned_types<SuperFanTpc, ulong>(updateOwned, Mapping.Tpc, "ULongVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPH_and_table_splitting(bool updateDependent)
        => Row_version_with_table_splitting<StreetCircuit, City, ulong>(updateDependent, Mapping.Tph, "ULongVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPT_and_table_splitting(bool updateDependent)
        => Row_version_with_table_splitting<StreetCircuitTpt, CityTpt, ulong>(updateDependent, Mapping.Tpt, "ULongVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPC_and_table_splitting(bool updateDependent)
        => Row_version_with_table_splitting<StreetCircuitTpc, CityTpc, ulong>(updateDependent, Mapping.Tpc, "ULongVersion");
}

public class OptimisticConcurrencySqlServerTest : OptimisticConcurrencySqlServerTestBase<F1SqlServerFixture, byte[]>
{
    public OptimisticConcurrencySqlServerTest(F1SqlServerFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Row_version_with_TPH_and_owned_types(bool updateOwned)
        => Row_version_with_owned_types<SuperFan, List<byte>>(updateOwned, Mapping.Tph, "BinaryVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Row_version_with_TPT_and_owned_types(bool updateOwned)
        => Row_version_with_owned_types<SuperFanTpt, List<byte>>(updateOwned, Mapping.Tpt, "BinaryVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Row_version_with_TPC_and_owned_types(bool updateOwned)
        => Row_version_with_owned_types<SuperFanTpc, List<byte>>(updateOwned, Mapping.Tpc, "BinaryVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPH_and_table_splitting(bool updateDependent)
        => Row_version_with_table_splitting<StreetCircuit, City, List<byte>>(updateDependent, Mapping.Tph, "BinaryVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPT_and_table_splitting(bool updateDependent)
        => Row_version_with_table_splitting<StreetCircuitTpt, CityTpt, List<byte>>(updateDependent, Mapping.Tpt, "BinaryVersion");

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public Task Ulong_row_version_with_TPC_and_table_splitting(bool updateDependent)
        => Row_version_with_table_splitting<StreetCircuitTpc, CityTpc, List<byte>>(updateDependent, Mapping.Tpc, "BinaryVersion");
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

    protected async Task Row_version_with_owned_types<TEntity, TVersion>(bool updateOwned, Mapping mapping, string propertyName)
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
                var originalFanVersion = fanEntry.Property<TVersion>(propertyName).CurrentValue;
                var originalSwagVersion = default(TVersion);

                if (mapping == Mapping.Tph)
                {
                    originalSwagVersion
                        = swagEntry.Property<TVersion>(synthesizedPropertyName).CurrentValue;

                    Assert.Equal(originalFanVersion, originalSwagVersion);
                }

                if (updateOwned)
                {
                    fan.Swag.Stuff += "+";
                }
                else
                {
                    fan.Name += "+";
                }

                await using var innerContext = CreateF1Context();
                UseTransaction(innerContext.Database, transaction);
                var fanInner = innerContext.Set<TEntity>().Single(e => e.Name == "Alice");

                if (updateOwned)
                {
                    fanInner.Swag.Stuff += "-";
                }
                else
                {
                    fanInner.Name += "-";
                }

                await innerContext.SaveChangesAsync();

                if (!updateOwned || mapping != Mapping.Tpt) // Issue #22060
                {
                    await Assert.ThrowsAnyAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());

                    await fanEntry.ReloadAsync();
                    await swagEntry.ReloadAsync();

                    await context.SaveChangesAsync();

                    var fanVersion = fanEntry.Property<TVersion>(propertyName).CurrentValue;
                    Assert.NotEqual(originalFanVersion, fanVersion);

                    if (mapping == Mapping.Tph)
                    {
                        var swagVersion = swagEntry.Property<TVersion>(synthesizedPropertyName).CurrentValue;
                        Assert.Equal(fanVersion, swagVersion);
                        Assert.NotEqual(originalSwagVersion, swagVersion);
                    }
                }
                else
                {
                    await context.SaveChangesAsync();
                }
            });
    }

    protected async Task Row_version_with_table_splitting<TEntity, TCity, TVersion>(
        bool updateDependent,
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
                var originalCircuitVersion = circuitEntry.Property<TVersion>(propertyName).CurrentValue;
                var originalCityVersion = default(TVersion);

                if (mapping == Mapping.Tph)
                {
                    originalCityVersion
                        = cityEntry.Property<TVersion>(synthesizedPropertyName).CurrentValue;

                    Assert.Equal(originalCircuitVersion, originalCityVersion);
                }

                if (updateDependent)
                {
                    circuit.City.Name += "+";
                }
                else
                {
                    circuit.Name += "+";
                }

                await using var innerContext = CreateF1Context();
                UseTransaction(innerContext.Database, transaction);
                var fanInner = innerContext.Set<TEntity>().Include(e => e.City).Single(e => e.Name == "Monaco");

                if (updateDependent)
                {
                    fanInner.City.Name += "-";
                }
                else
                {
                    fanInner.Name += "-";
                }

                if (mapping == Mapping.Tpc)
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(() => innerContext.SaveChangesAsync());

                }
                else
                {
                    await innerContext.SaveChangesAsync();

                    if (!updateDependent || mapping != Mapping.Tpt) // Issue #22060
                    {
                        await Assert.ThrowsAnyAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());

                        await circuitEntry.ReloadAsync();
                        await cityEntry.ReloadAsync();

                        await context.SaveChangesAsync();

                        var circuitVersion = circuitEntry.Property<TVersion>(propertyName).CurrentValue;
                        Assert.NotEqual(originalCircuitVersion, circuitVersion);

                        if (mapping == Mapping.Tph)
                        {
                            var cityVersion = cityEntry.Property<TVersion>(synthesizedPropertyName).CurrentValue;
                            Assert.Equal(circuitVersion, cityVersion);
                            Assert.NotEqual(originalCityVersion, cityVersion);
                        }
                    }
                    else
                    {
                        await context.SaveChangesAsync();
                    }
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
