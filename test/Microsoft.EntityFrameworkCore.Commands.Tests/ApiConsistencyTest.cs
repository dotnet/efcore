// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Tests;

namespace Microsoft.EntityFrameworkCore.Commands.Tests
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        // NOTE: These classes are compiled by the PowerShell module and must not reference external types.
        private static readonly IEnumerable<Type> _ignoredTypes = new[]
        {
            typeof(IOperationLogHandler),
            typeof(IOperationResultHandler),
            typeof(OperationLogHandler),
            typeof(OperationResultHandler)
        };

        protected override Assembly TargetAssembly => typeof(MigrationsOperations).GetTypeInfo().Assembly;

        protected override IEnumerable<Type> GetAllTypes(IEnumerable<Type> types)
            => base.GetAllTypes(types).Except(_ignoredTypes);
    }
}
