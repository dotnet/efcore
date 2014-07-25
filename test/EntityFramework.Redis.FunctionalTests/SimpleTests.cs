// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Redis
{
    [RequiresRedisServer]
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
                        CustomerID = 1,
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
    }
}
