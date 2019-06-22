// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ConnectionInterceptionSqliteTest
        : ConnectionInterceptionTestBase, IClassFixture<ConnectionInterceptionSqliteTest.InterceptionSqliteFixture>
    {
        public ConnectionInterceptionSqliteTest(InterceptionSqliteFixture fixture)
            : base(fixture)
        {
        }

        protected override BadUniverseContext CreateBadUniverse(DbContextOptionsBuilder optionsBuilder)
            => new BadUniverseContext(optionsBuilder.UseSqlite("Data Source=file:data.db?mode=invalidmode").Options);

        public class InterceptionSqliteFixture : InterceptionFixtureBase
        {
            protected override string StoreName => "ConnectionInterception";

            protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlite(), injectedInterceptors);
        }
    }
}
