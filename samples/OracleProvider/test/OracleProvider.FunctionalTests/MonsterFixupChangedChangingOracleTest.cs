// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class MonsterFixupChangedChangingOracleTest :
        MonsterFixupTestBase<MonsterFixupChangedChangingOracleTest.MonsterFixupChangedChangingOracleFixture>
    {
        public MonsterFixupChangedChangingOracleTest(MonsterFixupChangedChangingOracleFixture fixture)
            : base(fixture)
        {
        }

        public class MonsterFixupChangedChangingOracleFixture : MonsterFixupChangedChangingFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => OracleTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(w => w.Log(RelationalEventId.QueryClientEvaluationWarning));

            protected override void OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(
                ModelBuilder builder)
            {
                base.OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(builder);

                builder.Entity<TMessage>().Property(e => e.MessageId).UseOracleIdentityColumn();
                builder.Entity<TProductPhoto>().Property(e => e.PhotoId).UseOracleIdentityColumn();
                builder.Entity<TProductReview>().Property(e => e.ReviewId).UseOracleIdentityColumn();
            }
        }
    }
}
