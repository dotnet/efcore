// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;

namespace Microsoft.EntityFrameworkCore
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

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.ForSqlServerUseIdentityColumns(); // ensure model uses identity

                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
