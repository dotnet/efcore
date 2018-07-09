// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Cosmos.Sql.TestUtilities;
using Microsoft.EntityFrameworkCore.TestUtilities;
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

        [Fact]
        // TODO: Remove ToList when Single/Count works
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
                    var customerFromStore = context.Set<Customer>().ToList().Single();

                    Assert.Equal(42, customerFromStore.Id);
                    Assert.Equal("Theon", customerFromStore.Name);
                }

                using (var context = new CustomerContext(options))
                {
                    customer.Name = "Theon Greyjoy";
                    context.Update(customer);

                    await context.SaveChangesAsync();
                }

                using (var context = new CustomerContext(options))
                {
                    var customerFromStore = context.Set<Customer>().ToList().Single();

                    Assert.Equal(42, customerFromStore.Id);
                    Assert.Equal("Theon Greyjoy", customerFromStore.Name);
                }

                using (var context = new CustomerContext(options))
                {
                    context.Remove(customer);

                    await context.SaveChangesAsync();
                }

                using (var context = new CustomerContext(options))
                {
                    Assert.Equal(0, context.Set<Customer>().ToList().Count());
                }
            }
        }

        private class Customer
        {
            public Customer()
            {
            }

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

        public class CosmosSqlFixture : ServiceProviderFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => CosmosSqlTestStoreFactory.Instance;
        }
    }
}
