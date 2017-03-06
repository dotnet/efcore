// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.Northwind;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class MappingQueryTestBase
    {
        [Fact]
        public virtual void All_customers()
        {
            using (var context = CreateContext())
            {
                var customers
                    = context.Set<MappedCustomer>()
                        .ToList();

                Assert.Equal(91, customers.Count);
            }
        }

        [Fact]
        public virtual void All_employees()
        {
            using (var context = CreateContext())
            {
                var employees
                    = context.Set<MappedEmployee>()
                        .ToList();

                Assert.Equal(9, employees.Count);
            }
        }

        [Fact]
        public virtual void All_orders()
        {
            using (var context = CreateContext())
            {
                var orders
                    = context.Set<MappedOrder>()
                        .ToList();

                Assert.Equal(830, orders.Count);
            }
        }

        [Fact]
        public virtual void Project_nullable_enum()
        {
            using (var context = CreateContext())
            {
                var orders
                    = context.Set<MappedOrder>()
                        .Select(o => o.ShipVia2)
                        .ToList();

                Assert.Equal(830, orders.Count);
            }
        }

        protected abstract DbContext CreateContext();

        public class MappedCustomer : Customer
        {
            public string CompanyName2 { get; set; }
        }

        public class MappedEmployee : Employee
        {
            public string City2 { get; set; }
        }

        public class MappedOrder : Order
        {
            public ShipVia? ShipVia2 { get; set; }
        }

        public enum ShipVia
        {
            One = 1,
            Two,
            Three
        }
    }
}
