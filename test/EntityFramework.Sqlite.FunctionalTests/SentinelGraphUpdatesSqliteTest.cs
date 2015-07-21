// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class SentinelGraphUpdatesSqliteTest : GraphUpdatesSqliteTestBase<SentinelGraphUpdatesSqliteTest.SentinelGraphUpdatesSqliteFixture>
    {
        public SentinelGraphUpdatesSqliteTest(SentinelGraphUpdatesSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class SentinelGraphUpdatesSqliteFixture : GraphUpdatesSqliteFixtureBase
        {
            protected override string DatabaseName => "SentinelGraphUpdatesTest";

            public override int IntSentinel => -10000000;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                SetSentinelValues(modelBuilder, IntSentinel);
            }
        }
    }
}
