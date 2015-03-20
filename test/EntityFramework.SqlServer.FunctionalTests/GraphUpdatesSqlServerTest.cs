// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class GraphUpdatesSqlServerTest : GraphUpdatesSqlServerTestBase<GraphUpdatesSqlServerTest.GraphUpdatesSqlServerFixture>
    {
        public GraphUpdatesSqlServerTest(GraphUpdatesSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class GraphUpdatesSqlServerFixture : GraphUpdatesSqlServerFixtureBase
        {
            protected override string DatabaseName => "GraphUpdatesTest";

            public override int IntSentinel => 0;
        }
    }
}
