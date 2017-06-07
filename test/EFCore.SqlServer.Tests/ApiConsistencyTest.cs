// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        protected override void AddServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkSqlServer();
        }

        protected override Assembly TargetAssembly => typeof(SqlServerConnection).GetTypeInfo().Assembly;
    }
}
