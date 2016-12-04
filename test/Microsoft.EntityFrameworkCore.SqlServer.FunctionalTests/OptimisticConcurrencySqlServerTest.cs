// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Specification.Tests;
using Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests.Utilities;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.SqlServer.FunctionalTests
{
    public class OptimisticConcurrencySqlServerTest : OptimisticConcurrencyTestBase<SqlServerTestStore, F1SqlServerFixture>
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
                await c.Database.CreateExecutionStrategy().ExecuteAsync(async context =>
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            var driver = context.Drivers.Single(d => d.CarNumber == 1);
                            Assert.NotEqual(1, context.Entry(driver).Property<byte[]>("Version").CurrentValue[0]);
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
                    }, c);
            }
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());
    }
}
