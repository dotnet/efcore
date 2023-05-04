// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class QueryExpressionInterceptionInMemoryTestBase : QueryExpressionInterceptionTestBase
{
    protected QueryExpressionInterceptionInMemoryTestBase(InterceptionInMemoryFixtureBase fixture)
        : base(fixture)
    {
    }

    public override UniverseContext Seed(UniverseContext context)
    {
        base.Seed(context);

        context.AddRange(
            new Singularity { Id = 77, Type = "Black Hole" },
            new Singularity { Id = 88, Type = "Bing Bang" },
            new Brane { Id = 77, Type = "Black Hole?" },
            new Brane { Id = 88, Type = "Bing Bang?" });

        context.SaveChanges();
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

    public class QueryExpressionInterceptionInMemoryTest
        : QueryExpressionInterceptionInMemoryTestBase, IClassFixture<QueryExpressionInterceptionInMemoryTest.InterceptionInMemoryFixture>
    {
        public QueryExpressionInterceptionInMemoryTest(InterceptionInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionInMemoryFixture : InterceptionInMemoryFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class QueryExpressionInterceptionWithDiagnosticsInMemoryTest
        : QueryExpressionInterceptionInMemoryTestBase,
            IClassFixture<QueryExpressionInterceptionWithDiagnosticsInMemoryTest.InterceptionInMemoryFixture>
    {
        public QueryExpressionInterceptionWithDiagnosticsInMemoryTest(InterceptionInMemoryFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionInMemoryFixture : InterceptionInMemoryFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
