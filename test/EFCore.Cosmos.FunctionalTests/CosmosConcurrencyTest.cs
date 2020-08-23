// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
                        Id = "1", Name = "CreatedTwice",
                    }));
        }

        [ConditionalFact]
        public virtual Task Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException()
        {
            return ConcurrencyTestAsync<DbUpdateConcurrencyException>(
                ctx => ctx.Customers.Add(
                    new Customer
                    {
                        Id = "2", Name = "Added",
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
                        Id = "3", Name = "Added",
                    }),
                ctx => ctx.Customers.Single(c => c.Id == "3").Name = "Updated",
                ctx => ctx.Customers.Single(c => c.Id == "3").Name = "Updated");
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

        public class Customer
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string ETag { get; set; }
        }
    }
}
