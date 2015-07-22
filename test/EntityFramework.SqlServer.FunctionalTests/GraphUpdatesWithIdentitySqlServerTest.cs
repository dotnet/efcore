// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class GraphUpdatesWithIdentitySqlServerTest : GraphUpdatesSqlServerTestBase<GraphUpdatesWithIdentitySqlServerTest.GraphUpdatesWithIdentitySqlServerFixture>
    {
        public GraphUpdatesWithIdentitySqlServerTest(GraphUpdatesWithIdentitySqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class GraphUpdatesWithIdentitySqlServerFixture : GraphUpdatesSqlServerFixtureBase
        {
            protected override string DatabaseName => "GraphIdentityUpdatesTest";

            public override int IntSentinel => 0;

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.UseSqlServerIdentityColumns(); // ensure model uses identity

                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
