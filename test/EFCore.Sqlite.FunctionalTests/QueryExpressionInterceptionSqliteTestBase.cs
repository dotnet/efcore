// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class QueryExpressionInterceptionSqliteTestBase : QueryExpressionInterceptionTestBase
{
    protected QueryExpressionInterceptionSqliteTestBase(InterceptionSqliteFixtureBase fixture)
        : base(fixture)
    {
    }

    public abstract class InterceptionSqliteFixtureBase : InterceptionFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlite(), injectedInterceptors);
    }

    public class QueryExpressionInterceptionSqliteTest
        : QueryExpressionInterceptionSqliteTestBase, IClassFixture<QueryExpressionInterceptionSqliteTest.InterceptionSqliteFixture>
    {
        public QueryExpressionInterceptionSqliteTest(InterceptionSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionSqliteFixture : InterceptionSqliteFixtureBase
        {
            protected override string StoreName
                => "QueryExpressionInterception";

            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class QueryExpressionInterceptionWithDiagnosticsSqliteTest
        : QueryExpressionInterceptionSqliteTestBase,
            IClassFixture<QueryExpressionInterceptionWithDiagnosticsSqliteTest.InterceptionSqliteFixture>
    {
        public QueryExpressionInterceptionWithDiagnosticsSqliteTest(InterceptionSqliteFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionSqliteFixture : InterceptionSqliteFixtureBase
        {
            protected override string StoreName
                => "QueryExpressionInterceptionWithDiagnostics";

            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
