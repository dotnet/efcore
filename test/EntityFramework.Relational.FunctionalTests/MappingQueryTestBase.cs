// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Northwind;
using Xunit;

namespace Microsoft.Data.Entity.Relational.FunctionalTests
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

        protected abstract DbContext CreateContext();

        public class MappedCustomer : Customer
        {
            public string CompanyName2 { get; set; }
        }

        public class MappedEmployee : Employee
        {
            public string City2 { get; set; }
        }
    }
}
