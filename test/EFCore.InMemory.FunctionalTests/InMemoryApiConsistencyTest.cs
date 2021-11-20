// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.Storage.Internal;
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
            public override HashSet<Type> FluentApiTypes { get; } = new()
            {
                typeof(InMemoryServiceCollectionExtensions),
                typeof(InMemoryDbContextOptionsExtensions),
                typeof(InMemoryDbContextOptionsBuilder)
            };
        }
    }
}
