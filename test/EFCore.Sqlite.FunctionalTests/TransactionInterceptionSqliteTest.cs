// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class TransactionInterceptionSqliteTestBase : TransactionInterceptionTestBase
{
    protected TransactionInterceptionSqliteTestBase(InterceptionSqliteFixtureBase fixture)
        : base(fixture)
    {
    }

    public abstract class InterceptionSqliteFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName
            => "TransactionInterception";

        protected override ITestStoreFactory TestStoreFactory
            => SharedCacheSqliteTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlite(), injectedInterceptors);
    }

    public class TransactionInterceptionSqliteTest(TransactionInterceptionSqliteTest.InterceptionSqliteFixture fixture)
        : TransactionInterceptionSqliteTestBase(fixture), IClassFixture<TransactionInterceptionSqliteTest.InterceptionSqliteFixture>
    {
        public class InterceptionSqliteFixture : InterceptionSqliteFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class TransactionInterceptionWithDiagnosticsSqliteTest(TransactionInterceptionWithDiagnosticsSqliteTest.InterceptionSqliteFixture fixture)
        : TransactionInterceptionSqliteTestBase(fixture),
            IClassFixture<TransactionInterceptionWithDiagnosticsSqliteTest.InterceptionSqliteFixture>
    {
        public class InterceptionSqliteFixture : InterceptionSqliteFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
