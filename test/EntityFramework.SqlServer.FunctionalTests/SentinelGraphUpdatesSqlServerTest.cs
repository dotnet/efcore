// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SentinelGraphUpdatesSqlServerTest : GraphUpdatesSqlServerTestBase<SentinelGraphUpdatesSqlServerTest.SentinelGraphUpdatesSqlServerFixture>
    {
        public SentinelGraphUpdatesSqlServerTest(SentinelGraphUpdatesSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class SentinelGraphUpdatesSqlServerFixture : GraphUpdatesSqlServerFixtureBase
        {
            protected override string DatabaseName => "SentinelGraphUpdatesTest";

            public override int IntSentinel => -1;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.ForRelational().Sequence("StartAtZeroSequence").Start(0);
                modelBuilder.ForSqlServer().UseSequence("StartAtZeroSequence");

                SetSentinelValues(modelBuilder);
            }
        }
    }
}
