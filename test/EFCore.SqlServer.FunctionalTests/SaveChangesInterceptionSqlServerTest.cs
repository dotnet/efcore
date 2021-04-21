// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class SaveChangesInterceptionSqlServerTestBase : SaveChangesInterceptionTestBase
    {
        protected SaveChangesInterceptionSqlServerTestBase(InterceptionSqlServerFixtureBase fixture)
            : base(fixture)
        {
        }

        public abstract class InterceptionSqlServerFixtureBase : InterceptionFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlServer(), injectedInterceptors);
        }

        public class SaveChangesInterceptionSqlServerTest
            : SaveChangesInterceptionSqlServerTestBase, IClassFixture<SaveChangesInterceptionSqlServerTest.InterceptionSqlServerFixture>
        {
            public SaveChangesInterceptionSqlServerTest(InterceptionSqlServerFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
            {
                protected override string StoreName
                    => "SaveChangesInterception";

                protected override bool ShouldSubscribeToDiagnosticListener
                    => false;

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                {
                    new SqlServerDbContextOptionsBuilder(base.AddOptions(builder))
                        .ExecutionStrategy(d => new SqlServerExecutionStrategy(d));
                    return builder;
                }
            }
        }

        public class SaveChangesInterceptionWithDiagnosticsSqlServerTest
            : SaveChangesInterceptionSqlServerTestBase,
                IClassFixture<SaveChangesInterceptionWithDiagnosticsSqlServerTest.InterceptionSqlServerFixture>
        {
            public SaveChangesInterceptionWithDiagnosticsSqlServerTest(InterceptionSqlServerFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
            {
                protected override string StoreName
                    => "SaveChangesInterceptionWithDiagnostics";

                protected override bool ShouldSubscribeToDiagnosticListener
                    => true;

                public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                {
                    new SqlServerDbContextOptionsBuilder(base.AddOptions(builder))
                        .ExecutionStrategy(d => new SqlServerExecutionStrategy(d));
                    return builder;
                }
            }
        }
    }
}
