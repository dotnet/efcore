// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public class OptimisticConcurrencySqlServerTest : OptimisticConcurrencyTestBase<F1SqlServerFixture>
    {
        public OptimisticConcurrencySqlServerTest(F1SqlServerFixture fixture)
            : base(fixture)
        {
        }

        [Fact]
        public async Task Modifying_concurrency_token_only_is_noop()
        {
            using (var c = CreateF1Context())
            {
                await c.Database.CreateExecutionStrategy().ExecuteAsync(
                    c, async context =>
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            var driver = context.Drivers.Single(d => d.CarNumber == 1);
                            driver.Podiums = StorePodiums;
                            var firstVersion = context.Entry(driver).Property<byte[]>("Version").CurrentValue;
                            await context.SaveChangesAsync();

                            using (var innerContext = CreateF1Context())
                            {
                                innerContext.Database.UseTransaction(transaction.GetDbTransaction());
                                driver = innerContext.Drivers.Single(d => d.CarNumber == 1);
                                Assert.NotEqual(firstVersion, innerContext.Entry(driver).Property<byte[]>("Version").CurrentValue);
                                Assert.Equal(StorePodiums, driver.Podiums);

                                var secondVersion = innerContext.Entry(driver).Property<byte[]>("Version").CurrentValue;
                                innerContext.Entry(driver).Property<byte[]>("Version").CurrentValue = firstVersion;
                                await innerContext.SaveChangesAsync();
                                using (var validationContext = CreateF1Context())
                                {
                                    validationContext.Database.UseTransaction(transaction.GetDbTransaction());
                                    driver = validationContext.Drivers.Single(d => d.CarNumber == 1);
                                    Assert.Equal(secondVersion, validationContext.Entry(driver).Property<byte[]>("Version").CurrentValue);
                                    Assert.Equal(StorePodiums, driver.Podiums);
                                }
                            }
                        }
                    });
            }
        }

        [Fact]
        public Task Database_concurrency_token_value_is_discarded_for_non_conflicting_entities()
        {
            byte[] firstVersion = null;
            byte[] secondVersion = null;
            return ConcurrencyTestAsync(
                c => c.Drivers.Single(d => d.CarNumber == 2).Podiums = StorePodiums,
                c =>
                {
                    var driver = c.Drivers.Single(d => d.CarNumber == 1);
                    driver.Podiums = ClientPodiums;
                    firstVersion = c.Entry(driver).Property<byte[]>("Version").CurrentValue;

                    var secondDriver = c.Drivers.Single(d => d.CarNumber == 2);
                    secondDriver.Podiums = ClientPodiums;
                    secondVersion = c.Entry(secondDriver).Property<byte[]>("Version").CurrentValue;
                },
                (c, ex) =>
                {
                    Assert.IsType<DbUpdateConcurrencyException>(ex);

                    var firstDriverEntry = c.Entry(c.Drivers.Local.Single(d => d.CarNumber == 1));
                    Assert.Equal(firstVersion, firstDriverEntry.Property<byte[]>("Version").CurrentValue);
                    var databaseValues = firstDriverEntry.GetDatabaseValues();
                    Assert.NotEqual(firstVersion, databaseValues["Version"]);
                    firstDriverEntry.OriginalValues.SetValues(databaseValues);

                    var secondDriverEntry = ex.Entries.Single();
                    Assert.Equal(secondVersion, secondDriverEntry.Property("Version").CurrentValue);
                    secondDriverEntry.OriginalValues.SetValues(secondDriverEntry.GetDatabaseValues());
                    ResolveConcurrencyTokens(secondDriverEntry);
                },
                c => Assert.Equal(ClientPodiums, c.Drivers.Single(d => d.CarNumber == 2).Podiums));
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());
    }
}
