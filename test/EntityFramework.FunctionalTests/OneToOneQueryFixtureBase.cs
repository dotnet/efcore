// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class OneToOneQueryFixtureBase
    {
        protected static Model CreateModel()
        {
            var model = new Model();
            var modelBuilder = new ModelBuilder(model);

            modelBuilder
                .Entity<Person>(
                    e => e.OneToOne(p => p.Address, a => a.Resident));

            modelBuilder.Entity<Address>();

            return model;
        }

        protected static void AddTestData(DbContext context)
        {
            var address1 = new Address { Street = "3 Dragons Way", City = "Meereen" };
            var address2 = new Address { Street = "42 Castle Black", City = "The Wall" };
            var address3 = new Address { Street = "House of Black and White", City = "Braavos" };

            context.Set<Person>().AddRange(
                new[]
                    {
                        new Person { Name = "Daenerys Targaryen", Address = address1 },
                        new Person { Name = "John Snow", Address = address2 },
                        new Person { Name = "Arya Stark", Address = address3 }
                    }
                );

            context.Set<Address>().AddRange(
                new[]
                    {
                        address1,
                        address2,
                        address3
                    }
                );

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
}
