// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class GraphUpdatesInMemoryTest(GraphUpdatesInMemoryTest.InMemoryFixture fixture)
    : GraphUpdatesInMemoryTestBase<GraphUpdatesInMemoryTest.InMemoryFixture>(fixture)
{
    public class InMemoryFixture : GraphUpdatesInMemoryFixtureBase
    {
        protected override string StoreName
            => "GraphUpdatesTest";
    }
}
