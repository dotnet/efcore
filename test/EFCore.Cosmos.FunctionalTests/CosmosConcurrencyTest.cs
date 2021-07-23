﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class CosmosConcurrencyTest : IClassFixture<CosmosConcurrencyTest.CosmosFixture>
    {
        private const string DatabaseName = "CosmosConcurrencyTest";

        protected CosmosFixture Fixture { get; }

        public CosmosConcurrencyTest(CosmosFixture fixture)
        {
            Fixture = fixture;
        }

        [ConditionalFact]
        public virtual Task Adding_the_same_entity_twice_results_in_DbUpdateException()
        {
            return ConcurrencyTestAsync<DbUpdateException>(
                ctx => ctx.Customers.Add(
                    new Customer
                    {
                        Id = "1",
                        Name = "CreatedTwice",
                    }));
        }

        [ConditionalFact]
        public virtual Task Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException()
        {
            return ConcurrencyTestAsync<DbUpdateConcurrencyException>(
                ctx => ctx.Customers.Add(
                    new Customer
                    {
                        Id = "2",
                        Name = "Added",
                    }),
                ctx => ctx.Customers.Single(c => c.Id == "2").Name = "Updated",
                ctx => ctx.Customers.Remove(ctx.Customers.Single(c => c.Id == "2")));
        }

        [ConditionalFact]
        public virtual Task Updating_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException()
        {
            return ConcurrencyTestAsync<DbUpdateConcurrencyException>(
                ctx => ctx.Customers.Add(
                    new Customer
                    {
                        Id = "3",
                        Name = "Added",
                    }),
                ctx => ctx.Customers.Single(c => c.Id == "3").Name = "Updated",
                ctx => ctx.Customers.Single(c => c.Id == "3").Name = "Updated");
        }

        [ConditionalFact]
        public async Task Etag_will_return_when_content_response_enabled_false()
        {
            await using var testDatabase = CosmosTestStore.CreateInitialized(DatabaseName);

            var customer = new Customer
            {
                Id = "4",
                Name = "Theon",
            };

            await using (var context = new ConcurrencyContext(CreateOptions(testDatabase, enableContentResponseOnWrite: false)))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(customer);

                await context.SaveChangesAsync();
            }

            await using (var context = new ConcurrencyContext(CreateOptions(testDatabase, enableContentResponseOnWrite: false)))
            {
                var customerFromStore = await context.Set<Customer>().SingleAsync();

                Assert.Equal(customer.Id, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
                Assert.Equal(customer.ETag, customerFromStore.ETag);

                context.Remove(customerFromStore);

                await context.SaveChangesAsync();
            }
        }

        [ConditionalFact]
        public async Task Etag_will_return_when_content_response_enabled_true()
        {
            await using var testDatabase = CosmosTestStore.Create(DatabaseName);

            var customer = new Customer
            {
                Id = "3",
                Name = "Theon",
            };

            await using (var context = new ConcurrencyContext(CreateOptions(testDatabase, enableContentResponseOnWrite: true)))
            {
                await context.Database.EnsureCreatedAsync();

                context.Add(customer);

                await context.SaveChangesAsync();
            }

            await using (var context = new ConcurrencyContext(CreateOptions(testDatabase, enableContentResponseOnWrite: true)))
            {
                var customerFromStore = await context.Set<Customer>().SingleAsync();

                Assert.Equal(customer.Id, customerFromStore.Id);
                Assert.Equal("Theon", customerFromStore.Name);
                Assert.Equal(customer.ETag, customerFromStore.ETag);

                context.Remove(customerFromStore);

                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        ///     Runs the two actions with two different contexts and calling
        ///     SaveChanges such that storeChange will succeed and the store will reflect this change, and
        ///     then clientChange will result in a concurrency exception.
        ///     After the exception is caught the resolver action is called, after which SaveChanges is called
        ///     again. Finally, a new context is created and the validator is called so that the state of
        ///     the database at the end of the process can be validated.
        /// </summary>
        protected virtual Task ConcurrencyTestAsync<TException>(
            Action<ConcurrencyContext> change)
            where TException : DbUpdateException
            => ConcurrencyTestAsync<TException>(
                null, change, change);

        /// <summary>
        ///     Runs the two actions with two different contexts and calling
        ///     SaveChanges such that storeChange will succeed and the store will reflect this change, and
        ///     then clientChange will result in a concurrency exception.
        ///     After the exception is caught the resolver action is called, after which SaveChanges is called
        ///     again. Finally, a new context is created and the validator is called so that the state of
        ///     the database at the end of the process can be validated.
        /// </summary>
        protected virtual async Task ConcurrencyTestAsync<TException>(
            Action<ConcurrencyContext> seedAction,
            Action<ConcurrencyContext> storeChange,
            Action<ConcurrencyContext> clientChange)
            where TException : DbUpdateException
        {
            using var outerContext = CreateContext();
            await outerContext.Database.EnsureCreatedAsync();
            seedAction?.Invoke(outerContext);
            await outerContext.SaveChangesAsync();

            clientChange?.Invoke(outerContext);

            using (var innerContext = CreateContext())
            {
                storeChange?.Invoke(innerContext);
                await innerContext.SaveChangesAsync();
            }

            var updateException =
                await Assert.ThrowsAnyAsync<TException>(() => outerContext.SaveChangesAsync());

            var entry = updateException.Entries.Single();
            Assert.IsAssignableFrom<Customer>(entry.Entity);
        }

        protected ConcurrencyContext CreateContext()
            => Fixture.CreateContext();

        public class CosmosFixture : SharedStoreFixtureBase<ConcurrencyContext>
        {
            protected override string StoreName
                => DatabaseName;

            protected override ITestStoreFactory TestStoreFactory
                => CosmosTestStoreFactory.Instance;
        }

        public class ConcurrencyContext : PoolableDbContext
        {
            public ConcurrencyContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Customer> Customers { get; set; }

            protected override void OnModelCreating(ModelBuilder builder)
            {
                builder.Entity<Customer>(
                    b =>
                    {
                        b.HasKey(c => c.Id);
                        b.Property(c => c.ETag).IsETagConcurrency();
                    });
            }
        }

        private DbContextOptions CreateOptions(CosmosTestStore testDatabase, bool enableContentResponseOnWrite)
        {
            var optionsBuilder = new DbContextOptionsBuilder();

            new DbContextOptionsBuilder().UseCosmos(testDatabase.ConnectionString, testDatabase.Name,
                b => b.ApplyConfiguration().ContentResponseOnWriteEnabled(enabled: enableContentResponseOnWrite));

            return testDatabase.AddProviderOptions(optionsBuilder)
                .EnableDetailedErrors()
                .Options;
        }

        public class Customer
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string ETag { get; set; }
        }
    }
}
