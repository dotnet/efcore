// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable enable

public abstract class ConnectionInterceptionSqliteTestBase : ConnectionInterceptionTestBase
{
    protected ConnectionInterceptionSqliteTestBase(InterceptionSqliteFixtureBase fixture)
        : base(fixture)
    {
    }

    protected override DbContextOptionsBuilder ConfigureProvider(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlite();

    protected override BadUniverseContext CreateBadUniverse(DbContextOptionsBuilder optionsBuilder)
        => new(optionsBuilder.UseSqlite("Data Source=file:data.db?mode=invalidmode").Options);

    public abstract class InterceptionSqliteFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName
            => "ConnectionInterception";

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlite(), injectedInterceptors);
    }

    public class ConnectionInterceptionSqliteTest
        : ConnectionInterceptionSqliteTestBase, IClassFixture<ConnectionInterceptionSqliteTest.InterceptionSqliteFixture>
    {
        public ConnectionInterceptionSqliteTest(InterceptionSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionSqliteFixture : InterceptionSqliteFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class ConnectionInterceptionWithDiagnosticsSqliteTest
        : ConnectionInterceptionSqliteTestBase, IClassFixture<ConnectionInterceptionWithDiagnosticsSqliteTest.InterceptionSqliteFixture>
    {
        public ConnectionInterceptionWithDiagnosticsSqliteTest(InterceptionSqliteFixture fixture)
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
