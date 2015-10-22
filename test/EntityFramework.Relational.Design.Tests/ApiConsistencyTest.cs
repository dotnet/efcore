// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Entity.Scaffolding;

namespace Microsoft.Data.Entity.Relational.Design
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        protected override Assembly TargetAssembly 
            => typeof(IScaffoldingModelFactory).Assembly;
    }
}
