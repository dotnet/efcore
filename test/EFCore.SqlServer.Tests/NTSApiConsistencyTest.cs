// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
#if !Test21
    public class NTSApiConsistencyTest : ApiConsistencyTestBase
    {
        protected override void AddServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkSqlServerNetTopologySuite();
        }

        protected override Assembly TargetAssembly
            => typeof(SqlServerNTSServiceCollectionExtensions).GetTypeInfo().Assembly;
    }
#endif
}
