using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class ContentResponseEtagTest
    {
        [ConditionalFact]
        public async Task Etag_will_return_when_content_response_enabled_false()
        {
            await using var testDatabase = CosmosTestStore.Create("CustomerDemo");

            var customer = new CustomerWithEtag
            {
                Id = Guid.NewGuid(),
                Name = "Theon",
            };

            using (var context = new CustomerContextWithContentResponse(testDatabase, false))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContextWithContentResponse(testDatabase, false))
            {
                var customerFromStore = await context.Set<CustomerWithEtag>().SingleAsync();

                Assert.Equal(customer.Id, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
                Assert.Equal(customer.ETag, customerFromStore.ETag);

                context.Remove(customerFromStore);

                context.SaveChanges();
            }
        }

        [ConditionalFact]
        public async Task Etag_will_return_when_content_response_enabled_true()
        {
            await using var testDatabase = CosmosTestStore.Create("CustomerDemo");

            var customer = new CustomerWithEtag
            {
                Id = Guid.NewGuid(),
                Name = "Theon",
            };

            using (var context = new CustomerContextWithContentResponse(testDatabase, true))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(customer);

                await context.SaveChangesAsync();
            }

            using (var context = new CustomerContextWithContentResponse(testDatabase, true))
            {
                var customerFromStore = await context.Set<CustomerWithEtag>().SingleAsync();

                Assert.Equal(customer.Id, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
                Assert.Equal(customer.ETag, customerFromStore.ETag);

                context.Remove(customerFromStore);

                context.SaveChanges();
            }
        }

        private class CustomerContextWithContentResponse : DbContext
        {
            private readonly string _connectionString;
            private readonly string _name;
            private readonly bool _contentResponseOnWriteEnabled;

            public CustomerContextWithContentResponse(CosmosTestStore testStore, bool contentResponseOnWriteEnabled)
            {
                _connectionString = testStore.ConnectionString;
                _name = testStore.Name;
                _contentResponseOnWriteEnabled = contentResponseOnWriteEnabled;
            }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseCosmos(_connectionString, _name, b => b.ApplyConfiguration().ContentResponseOnWriteEnabled(_contentResponseOnWriteEnabled));
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<CustomerWithEtag>(
                    b =>
                    {
                        b.HasKey(c => c.Id);
                        b.Property(c => c.ETag).IsETagConcurrency();
                    });
            }

            public DbSet<CustomerWithEtag> Customers { get; set; }
        }

        private class CustomerWithEtag
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string ETag { get; set; }
        }
    }
}
