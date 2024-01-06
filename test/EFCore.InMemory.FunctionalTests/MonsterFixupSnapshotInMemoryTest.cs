// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class MonsterFixupSnapshotInMemoryTest(MonsterFixupSnapshotInMemoryTest.MonsterFixupSnapshotInMemoryFixture fixture) : MonsterFixupTestBase<
    MonsterFixupSnapshotInMemoryTest.MonsterFixupSnapshotInMemoryFixture>(fixture)
{
    public class MonsterFixupSnapshotInMemoryFixture : MonsterFixupSnapshotFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override void OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(
            ModelBuilder builder)
        {
            base.OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(builder);

            builder.Entity<TMessage>().Property(e => e.MessageId).ValueGeneratedOnAdd();
            builder.Entity<TProductPhoto>().Property(e => e.PhotoId).ValueGeneratedOnAdd();
            builder.Entity<TProductReview>().Property(e => e.ReviewId).ValueGeneratedOnAdd();
        }
    }
}
