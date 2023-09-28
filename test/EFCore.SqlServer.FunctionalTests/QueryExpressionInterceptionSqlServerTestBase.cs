// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

public abstract class QueryExpressionInterceptionSqlServerTestBase : QueryExpressionInterceptionTestBase
{
    protected QueryExpressionInterceptionSqlServerTestBase(InterceptionSqlServerFixtureBase fixture)
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

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            new SqlServerDbContextOptionsBuilder(base.AddOptions(builder))
                .ExecutionStrategy(d => new SqlServerExecutionStrategy(d));
            return builder;
        }
    }

    public class QueryExpressionInterceptionSqlServerTest
        : QueryExpressionInterceptionSqlServerTestBase, IClassFixture<QueryExpressionInterceptionSqlServerTest.InterceptionSqlServerFixture>
    {
        public QueryExpressionInterceptionSqlServerTest(InterceptionSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
            protected override string StoreName
                => "QueryExpressionInterception";

            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class QueryExpressionInterceptionWithDiagnosticsSqlServerTest
        : QueryExpressionInterceptionSqlServerTestBase,
            IClassFixture<QueryExpressionInterceptionWithDiagnosticsSqlServerTest.InterceptionSqlServerFixture>
    {
        public QueryExpressionInterceptionWithDiagnosticsSqlServerTest(InterceptionSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
            protected override string StoreName
                => "QueryExpressionInterceptionWithDiagnostics";

            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
