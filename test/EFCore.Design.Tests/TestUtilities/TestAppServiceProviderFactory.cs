// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestAppServiceProviderFactory(Assembly startupAssembly, TestOperationReporter reporter, bool throwOnCreate = false)
    : AppServiceProviderFactory(startupAssembly, reporter)
{
    public TestAppServiceProviderFactory(Assembly startupAssembly, bool throwOnCreate = false)
        : this(startupAssembly, new TestOperationReporter(), throwOnCreate)
    {
    }

    public TestOperationReporter TestOperationReporter { get; } = reporter;

    public override IServiceProvider Create(string[] args)
    {
        Assert.False(throwOnCreate, "Service provider shouldn't be used in this case.");

        return base.Create(args);
    }
}
