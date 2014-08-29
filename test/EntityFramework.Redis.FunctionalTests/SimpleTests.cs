// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class SimpleTests : IClassFixture<SimpleFixture>
    {
        private readonly DbContext _context;

        public SimpleTests(SimpleFixture fixture)
        {
            _context = fixture.CreateContext();
        }

        [Fact]
        public void Add_modify_and_delete_Customer()
        {
            var simplePoco = _context.Set<SimplePoco>().Add(
                new SimplePoco
                    {
                        PocoKey = 100,
                        Name = "A. Name",
                    });
            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            simplePoco.Name = "Updated Name";
            changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            _context.Set<SimplePoco>().Remove(simplePoco);
            changes = _context.SaveChanges();
            Assert.Equal(1, changes);
        }

        [Fact]
        public void Get_customer_count()
        {
            _context.Set<SimplePoco>().Add(
                new SimplePoco
                {
                    PocoKey = 200,
                    Name = "B. Name",
                });
            var changes = _context.SaveChanges();
            Assert.Equal(1, changes);

            var simplePocos =
                from c in _context.Set<SimplePoco>()
                select c;
            Assert.Equal(1, simplePocos.Count(cust => cust.PocoKey == 200));
        }

        [Fact]
        public void Get_customer_projection()
        {
            _context.Set<SimplePoco>().Add(
                new SimplePoco
                {
                    PocoKey = 300,
                    Name = "C. Name",
                });
            _context.Set<SimplePoco>().Add(
                new SimplePoco
                {
                    PocoKey = 301,
                    Name = "C. Name the 2nd",
                });
            var changes = _context.SaveChanges();
            Assert.Equal(2, changes);

            var simplePocoNames =
                from simplePoco in _context.Set<SimplePoco>()
                where (simplePoco.PocoKey == 300 || simplePoco.PocoKey == 301)
                select simplePoco.Name;

            var simplePocoNamesArray = simplePocoNames.ToArray();
            Assert.Equal(2, simplePocoNamesArray.Length);
            Assert.Equal("C. Name", simplePocoNamesArray[0]);
            Assert.Equal("C. Name the 2nd", simplePocoNamesArray[1]);
        }
    }
}
