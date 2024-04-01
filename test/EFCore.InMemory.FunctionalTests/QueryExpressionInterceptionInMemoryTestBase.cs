// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class QueryExpressionInterceptionInMemoryTestBase : QueryExpressionInterceptionTestBase
{
    protected QueryExpressionInterceptionInMemoryTestBase(InterceptionInMemoryFixtureBase fixture)
        : base(fixture)
    {
    }

    public override async Task<UniverseContext> SeedAsync(UniverseContext context)
    {
        await base.SeedAsync(context);

        context.AddRange(
            new Singularity { Id = 77, Type = "Black Hole" },
            new Singularity { Id = 88, Type = "Bing Bang" },
            new Brane { Id = 77, Type = "Black Hole?" },
            new Brane { Id = 88, Type = "Bing Bang?" });

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        return context;
    }

    public abstract class InterceptionInMemoryFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName
            => "QueryExpressionInterception";

        protected override ITestStoreFactory TestStoreFactory
            => InMemoryTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkInMemoryDatabase(), injectedInterceptors);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Ignore(InMemoryEventId.TransactionIgnoredWarning));
    }

    public class QueryExpressionInterceptionInMemoryTest(QueryExpressionInterceptionInMemoryTest.InterceptionInMemoryFixture fixture)
        : QueryExpressionInterceptionInMemoryTestBase(fixture),
            IClassFixture<QueryExpressionInterceptionInMemoryTest.InterceptionInMemoryFixture>
    {
        public class InterceptionInMemoryFixture : InterceptionInMemoryFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class QueryExpressionInterceptionWithDiagnosticsInMemoryTest(
        QueryExpressionInterceptionWithDiagnosticsInMemoryTest.InterceptionInMemoryFixture fixture)
        : QueryExpressionInterceptionInMemoryTestBase(fixture),
            IClassFixture<QueryExpressionInterceptionWithDiagnosticsInMemoryTest.InterceptionInMemoryFixture>
    {
        public class InterceptionInMemoryFixture : InterceptionInMemoryFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
