// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class WithNoLockSimpleTest<TFixture> : IClassFixture<TFixture>
        where TFixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>, new()
    {
        protected WithNoLockSimpleTest(TFixture fixture)
        {
            Fixture = fixture;
            Fixture.TestSqlLoggerFactory.Clear();
        }

        protected TFixture Fixture { get; }

        protected NorthwindContext CreateContext()
        {
            return Fixture.CreateContext();
        }

        [Fact]
        public virtual void With_nolock_default()
        {
            using (var context = CreateContext())
            {
                var customers = context
                    .Customers
                    .WithNoLock()
                    .Where(p=>p.CustomerID == "ALFKI" && p.PostalCode == "12209")
                    .ToList();

                Assert.Equal(1, customers.Count);
            }
        }

        [Fact]
        public virtual void With_nolock_parameter_false()
        {
            using (var context = CreateContext())
            {
                var customers = context
                    .Customers
                    .WithNoLock(false)
                    .Where(p => p.CustomerID == "ALFKI" && p.PostalCode == "12209")
                    .ToList();

                Assert.Equal(1, customers.Count);
            }
        }

        [Fact]
        public virtual void With_nolock_parameter_true()
        {
            using (var context = CreateContext())
            {
                var customers = context
                    .Customers
                    .WithNoLock(true)
                    .Where(p => p.CustomerID == "ALFKI" && p.PostalCode == "12209")
                    .ToList();

                Assert.Equal(1, customers.Count);
            }
        }
    }
}
