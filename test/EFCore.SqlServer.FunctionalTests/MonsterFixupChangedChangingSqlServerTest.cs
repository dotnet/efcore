// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class MonsterFixupChangedChangingSqlServerTest(MonsterFixupChangedChangingSqlServerTest.MonsterFixupChangedChangingSqlServerFixture fixture) :
    MonsterFixupTestBase<MonsterFixupChangedChangingSqlServerTest.MonsterFixupChangedChangingSqlServerFixture>(fixture)
{
    public class MonsterFixupChangedChangingSqlServerFixture : MonsterFixupChangedChangingFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override void OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(
            ModelBuilder builder)
        {
            base.OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(builder);

            builder.Entity<TMessage>().Property(e => e.MessageId).UseIdentityColumn();

            builder.Entity<TProduct>()
                .OwnsOne(
                    c => (TDimensions)c.Dimensions, db =>
                    {
                        db.Property(d => d.Depth).HasColumnType("decimal(18,2)");
                        db.Property(d => d.Width).HasColumnType("decimal(18,2)");
                        db.Property(d => d.Height).HasColumnType("decimal(18,2)");
                    });

            builder.Entity<TProductPhoto>().Property(e => e.PhotoId).UseIdentityColumn();
            builder.Entity<TProductReview>().Property(e => e.ReviewId).UseIdentityColumn();

            builder.Entity<TComputerDetail>()
                .OwnsOne(
                    c => (TDimensions)c.Dimensions, db =>
                    {
                        db.Property(d => d.Depth).HasColumnType("decimal(18,2)");
                        db.Property(d => d.Width).HasColumnType("decimal(18,2)");
                        db.Property(d => d.Height).HasColumnType("decimal(18,2)");
                    });
        }
    }
}
