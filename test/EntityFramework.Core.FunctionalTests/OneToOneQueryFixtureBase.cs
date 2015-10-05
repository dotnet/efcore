// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class OneToOneQueryFixtureBase
    {
        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<Address>(e => e.HasOne(a => a.Resident).WithOne(p => p.Address)
                    .HasPrincipalKey<Person>(person => person.Id));

            modelBuilder.Entity<Address2>().Property<int>("PersonId");

            modelBuilder
                .Entity<Person2>(
                    e => e.HasOne(p => p.Address).WithOne(a => a.Resident)
                        .HasForeignKey(typeof(Address2), "PersonId"));
        }

        protected static void AddTestData(DbContext context)
        {
            var address1 = new Address { Street = "3 Dragons Way", City = "Meereen" };
            var address2 = new Address { Street = "42 Castle Black", City = "The Wall" };
            var address3 = new Address { Street = "House of Black and White", City = "Braavos" };

            context.Set<Person>().AddRange(new Person { Name = "Daenerys Targaryen", Address = address1 }, new Person { Name = "John Snow", Address = address2 }, new Person { Name = "Arya Stark", Address = address3 }, new Person { Name = "Harry Strickland" });

            context.Set<Address>().AddRange(address1, address2, address3);

            var address21 = new Address2 { Id = "1", Street = "3 Dragons Way", City = "Meereen" };
            var address22 = new Address2 { Id = "2", Street = "42 Castle Black", City = "The Wall" };
            var address23 = new Address2 { Id = "3", Street = "House of Black and White", City = "Braavos" };

            context.Set<Person2>().AddRange(new Person2 { Name = "Daenerys Targaryen", Address = address21 }, new Person2 { Name = "John Snow", Address = address22 }, new Person2 { Name = "Arya Stark", Address = address23 });

            context.Set<Address2>().AddRange(address21, address22, address23);

            context.SaveChanges();
        }
    }

    public class Person
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
    }

    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public Person Resident { get; set; }
    }

    public class Person2
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Address2 Address { get; set; }
    }

    public class Address2
    {
        public string Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public Person2 Resident { get; set; }
    }
}
