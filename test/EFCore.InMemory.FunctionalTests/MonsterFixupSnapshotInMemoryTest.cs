// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class MonsterFixupSnapshotInMemoryTest : MonsterFixupTestBase<
        MonsterFixupSnapshotInMemoryTest.MonsterFixupSnapshotInMemoryFixture>
    {
        public MonsterFixupSnapshotInMemoryTest(MonsterFixupSnapshotInMemoryFixture fixture)
            : base(fixture)
        {
        }

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
}
