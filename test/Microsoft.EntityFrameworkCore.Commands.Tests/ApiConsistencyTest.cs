// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Design;

namespace Microsoft.Data.Entity.Commands.Tests
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        protected override Assembly TargetAssembly => typeof(MigrationsOperations).GetTypeInfo().Assembly;

        protected override IEnumerable<Type> GetAllTypes(IEnumerable<Type> types)
        {
            // NOTE: These classes are compiled by the PowerShell module and must not reference external types.
            return base.GetAllTypes(types).Where(
                t => t.FullName != "Microsoft.Data.Entity.Design.IOperationResultHandler"
                     && t.FullName != "Microsoft.Data.Entity.Design.IOperationLogHandler");
        }
    }
}
