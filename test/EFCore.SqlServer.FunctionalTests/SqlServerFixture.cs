// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore
{
    public class SqlServerFixture : ServiceProviderFixtureBase
    {
        public static IServiceProvider DefaultServiceProvider { get; }
            = new ServiceCollection().AddEntityFrameworkSqlServer().BuildServiceProvider();

        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();
        protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                w =>
                {
                    w.Log(SqlServerEventId.ByteIdentityColumnWarning);
                });
    }
}
