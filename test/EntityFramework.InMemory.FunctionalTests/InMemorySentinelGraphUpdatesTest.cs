// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class InMemorySentinelGraphUpdatesTest : InMemoryGraphUpdatesTestBase<InMemorySentinelGraphUpdatesTest.InMemorySentinelGraphUpdatesFixture>
    {
        public InMemorySentinelGraphUpdatesTest(InMemorySentinelGraphUpdatesFixture fixture)
            : base(fixture)
        {
        }

        public class InMemorySentinelGraphUpdatesFixture : InMemoryGraphUpdatesFixtureBase
        {
            public override int IntSentinel => -1;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                SetSentinelValues(modelBuilder);
            }
        }
    }
}
