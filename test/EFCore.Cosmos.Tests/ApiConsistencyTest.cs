// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Cosmos
{
    public class ApiConsistencyTest : ApiConsistencyTestBase
    {
        private static readonly Type[] _fluentApiTypes =
        {
            typeof(CosmosServiceCollectionExtensions), typeof(CosmosDbContextOptionsExtensions), typeof(CosmosDbContextOptionsBuilder)
        };

        protected override IEnumerable<Type> FluentApiTypes => _fluentApiTypes;

        protected override void AddServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkCosmos();
        }

        protected override Assembly TargetAssembly => typeof(CosmosDatabaseWrapper).GetTypeInfo().Assembly;
    }
}
