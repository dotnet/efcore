// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

public class InMemoryApiConsistencyTest(InMemoryApiConsistencyTest.InMemoryApiConsistencyFixture fixture) : ApiConsistencyTestBase<InMemoryApiConsistencyTest.InMemoryApiConsistencyFixture>(fixture)
{
    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkInMemoryDatabase();

    protected override Assembly TargetAssembly
        => typeof(InMemoryDatabase).Assembly;

    public class InMemoryApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } =
        [
            typeof(InMemoryServiceCollectionExtensions),
            typeof(InMemoryDbContextOptionsExtensions),
            typeof(InMemoryDbContextOptionsBuilder)
        ];
    }
}
