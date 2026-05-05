// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class WithConstructorsInMemoryTest(WithConstructorsInMemoryTest.WithConstructorsInMemoryFixture fixture)
    : WithConstructorsTestBase<WithConstructorsInMemoryTest.WithConstructorsInMemoryFixture>(fixture)
{
    public override async Task Query_and_update_using_constructors_with_property_parameters()
    {
        await base.Query_and_update_using_constructors_with_property_parameters();

        await Fixture.ReseedAsync();
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
