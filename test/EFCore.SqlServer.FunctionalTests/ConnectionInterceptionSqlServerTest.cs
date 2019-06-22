// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class ConnectionInterceptionSqlServerTest
        : ConnectionInterceptionTestBase, IClassFixture<ConnectionInterceptionSqlServerTest.InterceptionSqlServerFixture>
    {
        public ConnectionInterceptionSqlServerTest(InterceptionSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionSqlServerFixture : InterceptionFixtureBase
        {
            protected override string StoreName => "ConnectionInterception";

            protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlServer(), injectedInterceptors);
        }

        protected override BadUniverseContext CreateBadUniverse(DbContextOptionsBuilder optionsBuilder)
            => new BadUniverseContext(optionsBuilder.UseSqlServer("Database = IIzBad").Options);
    }
}
