// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Metadata;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerSentinelGraphUpdatesTest : SqlServerGraphUpdatesTestBase<SqlServerSentinelGraphUpdatesTest.SqlServerSentinelGraphUpdatesFixture>
    {
        public SqlServerSentinelGraphUpdatesTest(SqlServerSentinelGraphUpdatesFixture fixture)
            : base(fixture)
        {
        }

        public class SqlServerSentinelGraphUpdatesFixture : SqlServerGraphUpdatesFixtureBase
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
