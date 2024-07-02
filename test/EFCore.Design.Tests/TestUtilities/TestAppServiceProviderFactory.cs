// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestAppServiceProviderFactory : AppServiceProviderFactory
{
    private readonly bool _throwOnCreate;

    public TestAppServiceProviderFactory(Assembly startupAssembly, bool throwOnCreate = false)
        : this(startupAssembly, new TestOperationReporter(), throwOnCreate)
    {
    }

    public TestAppServiceProviderFactory(Assembly startupAssembly, TestOperationReporter reporter, bool throwOnCreate = false)
        : base(startupAssembly, reporter)
    {
        TestOperationReporter = reporter;
        _throwOnCreate = throwOnCreate;
    }

    public TestOperationReporter TestOperationReporter { get; }

    public override IServiceProvider Create(string[] args)
    {
        Assert.False(_throwOnCreate, "Service provider shouldn't be used in this case.");

        return base.Create(args);
    }
}
