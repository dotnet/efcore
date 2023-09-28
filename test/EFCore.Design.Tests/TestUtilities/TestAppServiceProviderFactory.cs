// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestAppServiceProviderFactory : AppServiceProviderFactory
{
    public TestAppServiceProviderFactory(Assembly startupAssembly, IOperationReporter reporter = null)
        : base(startupAssembly, reporter ?? new TestOperationReporter())
    {
    }
}
