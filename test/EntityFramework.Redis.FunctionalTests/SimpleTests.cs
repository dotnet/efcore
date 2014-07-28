// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Redis
{
    public class SimpleTests : IClassFixture<SimpleFixture>
    {
        private readonly DbContext _context;

        public SimpleTests(SimpleFixture fixture)
        {
            _context = fixture.GetOrCreateContext();
            _context.Database.EnsureCreated();
        }

        [Fact]
        public void Add_modify_and_delete_Customer()
        {
            var cust = _context.Set<Customer>().Add(
                new Customer
                    {
                        CustomerID = 100,
                        Name = "A. Customer",
                    });
            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            cust.Name = "Updated Customer";
            changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            _context.Set<Customer>().Remove(cust);
            changes = _context.SaveChanges();
            Assert.Equal(1, changes);
        }

        [Fact]
        public void Get_customer_count()
        {
            _context.Set<Customer>().Add(
                new Customer
                {
                    CustomerID = 200,
                    Name = "B. Customer",
                });
            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            var custs =
                from c in _context.Set<Customer>()
                select c;
            Assert.Equal(1, custs.Count(cust => cust.CustomerID == 200));
        }

        [Fact]
        public void Get_customer_projection()
        {
            _context.Set<Customer>().Add(
                new Customer
                {
                    CustomerID = 300,
                    Name = "C. Customer",
                });
            _context.Set<Customer>().Add(
                new Customer
                {
                    CustomerID = 301,
                    Name = "C. Customer the 2nd",
                });
            var changes = _context.SaveChanges();
            Assert.Equal(2, changes);

            var custNames =
                from cust in _context.Set<Customer>()
                where (cust.CustomerID == 300 || cust.CustomerID == 301)
                select cust.Name;

            var custNamesArray = custNames.ToArray();
            Assert.Equal(2, custNamesArray.Length);
            Assert.Equal("C. Customer", custNamesArray[0]);
            Assert.Equal("C. Customer the 2nd", custNamesArray[1]);
        }
    }
}
