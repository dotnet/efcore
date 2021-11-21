// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore;

public abstract class CommandInterceptionSqliteTestBase : CommandInterceptionTestBase
{
    protected CommandInterceptionSqliteTestBase(InterceptionSqliteFixtureBase fixture)
        : base(fixture)
    {
    }

    public override async Task<string> Intercept_query_passively(bool async, bool inject)
    {
        AssertSql(
            @"SELECT ""s"".""Id"", ""s"".""Type"" FROM ""Singularity"" AS ""s""",
            await base.Intercept_query_passively(async, inject));

        return null;
    }

    public override async Task<string> Intercept_query_to_mutate_command(bool async, bool inject)
    {
        AssertSql(
            @"SELECT ""s"".""Id"", ""s"".""Type"" FROM ""Brane"" AS ""s""",
            await base.Intercept_query_to_mutate_command(async, inject));

        return null;
    }

    public override async Task<string> Intercept_query_to_replace_execution(bool async, bool inject)
    {
        AssertSql(
            @"SELECT ""s"".""Id"", ""s"".""Type"" FROM ""Singularity"" AS ""s""",
            await base.Intercept_query_to_replace_execution(async, inject));

        return null;
    }

    public abstract class InterceptionSqliteFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName
            => "CommandInterception";

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlite(), injectedInterceptors);
    }

    public class CommandInterceptionSqliteTest
        : CommandInterceptionSqliteTestBase, IClassFixture<CommandInterceptionSqliteTest.InterceptionSqliteFixture>
    {
        public CommandInterceptionSqliteTest(InterceptionSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionSqliteFixture : InterceptionSqliteFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class CommandInterceptionWithDiagnosticsSqliteTest
        : CommandInterceptionSqliteTestBase, IClassFixture<CommandInterceptionWithDiagnosticsSqliteTest.InterceptionSqliteFixture>
    {
        public CommandInterceptionWithDiagnosticsSqliteTest(InterceptionSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionSqliteFixture : InterceptionSqliteFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
