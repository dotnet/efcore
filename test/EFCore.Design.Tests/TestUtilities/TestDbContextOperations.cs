// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestDbContextOperations : DbContextOperations
    {
        public TestDbContextOperations(
            IOperationReporter reporter,
            Assembly assembly,
            Assembly startupAssembly,
            string projectDir,
            string rootNamespace,
            string language,
            bool nullable,
            string[] args,
            AppServiceProviderFactory appServicesFactory)
            : base(reporter, assembly, startupAssembly, projectDir, rootNamespace, language, nullable, args, appServicesFactory)
        {
        }
    }
}
