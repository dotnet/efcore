// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class GraphUpdatesInMemoryTest : GraphUpdatesInMemoryTestBase<GraphUpdatesInMemoryTest.GraphUpdatesInMemoryFixture>
    {
        public GraphUpdatesInMemoryTest(GraphUpdatesInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class GraphUpdatesInMemoryFixture : GraphUpdatesInMemoryFixtureBase
        {
            public override int IntSentinel => 0;
        }
    }
}
