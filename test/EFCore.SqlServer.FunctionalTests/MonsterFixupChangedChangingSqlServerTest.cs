// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class MonsterFixupChangedChangingSqlServerTest : 
        MonsterFixupTestBase<MonsterFixupChangedChangingSqlServerTest.MonsterFixupChangedChangingSqlServerFixture>
    {
        public MonsterFixupChangedChangingSqlServerTest(MonsterFixupChangedChangingSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class MonsterFixupChangedChangingSqlServerFixture : MonsterFixupChangedChangingFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(w => w.Log(RelationalEventId.QueryClientEvaluationWarning));

            protected override void OnModelCreating<TMessage, TProductPhoto, TProductReview>(ModelBuilder builder)
            {
                base.OnModelCreating<TMessage, TProductPhoto, TProductReview>(builder);

                builder.Entity<TMessage>().Property(e => e.MessageId).UseSqlServerIdentityColumn();
                builder.Entity<TProductPhoto>().Property(e => e.PhotoId).UseSqlServerIdentityColumn();
                builder.Entity<TProductReview>().Property(e => e.ReviewId).UseSqlServerIdentityColumn();
            }
        }
    }
}
