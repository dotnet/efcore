// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Scaffolding.Internal;

namespace Microsoft.EntityFrameworkCore;

public class DesignApiConsistencyTest(DesignApiConsistencyTest.DesignApiConsistencyFixture fixture) : ApiConsistencyTestBase<DesignApiConsistencyTest.DesignApiConsistencyFixture>(fixture)
{
    protected override void AddServices(ServiceCollection serviceCollection)
    {
    }

    protected override Assembly TargetAssembly
        => typeof(OperationExecutor).Assembly;

    public class DesignApiConsistencyFixture : ApiConsistencyFixtureBase
    {
        public override HashSet<Type> FluentApiTypes { get; } = [typeof(DesignTimeServiceCollectionExtensions)];

        public override HashSet<MethodInfo> NonVirtualMethods { get; } =
        [
            typeof(CSharpEntityTypeGeneratorBase.ToStringInstanceHelper)
                .GetProperty(nameof(CSharpEntityTypeGeneratorBase.ToStringInstanceHelper.FormatProvider)).GetMethod,
            typeof(CSharpEntityTypeGeneratorBase.ToStringInstanceHelper)
                .GetProperty(nameof(CSharpEntityTypeGeneratorBase.ToStringInstanceHelper.FormatProvider)).SetMethod,
            typeof(CSharpEntityTypeGeneratorBase.ToStringInstanceHelper).GetMethod(
                nameof(CSharpEntityTypeGeneratorBase.ToStringInstanceHelper.ToStringWithCulture)),
            typeof(CSharpDbContextGeneratorBase.ToStringInstanceHelper)
                .GetProperty(nameof(CSharpDbContextGeneratorBase.ToStringInstanceHelper.FormatProvider)).GetMethod,
            typeof(CSharpDbContextGeneratorBase.ToStringInstanceHelper)
                .GetProperty(nameof(CSharpDbContextGeneratorBase.ToStringInstanceHelper.FormatProvider)).SetMethod,
            typeof(CSharpDbContextGeneratorBase.ToStringInstanceHelper).GetMethod(
                nameof(CSharpDbContextGeneratorBase.ToStringInstanceHelper.ToStringWithCulture))
        ];
    }
}
