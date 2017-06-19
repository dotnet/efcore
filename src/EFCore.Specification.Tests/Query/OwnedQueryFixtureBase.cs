// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class OwnedQueryFixtureBase
    {
        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OwnedPerson>().OwnsOne(p => p.PersonAddress).OwnsOne(a => a.Country);
            modelBuilder.Entity<Branch>().OwnsOne(p => p.BranchAddress).OwnsOne(a => a.Country);
            modelBuilder.Entity<LeafA>().OwnsOne(p => p.LeafAAddress).OwnsOne(a => a.Country);
            modelBuilder.Entity<LeafB>().OwnsOne(p => p.LeafBAddress).OwnsOne(a => a.Country);
        }

        protected static void AddTestData(DbContext context)
        {
            context.Set<OwnedPerson>().AddRange(
                new OwnedPerson
                {
                    PersonAddress = new OwnedAddress
                    {
                        Country = new OwnedCountry { Name = "USA" }
                    }
                },
                new Branch
                {
                    PersonAddress = new OwnedAddress
                    {
                        Country = new OwnedCountry { Name = "USA" }
                    },
                    BranchAddress = new OwnedAddress
                    {
                        Country = new OwnedCountry { Name = "Canada" }
                    }
                },
                new LeafA
                {
                    PersonAddress = new OwnedAddress
                    {
                        Country = new OwnedCountry { Name = "USA" }
                    },
                    BranchAddress = new OwnedAddress
                    {
                        Country = new OwnedCountry { Name = "Canada" }
                    },
                    LeafAAddress = new OwnedAddress
                    {
                        Country = new OwnedCountry { Name = "Mexico" }
                    }
                },
                new LeafB
                {
                    PersonAddress = new OwnedAddress
                    {
                        Country = new OwnedCountry { Name = "USA" }
                    },
                    LeafBAddress = new OwnedAddress
                    {
                        Country = new OwnedCountry { Name = "Panama" }
                    }
                });

            context.SaveChanges();
        }
    }

    public class OwnedAddress
    {
        public OwnedCountry Country { get; set; }
    }

    public class OwnedCountry
    {
        public string Name { get; set; }
    }

    public class OwnedPerson
    {
        public int Id { get; set; }
        public OwnedAddress PersonAddress { get; set; }
    }

    public class Branch : OwnedPerson
    {
        public OwnedAddress BranchAddress { get; set; }
    }

    public class LeafA : Branch
    {
        public OwnedAddress LeafAAddress { get; set; }
    }

    public class LeafB : OwnedPerson
    {
        public OwnedAddress LeafBAddress { get; set; }
    }
}
