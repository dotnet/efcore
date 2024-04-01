// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable enable

public class ValueConvertersEndToEndCosmosTest(ValueConvertersEndToEndCosmosTest.ValueConvertersEndToEndCosmosFixture fixture)
    : ValueConvertersEndToEndTestBase<ValueConvertersEndToEndCosmosTest.ValueConvertersEndToEndCosmosFixture>(fixture)
{
    public class ValueConvertersEndToEndCosmosFixture : ValueConvertersEndToEndFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<ConvertingEntity>(
                b =>
                {
                    // Issue #24684
                    b.Ignore(e => e.StringToDateTimeOffset);
                    b.Ignore(e => e.NullableStringToDateTimeOffset);
                    b.Ignore(e => e.StringToNullableDateTimeOffset);
                    b.Ignore(e => e.NullableStringToNullableDateTimeOffset);
                });
        }
    }
}
