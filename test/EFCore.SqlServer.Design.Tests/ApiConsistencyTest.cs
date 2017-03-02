// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Tests;

namespace Microsoft.EntityFrameworkCore.SqlServer.Design.Tests
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        protected override Assembly TargetAssembly => typeof(SqlServerScaffoldingModelFactory).GetTypeInfo().Assembly;
    }
}
