// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class TransactionInterceptionSqlServerTestBase : TransactionInterceptionTestBase
{
    protected TransactionInterceptionSqlServerTestBase(InterceptionSqlServerFixtureBase fixture)
        : base(fixture)
    {
    }

    public abstract class InterceptionSqlServerFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName
            => "TransactionInterception";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlServer(), injectedInterceptors);
    }

    public class TransactionInterceptionSqlServerTest
        : TransactionInterceptionSqlServerTestBase, IClassFixture<TransactionInterceptionSqlServerTest.InterceptionSqlServerFixture>
    {
        public TransactionInterceptionSqlServerTest(InterceptionSqlServerFixture fixture)
            : base(fixture)
        {
        }

        // ReleaseSavepoint is unsupported by SQL Server and is ignored
        public override Task Intercept_ReleaseSavepoint(bool async)
            => Task.CompletedTask;

        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class TransactionInterceptionWithDiagnosticsSqlServerTest
        : TransactionInterceptionSqlServerTestBase,
            IClassFixture<TransactionInterceptionWithDiagnosticsSqlServerTest.InterceptionSqlServerFixture>
    {
        public TransactionInterceptionWithDiagnosticsSqlServerTest(InterceptionSqlServerFixture fixture)
            : base(fixture)
        {
        }

        // ReleaseSavepoint is unsupported by SQL Server and is ignored
        public override Task Intercept_ReleaseSavepoint(bool async)
            => Task.CompletedTask;

        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
