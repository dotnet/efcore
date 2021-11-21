// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore;

public abstract class ConnectionInterceptionSqlServerTestBase : ConnectionInterceptionTestBase
{
    protected ConnectionInterceptionSqlServerTestBase(InterceptionSqlServerFixtureBase fixture)
        : base(fixture)
    {
    }

    public abstract class InterceptionSqlServerFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName
            => "ConnectionInterception";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlServer(), injectedInterceptors);
    }

    protected override BadUniverseContext CreateBadUniverse(DbContextOptionsBuilder optionsBuilder)
        => new(optionsBuilder.UseSqlServer(new FakeDbConnection()).Options);

    public class FakeDbConnection : DbConnection
    {
        public override string ConnectionString { get; set; }

        public override string Database
            => "Database";

        public override string DataSource
            => "DataSource";

        public override string ServerVersion
            => throw new NotImplementedException();

        public override ConnectionState State
            => ConnectionState.Closed;

        public override void ChangeDatabase(string databaseName)
            => throw new NotImplementedException();

        public override void Close()
            => throw new NotImplementedException();

        public override void Open()
            => throw new NotImplementedException();

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            => throw new NotImplementedException();

        protected override DbCommand CreateDbCommand()
            => throw new NotImplementedException();
    }

    public class ConnectionInterceptionSqlServerTest
        : ConnectionInterceptionSqlServerTestBase, IClassFixture<ConnectionInterceptionSqlServerTest.InterceptionSqlServerFixture>
    {
        public ConnectionInterceptionSqlServerTest(InterceptionSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class ConnectionInterceptionWithDiagnosticsSqlServerTest
        : ConnectionInterceptionSqlServerTestBase,
            IClassFixture<ConnectionInterceptionWithDiagnosticsSqlServerTest.InterceptionSqlServerFixture>
    {
        public ConnectionInterceptionWithDiagnosticsSqlServerTest(InterceptionSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
