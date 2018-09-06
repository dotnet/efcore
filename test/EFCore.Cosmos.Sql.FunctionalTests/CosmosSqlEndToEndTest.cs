// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos.Sql
{
    public class CosmosSqlEndToEndTest : IClassFixture<CosmosSqlEndToEndTest.CosmosSqlFixture>
    {
        private const string DatabaseName = "CosmosSqlEndToEndTest";

        protected CosmosSqlFixture Fixture { get; }

        public CosmosSqlEndToEndTest(CosmosSqlFixture fixture)
        {
            Fixture = fixture;
        }

        [ConditionalFact]
        public async Task Can_add_update_delete_end_to_end()
        {
            using (var testDatabase = CosmosSqlTestStore.CreateInitialized(DatabaseName))
            {
                var options = Fixture.CreateOptions(testDatabase);

                var customer = new Customer { Id = 42, Name = "Theon" };

                using (var context = new CustomerContext(options))
                {
                    await context.Database.EnsureCreatedAsync();

                    context.Add(customer);

                    await context.SaveChangesAsync();
                }

                using (var context = new CustomerContext(options))
                {
                    var customerFromStore = context.Set<Customer>().Single();

                    Assert.Equal(42, customerFromStore.Id);
                    Assert.Equal("Theon", customerFromStore.Name);

                    customerFromStore.Name = "Theon Greyjoy";

                    await context.SaveChangesAsync();
                }

                using (var context = new CustomerContext(options))
                {
                    var customerFromStore = context.Set<Customer>().Single();

                    Assert.Equal(42, customerFromStore.Id);
                    Assert.Equal("Theon Greyjoy", customerFromStore.Name);

                    context.Remove(customerFromStore);

                    await context.SaveChangesAsync();
                }

                using (var context = new CustomerContext(options))
                {
                    Assert.Equal(0, context.Set<Customer>().Count());
                }
            }
        }

        private class Customer
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class CustomerContext : DbContext
        {
            public CustomerContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Customer>();
            }
        }

        [ConditionalFact]
        public async Task Using_a_conflicting_incompatible_id_throws()
        {
            using (var testDatabase = CosmosSqlTestStore.CreateInitialized(DatabaseName))
            {
                var options = Fixture.CreateOptions(testDatabase);

                using (var context = new ConflictingIncompatibleIdContext(options))
                {
                    await Assert.ThrowsAnyAsync<Exception>(async () =>
                    {
                        await context.Database.EnsureCreatedAsync();

                        context.Add(new ConflictingIncompatibleId { id = 42 });

                        await context.SaveChangesAsync();
                    });
                }
            }
        }

        private class ConflictingIncompatibleId
        {
            public int id { get; set; }
            public string Name { get; set; }
        }

        public class ConflictingIncompatibleIdContext : DbContext
        {
            public ConflictingIncompatibleIdContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ConflictingIncompatibleId>();
            }
        }

        [ConditionalFact]
        public async Task Can_add_update_delete_end_to_end_with_conflicting_id()
        {
            using (var testDatabase = CosmosSqlTestStore.CreateInitialized(DatabaseName))
            {
                var options = Fixture.CreateOptions(testDatabase);

                var entity = new ConflictingId { id = "42", Name = "Theon" };

                using (var context = new ConflictingIdContext(options))
                {
                    await context.Database.EnsureCreatedAsync();

                    context.Add(entity);

                    await context.SaveChangesAsync();
                }

                using (var context = new ConflictingIdContext(options))
                {
                    var entityFromStore = context.Set<ConflictingId>().Single();

                    Assert.Equal("42", entityFromStore.id);
                    Assert.Equal("Theon", entityFromStore.Name);
                }

                using (var context = new ConflictingIdContext(options))
                {
                    entity.Name = "Theon Greyjoy";
                    context.Update(entity);

                    await context.SaveChangesAsync();
                }

                using (var context = new ConflictingIdContext(options))
                {
                    var entityFromStore = context.Set<ConflictingId>().Single();

                    Assert.Equal("42", entityFromStore.id);
                    Assert.Equal("Theon Greyjoy", entityFromStore.Name);
                }

                using (var context = new ConflictingIdContext(options))
                {
                    context.Remove(entity);

                    await context.SaveChangesAsync();
                }

                using (var context = new ConflictingIdContext(options))
                {
                    Assert.Equal(0, context.Set<ConflictingId>().Count());
                }
            }
        }

        private class ConflictingId
        {
            public string id { get; set; }
            public string Name { get; set; }
        }

        public class ConflictingIdContext : DbContext
        {
            public ConflictingIdContext(DbContextOptions dbContextOptions)
                : base(dbContextOptions)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ConflictingId>();
            }
        }

        public class CosmosSqlFixture : ServiceProviderFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => CosmosSqlTestStoreFactory.Instance;
        }
    }
}
