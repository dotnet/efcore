// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class WithConstructorsInMemoryTest : WithConstructorsTestBase<WithConstructorsInMemoryTest.WithConstructorsInMemoryFixture>
{
    public WithConstructorsInMemoryTest(WithConstructorsInMemoryFixture fixture)
        : base(fixture)
    {
    }

    public override void Query_and_update_using_constructors_with_property_parameters()
    {
        base.Query_and_update_using_constructors_with_property_parameters();

        Fixture.Reseed();
    }

    public class WithConstructorsInMemoryFixture : WithConstructorsFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(w => w.Log(InMemoryEventId.TransactionIgnoredWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<BlogQuery>().HasNoKey().ToInMemoryQuery(
                () => context.Set<Blog>().Select(b => new BlogQuery(b.Title, b.MonthlyRevenue)));
        }
    }
}
