// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class QueryExpressionInterceptionWithDiagnosticsCosmosTest(
    QueryExpressionInterceptionWithDiagnosticsCosmosTest.InterceptionCosmosFixture fixture)
    : QueryExpressionInterceptionTestBase(fixture),
        IClassFixture<QueryExpressionInterceptionWithDiagnosticsCosmosTest.InterceptionCosmosFixture>
{
    public override Task Intercept_query_passively(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Intercept_query_passively(a));

    public override Task Intercept_query_with_multiple_interceptors(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Intercept_query_with_multiple_interceptors(a));

    public override Task Intercept_to_change_query_expression(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Intercept_to_change_query_expression(a));

    public override Task Interceptor_does_not_leak_across_contexts(bool async)
        => CosmosTestHelpers.Instance.NoSyncTest(async, a => base.Interceptor_does_not_leak_across_contexts(a));

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

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.HasDiscriminatorInJsonIds();
        }

        protected override string StoreName
            => "QueryExpressionInterceptionWithDiagnostics";

        protected override bool ShouldSubscribeToDiagnosticListener
            => true;
    }
}
