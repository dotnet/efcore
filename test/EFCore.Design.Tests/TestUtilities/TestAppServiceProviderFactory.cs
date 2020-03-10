// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestAppServiceProviderFactory : AppServiceProviderFactory
    {
        public TestAppServiceProviderFactory(Assembly startupAssembly, IOperationReporter reporter = null)
            : base(startupAssembly, reporter ?? new TestOperationReporter())
        {
        }
    }
}
