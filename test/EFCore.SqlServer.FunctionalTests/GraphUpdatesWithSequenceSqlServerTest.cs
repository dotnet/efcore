// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore
{
    [SqlServerCondition(SqlServerCondition.SupportsSequences)]
    public class GraphUpdatesWithSequenceSqlServerTest : GraphUpdatesSqlServerTestBase<GraphUpdatesWithSequenceSqlServerTest.GraphUpdatesWithSequenceSqlServerFixture>
    {
        public GraphUpdatesWithSequenceSqlServerTest(GraphUpdatesWithSequenceSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class GraphUpdatesWithSequenceSqlServerFixture : GraphUpdatesSqlServerFixtureBase
        {
            protected override string DatabaseName => "GraphSequenceUpdatesTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ForSqlServerUseSequenceHiLo(); // ensure model uses sequences
                base.OnModelCreating(modelBuilder);
            }
        }
    }
}