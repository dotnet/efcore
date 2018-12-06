// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class NTSApiConsistencyTest : ApiConsistencyTestBase
    {
        private static readonly Type[] _fluentApiTypes =
        {
            typeof(SqlServerNetTopologySuiteDbContextOptionsBuilderExtensions),
            typeof(SqlServerNetTopologySuiteServiceCollectionExtensions)
        };

        protected override IEnumerable<Type> FluentApiTypes => _fluentApiTypes;

        protected override void AddServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkSqlServerNetTopologySuite();
        }

        protected override Assembly TargetAssembly
            => typeof(SqlServerNetTopologySuiteServiceCollectionExtensions).GetTypeInfo().Assembly;
    }
}
