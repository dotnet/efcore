// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

#nullable enable

namespace Microsoft.EntityFrameworkCore
{
    public class ValueConvertersEndToEndCosmosTest
        : ValueConvertersEndToEndTestBase<ValueConvertersEndToEndCosmosTest.ValueConvertersEndToEndCosmosFixture>
    {
        public ValueConvertersEndToEndCosmosTest(ValueConvertersEndToEndCosmosFixture fixture)
            : base(fixture)
        {
        }

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
}

#nullable restore
