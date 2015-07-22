// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class SentinelGraphUpdatesInMemoryTest : GraphUpdatesInMemoryTestBase<SentinelGraphUpdatesInMemoryTest.SentinelGraphUpdatesInMemoryFixture>
    {
        public SentinelGraphUpdatesInMemoryTest(SentinelGraphUpdatesInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class SentinelGraphUpdatesInMemoryFixture : GraphUpdatesInMemoryFixtureBase
        {
            public override int IntSentinel => -1;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                SetSentinelValues(modelBuilder, IntSentinel);
            }
        }
    }
}
