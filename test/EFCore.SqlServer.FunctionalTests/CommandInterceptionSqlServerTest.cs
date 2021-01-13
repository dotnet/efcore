// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class CommandInterceptionSqlServerTestBase : CommandInterceptionTestBase
    {
        protected CommandInterceptionSqlServerTestBase(InterceptionSqlServerFixtureBase fixture)
            : base(fixture)
        {
        }

        public override async Task<string> Intercept_query_passively(bool async, bool inject)
        {
            AssertSql(
                @"SELECT [s].[Id], [s].[Type] FROM [Singularity] AS [s]",
                await base.Intercept_query_passively(async, inject));

            return null;
        }

        public override async Task<string> Intercept_query_to_mutate_command(bool async, bool inject)
        {
            AssertSql(
                @"SELECT [s].[Id], [s].[Type] FROM [Brane] AS [s]",
                await base.Intercept_query_to_mutate_command(async, inject));

            return null;
        }

        public override async Task<string> Intercept_query_to_replace_execution(bool async, bool inject)
        {
            AssertSql(
                @"SELECT [s].[Id], [s].[Type] FROM [Singularity] AS [s]",
                await base.Intercept_query_to_replace_execution(async, inject));

            return null;
        }

        public abstract class InterceptionSqlServerFixtureBase : InterceptionFixtureBase
        {
            protected override string StoreName
                => "CommandInterception";

            protected override ITestStoreFactory TestStoreFactory
                => SqlServerTestStoreFactory.Instance;

            protected override IServiceCollection InjectInterceptors(
                IServiceCollection serviceCollection,
                IEnumerable<IInterceptor> injectedInterceptors)
                => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlServer(), injectedInterceptors);
        }

        public class CommandInterceptionSqlServerTest
            : CommandInterceptionSqlServerTestBase, IClassFixture<CommandInterceptionSqlServerTest.InterceptionSqlServerFixture>
        {
            public CommandInterceptionSqlServerTest(InterceptionSqlServerFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
            {
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

        public class CommandInterceptionWithDiagnosticsSqlServerTest
            : CommandInterceptionSqlServerTestBase,
                IClassFixture<CommandInterceptionWithDiagnosticsSqlServerTest.InterceptionSqlServerFixture>
        {
            public CommandInterceptionWithDiagnosticsSqlServerTest(InterceptionSqlServerFixture fixture)
                : base(fixture)
            {
            }

            public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
            {
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
