// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class OptimisticConcurrencySqlServerTest : OptimisticConcurrencyRelationalTestBase<F1SqlServerFixture>
    {
        public OptimisticConcurrencySqlServerTest(F1SqlServerFixture fixture)
            : base(fixture)
        {
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
                    var firstVersion = context.Entry(driver).Property<byte[]>("Version").CurrentValue;
                    await context.SaveChangesAsync();

                    using var innerContext = CreateF1Context();
                    innerContext.Database.UseTransaction(transaction.GetDbTransaction());
                    driver = innerContext.Drivers.Single(d => d.CarNumber == 1);
                    Assert.NotEqual(firstVersion, innerContext.Entry(driver).Property<byte[]>("Version").CurrentValue);
                    Assert.Equal(StorePodiums, driver.Podiums);

                    var secondVersion = innerContext.Entry(driver).Property<byte[]>("Version").CurrentValue;
                    innerContext.Entry(driver).Property<byte[]>("Version").CurrentValue = firstVersion;
                    await innerContext.SaveChangesAsync();
                    using var validationContext = CreateF1Context();
                    validationContext.Database.UseTransaction(transaction.GetDbTransaction());
                    driver = validationContext.Drivers.Single(d => d.CarNumber == 1);
                    Assert.Equal(secondVersion, validationContext.Entry(driver).Property<byte[]>("Version").CurrentValue);
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
                    var sponsorVersion = sponsorEntry.Property<byte[]>("Version").CurrentValue;
                    var detailsVersion = detailsEntry.Property<byte[]>("Version").CurrentValue;

                    Assert.Null(sponsorEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue);
                    sponsorEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue = 1;

                    sponsor.Name = "Telecom";

                    Assert.Equal(sponsorVersion, detailsVersion);

                    await context.SaveChangesAsync();

                    var newSponsorVersion = sponsorEntry.Property<byte[]>("Version").CurrentValue;
                    var newDetailsVersion = detailsEntry.Property<byte[]>("Version").CurrentValue;

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
                    var sponsorVersion = sponsorEntry.Property<byte[]>("Version").CurrentValue;

                    Assert.Null(sponsorEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue);
                    sponsorEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue = 1;

                    sponsor.Details = new SponsorDetails { Days = 11, Space = 51m };

                    context.ChangeTracker.DetectChanges();

                    var detailsEntry = sponsorEntry.Reference(s => s.Details).TargetEntry;
                    detailsEntry.Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue = 1;

                    await context.SaveChangesAsync();

                    var newSponsorVersion = sponsorEntry.Property<byte[]>("Version").CurrentValue;
                    var newDetailsVersion = detailsEntry.Property<byte[]>("Version").CurrentValue;

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
                @"SELECT TOP(1) [e].[Id], [e].[EngineSupplierId], [e].[Name], [e].[StorageLocation_Latitude], [e].[StorageLocation_Longitude]
FROM [Engines] AS [e]
ORDER BY [e].[Id]",
                //
                @"@p1='1'
@p2='Mercedes' (Size = 450)
@p0='FO 108X' (Size = 4000)
@p3='ChangedEngine' (Size = 4000)
@p4='47.64491' (Nullable = true)
@p5='-122.128101' (Nullable = true)

SET NOCOUNT ON;
UPDATE [Engines] SET [Name] = @p0
WHERE [Id] = @p1 AND [EngineSupplierId] = @p2 AND [Name] = @p3 AND [StorageLocation_Latitude] = @p4 AND [StorageLocation_Longitude] = @p5;
SELECT @@ROWCOUNT;");
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());
    }
}
