// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    [SqlServerCondition(SqlServerCondition.SupportsSequences)]
    public class GraphUpdatesSqlServerTestSequence : GraphUpdatesSqlServerTestBase<
        GraphUpdatesSqlServerTestSequence.GraphUpdatesWithSequenceSqlServerFixture>
    {
        public GraphUpdatesSqlServerTestSequence(GraphUpdatesWithSequenceSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class GraphUpdatesWithSequenceSqlServerFixture : GraphUpdatesSqlServerFixtureBase
        {
            protected override string StoreName { get; } = "GraphSequenceUpdatesTest";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.UseHiLo(); // ensure model uses sequences
                base.OnModelCreating(modelBuilder, context);
            }
        }
    }
}
