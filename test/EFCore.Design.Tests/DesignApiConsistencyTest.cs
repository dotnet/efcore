// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class DesignApiConsistencyTest : ApiConsistencyTestBase<DesignApiConsistencyTest.DesignApiConsistencyFixture>
{
    public DesignApiConsistencyTest(DesignApiConsistencyFixture fixture)
        : base(fixture)
    {
    }

    protected override void AddServices(ServiceCollection serviceCollection)
    {
    }

    protected override Assembly TargetAssembly
        => typeof(OperationExecutor).Assembly;

    public class DesignApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } = new() { typeof(DesignTimeServiceCollectionExtensions) };
    }
}
