// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class MonsterFixupChangedOnlySqliteTest(MonsterFixupChangedOnlySqliteTest.MonsterFixupChangedOnlySqliteFixture fixture) : MonsterFixupTestBase<
    MonsterFixupChangedOnlySqliteTest.MonsterFixupChangedOnlySqliteFixture>(fixture)
{
    public class MonsterFixupChangedOnlySqliteFixture : MonsterFixupChangedOnlyFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override void OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(
            ModelBuilder builder)
        {
            base.OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(builder);

            builder.Entity<TMessage>().HasKey(e => e.MessageId);
            builder.Entity<TProductPhoto>().HasKey(e => e.PhotoId);
            builder.Entity<TProductReview>().HasKey(e => e.ReviewId);
        }
    }
}
