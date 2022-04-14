// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

public abstract class SaveChangesInterceptionSqlServerTestBase : SaveChangesInterceptionTestBase
{
    protected SaveChangesInterceptionSqlServerTestBase(InterceptionSqlServerFixtureBase fixture)
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
    }

    public class SaveChangesInterceptionSqlServerTest
        : SaveChangesInterceptionSqlServerTestBase, IClassFixture<SaveChangesInterceptionSqlServerTest.InterceptionSqlServerFixture>
    {
        public SaveChangesInterceptionSqlServerTest(InterceptionSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
            protected override string StoreName
                => "SaveChangesInterception";

            protected override bool ShouldSubscribeToDiagnosticListener
                => false;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                new SqlServerDbContextOptionsBuilder(base.AddOptions(builder))
                    .ExecutionStrategy(d => new SqlServerExecutionStrategy(d));
                return builder;
            }
        }
    }

    public class SaveChangesInterceptionWithDiagnosticsSqlServerTest
        : SaveChangesInterceptionSqlServerTestBase,
            IClassFixture<SaveChangesInterceptionWithDiagnosticsSqlServerTest.InterceptionSqlServerFixture>
    {
        public SaveChangesInterceptionWithDiagnosticsSqlServerTest(InterceptionSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
            protected override string StoreName
                => "SaveChangesInterceptionWithDiagnostics";

            protected override bool ShouldSubscribeToDiagnosticListener
                => true;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                new SqlServerDbContextOptionsBuilder(base.AddOptions(builder))
                    .ExecutionStrategy(d => new SqlServerExecutionStrategy(d));
                return builder;
            }
        }
    }
}
