// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable CA1720
#pragma warning disable CA1716

// ReSharper disable UnusedVariable
// ReSharper disable InconsistentNaming
// ReSharper disable MethodHasAsyncOverload
namespace Microsoft.EntityFrameworkCore
{
    public abstract class ConcurrencyDetectorTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : ConcurrencyDetectorTestBase<TFixture>.ConcurrencyDetectorFixtureBase, new()
    {
        protected ConcurrencyDetectorTestBase(TFixture fixture)
            => Fixture = fixture;

        protected TFixture Fixture { get; }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Find(bool async)
            => ConcurrencyDetectorTest(async c => async ? await c.Products.FindAsync(1) : c.Products.Find(1));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Count(bool async)
            => ConcurrencyDetectorTest(async c => async ? await c.Products.CountAsync() : c.Products.Count());

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task First(bool async)
            => ConcurrencyDetectorTest(
                async c => async
                    ? await c.Products.OrderBy(p => p.Id).FirstAsync()
                    : c.Products.OrderBy(p => p.Id).First());

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Last(bool async)
            => ConcurrencyDetectorTest(
                async c => async
                    ? await c.Products.OrderBy(p => p.Id).LastAsync()
                    : c.Products.OrderBy(p => p.Id).Last());

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Single(bool async)
            => ConcurrencyDetectorTest(
                async c => async
                    ? await c.Products.SingleAsync(p => p.Id == 1)
                    : c.Products.Single(p => p.Id == 1));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task Any(bool async)
            => ConcurrencyDetectorTest(
                async c => async
                    ? await c.Products.AnyAsync(p => p.Id < 10)
                    : c.Products.Any(p => p.Id < 10));

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual Task ToList(bool async)
            => ConcurrencyDetectorTest(async c => async ? await c.Products.ToListAsync() : c.Products.ToList());

        protected abstract Task ConcurrencyDetectorTest(Func<ConcurrencyDetectorDbContext, Task<object>> test);

        protected ConcurrencyDetectorDbContext CreateContext()
            => Fixture.CreateContext();

        public class ConcurrencyDetectorDbContext : DbContext
        {
            public ConcurrencyDetectorDbContext(DbContextOptions<ConcurrencyDetectorDbContext> options)
                : base(options)
            {
            }

            public DbSet<Product> Products { get; set; }

            public static void Seed(ConcurrencyDetectorDbContext context)
            {
                context.Products.Add(new() { Id = 1, Name = "Unicorn Party Pack"});
                context.SaveChanges();
            }
        }

        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public abstract class ConcurrencyDetectorFixtureBase : SharedStoreFixtureBase<ConcurrencyDetectorDbContext>
        {
            protected override string StoreName => "ConcurrencyDetector";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
                => modelBuilder.Entity<Product>().Property(p => p.Id).ValueGeneratedNever();

            protected override void Seed(ConcurrencyDetectorDbContext context)
                => ConcurrencyDetectorDbContext.Seed(context);
        }

        public static IEnumerable<object[]> IsAsyncData = new[] { new object[] { false }, new object[] { true } };
    }
}
