// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        private static readonly Type[] _fluentApiTypes =
        {
            typeof(SqlServerDbContextOptionsBuilder),
            typeof(SqlServerDbContextOptionsExtensions),
            typeof(SqlServerIndexBuilderExtensions),
            typeof(SqlServerKeyBuilderExtensions),
            typeof(SqlServerMetadataExtensions),
            typeof(SqlServerModelBuilderExtensions),
            typeof(SqlServerPropertyBuilderExtensions),
            typeof(SqlServerOwnedNavigationBuilderExtensions),
            typeof(SqlServerServiceCollectionExtensions),
            typeof(SqlServerEntityTypeBuilderExtensions)
        };

        protected override IEnumerable<Type> FluentApiTypes => _fluentApiTypes;

        protected override void AddServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkSqlServer();
        }

        protected override Assembly TargetAssembly => typeof(SqlServerConnection).GetTypeInfo().Assembly;
    }
}
