// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests.TestModels.Northwind;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class SqlExecutorTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryFixtureBase, new()
    {
        [Fact]
        public virtual void Executes_stored_procedure()
        {
            using (var context = CreateContext())
            {
                context.Database.ExecuteSqlCommand(TenMostExpensiveProductsSproc);
            }
        }

        [Fact]
        public virtual async Task Executes_stored_procedure_async()
        {
            using (var context = CreateContext())
            {
                await context.Database.ExecuteSqlCommandAsync(TenMostExpensiveProductsSproc);
            }
        }

        [Fact]
        public virtual void Executes_stored_procedure_with_parameter()
        {
            using (var context = CreateContext())
            {
                context.Database.ExecuteSqlCommand(CustomerOrderHistorySproc, "ALFKI");
            }
        }

        [Fact]
        public virtual async Task Executes_stored_procedure_with_parameter_async()
        {
            using (var context = CreateContext())
            {
                await context.Database.ExecuteSqlCommandAsync(
                    CustomerOrderHistorySproc,
                    default(CancellationToken),
                    "ALFKI");
            }
        }

        protected NorthwindContext CreateContext()
        {
            return Fixture.CreateContext();
        }

        protected SqlExecutorTestBase(TFixture fixture)
        {
            Fixture = fixture;
        }

        protected TFixture Fixture { get; }

        protected abstract string TenMostExpensiveProductsSproc { get; }

        protected abstract string CustomerOrderHistorySproc { get; }
    }
}
