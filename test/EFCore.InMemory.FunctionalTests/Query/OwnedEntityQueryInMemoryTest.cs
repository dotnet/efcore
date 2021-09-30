// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class OwnedEntityQueryInMemoryTest : OwnedEntityQueryTestBase
    {
        protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public virtual async Task Expand_owned_navigation_as_optional_always(bool async)
        {
            var contextFactory = await InitializeAsync<MyContext>(seed: c => c.Seed());

            using var context = contextFactory.CreateContext();
            var query = context.Set<Foo>().Include(c => c.Bar);
            var foo = async
                ? await query.FirstOrDefaultAsync()
                : query.FirstOrDefault();

            Assert.NotNull(foo);
        }

        protected class MyContext : DbContext
        {
            public MyContext(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Warehouse> Warehouses { get; set; }

            public void Seed()
            {
                Add(new Foo());

                SaveChanges();
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Bar>().OwnsOne(t => t.Baz, e => { });
                modelBuilder.Entity<Foo>().HasOne(t => t.Bar)
                    .WithOne(t => t.Foo)
                    .HasForeignKey<Bar>(t => t.FooId);
            }
        }

#nullable enable
        protected class Bar
        {
            public long Id { get; set; }

            public long FooId { get; set; }
            public virtual Foo Foo { get; set; } = null!;

            public virtual Baz Baz { get; set; } = new();
        }

        protected class Baz
        { }

        protected class Foo
        {
            public long Id { get; set; }
            public virtual Bar? Bar { get; set; }
        }
#nullable disable
    }
}
