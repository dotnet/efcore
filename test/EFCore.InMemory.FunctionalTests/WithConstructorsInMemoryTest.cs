// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
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
            protected override ITestStoreFactory TestStoreFactory => InMemoryTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(w => w.Log(InMemoryEventId.TransactionIgnoredWarning));

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder
                    .Query<BlogQuery>()
                    .ToQuery(() => context.Set<Blog>().Select(b => new BlogQuery(b.Title, b.MonthlyRevenue)));
            }
        }
    }
}
