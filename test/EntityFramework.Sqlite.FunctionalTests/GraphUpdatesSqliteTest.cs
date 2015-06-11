// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Sqlite.FunctionalTests
{
    public class GraphUpdatesSqliteTest : GraphUpdatesSqliteTestBase<GraphUpdatesSqliteTest.GraphUpdatesSqliteFixture>
    {
        public GraphUpdatesSqliteTest(GraphUpdatesSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class GraphUpdatesSqliteFixture : GraphUpdatesSqliteFixtureBase
        {
            protected override string DatabaseName => "GraphUpdatesTest";

            public override int IntSentinel => 0;
        }
    }
}
