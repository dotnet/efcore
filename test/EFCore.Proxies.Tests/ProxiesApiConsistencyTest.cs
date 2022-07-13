// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class ProxiesApiConsistencyTest : ApiConsistencyTestBase<ProxiesApiConsistencyTest.ProxiesApiConsistencyFixture>
{
    public ProxiesApiConsistencyTest(ProxiesApiConsistencyFixture fixture)
        : base(fixture)
    {
    }

    protected override void AddServices(ServiceCollection serviceCollection)
        => serviceCollection.AddEntityFrameworkProxies();

    protected override Assembly TargetAssembly
        => typeof(ProxiesExtensions).Assembly;

    public class ProxiesApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } = new() { typeof(ProxiesServiceCollectionExtensions) };
    }
}
