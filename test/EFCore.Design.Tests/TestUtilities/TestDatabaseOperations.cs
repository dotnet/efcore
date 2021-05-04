// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Design.Internal;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestDatabaseOperations : DatabaseOperations
    {
        public TestDatabaseOperations(
            IOperationReporter reporter,
            Assembly assembly,
            Assembly startupAssembly,
            string projectDir,
            string rootNamespace,
            string language,
            string[] args)
            : base(reporter, assembly, startupAssembly, projectDir, rootNamespace, language, args)
        {
        }
    }
}
