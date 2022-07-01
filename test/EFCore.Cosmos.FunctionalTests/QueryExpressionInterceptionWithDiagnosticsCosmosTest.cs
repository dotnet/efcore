// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Cosmos;

public class QueryExpressionInterceptionWithDiagnosticsCosmosTest
    : QueryExpressionInterceptionTestBase,
        IClassFixture<QueryExpressionInterceptionWithDiagnosticsCosmosTest.InterceptionCosmosFixture>
{
    public QueryExpressionInterceptionWithDiagnosticsCosmosTest(InterceptionCosmosFixture fixture)
        : base(fixture)
    {
    }

    public class InterceptionCosmosFixture : InterceptionFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkCosmos(), injectedInterceptors);

        protected override string StoreName
            => "QueryExpressionInterceptionWithDiagnostics";

        protected override bool ShouldSubscribeToDiagnosticListener
            => true;
    }
}
