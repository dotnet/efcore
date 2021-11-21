// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Query;

public class RawSqlServerTest : NonSharedModelTestBase
{
    public RawSqlServerTest(ITestOutputHelper testOutputHelper)
    {
        //TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    // Issue #13346, #24623
    [ConditionalFact]
    public virtual async Task ToQuery_can_use_FromSqlRaw()
    {
        var contextFactory = await InitializeAsync<MyContext13346>(seed: c => c.Seed());

        using (var context = contextFactory.CreateContext())
        {
            var query = context.Set<MyContext13346.OrderSummary13346>().ToList();

            Assert.Equal(4, query.Count);

            AssertSql("SELECT o.Amount From Orders AS o -- RAW");
        }
    }

    protected class MyContext13346 : DbContext
    {
        public virtual DbSet<Order13346> Orders { get; set; }

        public MyContext13346(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            modelBuilder.Entity<OrderSummary13346>()
                .HasNoKey()
                .ToQuery(() => Set<OrderSummary13346>().FromSqlRaw("SELECT o.Amount From Orders AS o -- RAW"));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public void Seed()
        {
            AddRange(
                new Order13346 { Amount = 1 },
                new Order13346 { Amount = 2 },
                new Order13346 { Amount = 3 },
                new Order13346 { Amount = 4 }
            );

            SaveChanges();
        }

        public class Order13346
        {
            public int Id { get; set; }
            public int Amount { get; set; }
        }

        public class OrderSummary13346
        {
            public int Amount { get; set; }
        }
    }

    protected override string StoreName { get; } = "RawSqlServerTest";

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected override ITestStoreFactory TestStoreFactory
        => SqlServerTestStoreFactory.Instance;

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();
}
