// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore
{
    public class InMemoryApiConsistencyTest : ApiConsistencyTestBase<InMemoryApiConsistencyTest.InMemoryApiConsistencyFixture>
    {
        public InMemoryApiConsistencyTest(InMemoryApiConsistencyFixture fixture)
            : base(fixture)
        {
        }

        protected override void AddServices(ServiceCollection serviceCollection)
            => serviceCollection.AddEntityFrameworkInMemoryDatabase();

        protected override Assembly TargetAssembly
            => typeof(InMemoryDatabase).Assembly;

        public class InMemoryApiConsistencyFixture : ApiConsistencyFixtureBase
        {
            public override bool TryGetProviderOptionsDelegate(out Action<DbContextOptionsBuilder> configureOptions)
            {
                configureOptions = b => InMemoryTestHelpers.Instance.UseProviderOptions(b);

                return true;
            }

            public override HashSet<Type> FluentApiTypes { get; } = new HashSet<Type>
            {
                typeof(InMemoryServiceCollectionExtensions),
                typeof(InMemoryDbContextOptionsExtensions),
                typeof(InMemoryDbContextOptionsBuilder)
            };
        }
    }
}
