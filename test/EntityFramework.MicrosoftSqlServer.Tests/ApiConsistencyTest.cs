// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;

namespace Microsoft.Data.Entity.SqlServer.Tests
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        protected override Assembly TargetAssembly => typeof(SqlServerConnection).Assembly;
    }
}
