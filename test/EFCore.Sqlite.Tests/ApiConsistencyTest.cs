// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        private static readonly Type[] _fluentApiTypes =
        {
            typeof(SqliteServiceCollectionExtensions),
            typeof(SqliteDbContextOptionsBuilderExtensions),
            typeof(SqliteDbContextOptionsBuilder),
            typeof(SqlitePropertyBuilderExtensions)
        };

        protected override IEnumerable<Type> FluentApiTypes => _fluentApiTypes;

        protected override void AddServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkSqlite();
        }

        protected override Assembly TargetAssembly => typeof(SqliteRelationalConnection).GetTypeInfo().Assembly;
    }
}
