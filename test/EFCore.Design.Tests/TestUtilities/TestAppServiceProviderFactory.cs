// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestAppServiceProviderFactory : AppServiceProviderFactory
    {
        private readonly Type _programType;

        public TestAppServiceProviderFactory(Assembly startupAssembly, Type programType, IOperationReporter reporter = null)
            : base(startupAssembly, reporter ?? new TestOperationReporter())
            => _programType = programType;

        protected override Type FindProgramClass()
            => _programType;
    }
}
