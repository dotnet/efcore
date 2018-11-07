// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
#if !Test21
    public class NTSApiConsistencyTest : ApiConsistencyTestBase
    {
        private static readonly Type[] _fluentApiTypes =
        {
            typeof(SqliteNetTopologySuiteDbContextOptionsBuilderExtensions),
            typeof(SqliteNetTopologySuitePropertyBuilderExtensions),
            typeof(SqliteNetTopologySuiteServiceCollectionExtensions)
        };

        protected override IEnumerable<Type> FluentApiTypes => _fluentApiTypes;

        protected override void AddServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkSqliteNetTopologySuite();
        }

        protected override Assembly TargetAssembly
            => typeof(SqliteNetTopologySuiteServiceCollectionExtensions).GetTypeInfo().Assembly;
    }
#endif
}
