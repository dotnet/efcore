// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class SaveChangesInterceptionSqliteTestBase : SaveChangesInterceptionTestBase
{
    protected SaveChangesInterceptionSqliteTestBase(InterceptionSqliteFixtureBase fixture)
        : base(fixture)
    {
    }

    public abstract class InterceptionSqliteFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName
            => "SaveChangesInterception";

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlite(), injectedInterceptors);
    }

    public class SaveChangesInterceptionSqliteTest(SaveChangesInterceptionSqliteTest.InterceptionSqliteFixture fixture)
        : SaveChangesInterceptionSqliteTestBase(fixture), IClassFixture<SaveChangesInterceptionSqliteTest.InterceptionSqliteFixture>
    {
        public class InterceptionSqliteFixture : InterceptionSqliteFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class SaveChangesInterceptionWithDiagnosticsSqliteTest(SaveChangesInterceptionWithDiagnosticsSqliteTest.InterceptionSqliteFixture fixture)
        : SaveChangesInterceptionSqliteTestBase(fixture),
            IClassFixture<SaveChangesInterceptionWithDiagnosticsSqliteTest.InterceptionSqliteFixture>
    {
        public class InterceptionSqliteFixture : InterceptionSqliteFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
