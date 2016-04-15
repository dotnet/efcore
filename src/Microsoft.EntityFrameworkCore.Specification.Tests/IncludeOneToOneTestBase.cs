// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class IncludeOneToOneTestBase
    {
        [Fact]
        public virtual void Include_address()
        {
            using (var context = CreateContext())
            {
                var people
                    = context.Set<Person>()
                        .Include(p => p.Address)
                        .ToList();

                Assert.Equal(4, people.Count);
                Assert.Equal(3, people.Count(p => p.Address != null));
                Assert.Equal(4 + 3, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void Include_address_shadow()
        {
            using (var context = CreateContext())
            {
                var people
                    = context.Set<Person2>()
                        .Include(p => p.Address)
                        .ToList();

                Assert.Equal(3, people.Count);
                Assert.True(people.All(p => p.Address != null));
                Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void Include_address_no_tracking()
        {
            using (var context = CreateContext())
            {
                var people
                    = context.Set<Person>()
                        .Include(p => p.Address)
                        .AsNoTracking()
                        .ToList();

                Assert.Equal(4, people.Count);
                Assert.Equal(3, people.Count(p => p.Address != null));
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void Include_person()
        {
            using (var context = CreateContext())
            {
                var addresses
                    = context.Set<Address>()
                        .Include(a => a.Resident)
                        .ToList();

                Assert.Equal(3, addresses.Count);
                Assert.True(addresses.All(p => p.Resident != null));
                Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void Include_person_shadow()
        {
            using (var context = CreateContext())
            {
                var addresses
                    = context.Set<Address2>()
                        .Include(a => a.Resident)
                        .ToList();

                Assert.Equal(3, addresses.Count);
                Assert.True(addresses.All(p => p.Resident != null));
                Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void Include_person_no_tracking()
        {
            using (var context = CreateContext())
            {
                var addresses
                    = context.Set<Address>()
                        .Include(a => a.Resident)
                        .AsNoTracking()
                        .ToList();

                Assert.Equal(3, addresses.Count);
                Assert.True(addresses.All(p => p.Resident != null));
                Assert.Equal(0, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void Include_address_when_person_already_tracked()
        {
            using (var context = CreateContext())
            {
                var person
                    = context.Set<Person>()
                        .Single(p => p.Name == "John Snow");

                var people
                    = context.Set<Person>()
                        .Include(p => p.Address)
                        .ToList();

                Assert.Equal(4, people.Count);
                Assert.True(people.Contains(person));
                Assert.Equal(3, people.Count(p => p.Address != null));
                Assert.Equal(4 + 3, context.ChangeTracker.Entries().Count());
            }
        }

        [Fact]
        public virtual void Include_person_when_address_already_tracked()
        {
            using (var context = CreateContext())
            {
                var address
                    = context.Set<Address>()
                        .Single(a => a.City == "Meereen");

                var addresses
                    = context.Set<Address>()
                        .Include(a => a.Resident)
                        .ToList();

                Assert.Equal(3, addresses.Count);
                Assert.True(addresses.Contains(address));
                Assert.True(addresses.All(p => p.Resident != null));
                Assert.Equal(3 + 3, context.ChangeTracker.Entries().Count());
            }
        }

        protected abstract DbContext CreateContext();
    }
}
