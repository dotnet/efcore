// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

#nullable disable

[SqlServerCondition(SqlServerCondition.SupportsMemoryOptimized)]
public class MemoryOptimizedTablesTest(MemoryOptimizedTablesTest.MemoryOptimizedTablesFixture fixture)
    : IClassFixture<MemoryOptimizedTablesTest.MemoryOptimizedTablesFixture>
{
    protected MemoryOptimizedTablesFixture Fixture { get; } = fixture;

    [ConditionalFact]
    public async Task Can_create_memoryOptimized_table()
    {
        using (await CreateTestStoreAsync())
        {
            var bigUn = new BigUn();
            var fastUns = new[] { new FastUn { Name = "First 'un", BigUn = bigUn }, new FastUn { Name = "Second 'un", BigUn = bigUn } };
            using (var context = CreateContext())
            {
                await context.Database.EnsureCreatedResilientlyAsync();

                // ReSharper disable once CoVariantArrayConversion
                context.AddRange(fastUns);

                await context.SaveChangesAsync();
            }

            using (var context = CreateContext())
            {
                Assert.Equal(fastUns.Select(f => f.Name), await context.FastUns.OrderBy(f => f.Name).Select(f => f.Name).ToListAsync());
            }
        }
    }

    protected TestStore TestStore { get; set; }

    protected Task<TestStore> CreateTestStoreAsync()
    {
        TestStore = SqlServerTestStore.Create(nameof(MemoryOptimizedTablesTest));
        return TestStore.InitializeAsync(null, CreateContext, _ => Task.CompletedTask);
    }

    private MemoryOptimizedContext CreateContext()
        => new(Fixture.CreateOptions(TestStore));

    public class MemoryOptimizedTablesFixture : ServiceProviderFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;
    }

    private class MemoryOptimizedContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<FastUn> FastUns { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .Entity<FastUn>(
                    eb =>
                    {
                        eb.ToTable(tb => tb.IsMemoryOptimized());
                        eb.HasIndex(e => e.Name).IsUnique();
                        eb.HasOne(e => e.BigUn).WithMany(e => e.FastUns).IsRequired().OnDelete(DeleteBehavior.Restrict);
                    });

            modelBuilder.Entity<BigUn>().ToTable(tb => tb.IsMemoryOptimized());
        }
    }

    private class BigUn
    {
        public int Id { get; set; }
        public ICollection<FastUn> FastUns { get; set; }
    }

    private class FastUn
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BigUn BigUn { get; set; }
    }
}
