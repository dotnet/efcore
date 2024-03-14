// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class QueryExpressionInterceptionWithDiagnosticsCosmosTest(
    QueryExpressionInterceptionWithDiagnosticsCosmosTest.InterceptionCosmosFixture fixture)
    : QueryExpressionInterceptionTestBase(fixture),
        IClassFixture<QueryExpressionInterceptionWithDiagnosticsCosmosTest.InterceptionCosmosFixture>
{
    public override Task Intercept_query_passively(bool async, bool inject)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Intercept_query_passively(a, inject));

    public override Task Intercept_query_with_multiple_interceptors(bool async, bool inject)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Intercept_query_with_multiple_interceptors(a, inject));

    public override Task Intercept_to_change_query_expression(bool async, bool inject)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Intercept_to_change_query_expression(a, inject));

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
