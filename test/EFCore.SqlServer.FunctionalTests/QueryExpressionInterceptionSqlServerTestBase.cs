// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

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

    public class QueryExpressionInterceptionSqlServerTest(QueryExpressionInterceptionSqlServerTest.InterceptionSqlServerFixture fixture)
        : QueryExpressionInterceptionSqlServerTestBase(fixture), IClassFixture<QueryExpressionInterceptionSqlServerTest.InterceptionSqlServerFixture>
    {
        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
            protected override string StoreName
                => "QueryExpressionInterception";

            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class QueryExpressionInterceptionWithDiagnosticsSqlServerTest(QueryExpressionInterceptionWithDiagnosticsSqlServerTest.InterceptionSqlServerFixture fixture)
        : QueryExpressionInterceptionSqlServerTestBase(fixture),
            IClassFixture<QueryExpressionInterceptionWithDiagnosticsSqlServerTest.InterceptionSqlServerFixture>
    {
        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
            protected override string StoreName
                => "QueryExpressionInterceptionWithDiagnostics";

            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
