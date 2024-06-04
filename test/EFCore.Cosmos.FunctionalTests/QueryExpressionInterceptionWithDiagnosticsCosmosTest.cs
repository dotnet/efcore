// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class QueryExpressionInterceptionWithDiagnosticsCosmosTest(
    QueryExpressionInterceptionWithDiagnosticsCosmosTest.InterceptionCosmosFixture fixture)
    : QueryExpressionInterceptionTestBase(fixture),
        IClassFixture<QueryExpressionInterceptionWithDiagnosticsCosmosTest.InterceptionCosmosFixture>
{
    public class InterceptionCosmosFixture : InterceptionFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => CosmosTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined));

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
