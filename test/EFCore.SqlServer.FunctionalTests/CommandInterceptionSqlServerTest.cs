// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class CommandInterceptionSqlServerTestBase : CommandInterceptionTestBase
{
    protected CommandInterceptionSqlServerTestBase(InterceptionSqlServerFixtureBase fixture)
        : base(fixture)
    {
    }

    public override async Task<string> Intercept_query_passively(bool async, bool inject)
    {
        AssertSql(
            """
SELECT [s].[Id], [s].[Type] FROM [Singularity] AS [s]
""",
            await base.Intercept_query_passively(async, inject));

        return null;
    }

    protected override async Task<string> QueryMutationTest<TInterceptor>(bool async, bool inject)
    {
        AssertSql(
            """
SELECT [s].[Id], [s].[Type] FROM [Brane] AS [s]
""",
            await base.QueryMutationTest<TInterceptor>(async, inject));

        return null;
    }

    public override async Task<string> Intercept_query_to_replace_execution(bool async, bool inject)
    {
        AssertSql(
            """
SELECT [s].[Id], [s].[Type] FROM [Singularity] AS [s]
""",
            await base.Intercept_query_to_replace_execution(async, inject));

        return null;
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual async Task<string> Intercept_query_to_get_statistics(bool async, bool inject) // Issue #23535
    {
        var (context, interceptor) = await CreateContextAsync<StatisticsCommandInterceptor>(inject);
        using (context)
        {
            using (async
                       ? await context.Database.BeginTransactionAsync()
                       : context.Database.BeginTransaction())
            {
                var connection = (SqlConnection)context.Database.GetDbConnection();
                var message = "";

                connection.InfoMessage += ConnectionOnInfoMessage;

                using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

                var results = async
                    ? await context.Set<Singularity>().ToListAsync()
                    : context.Set<Singularity>().ToList();

                AssertNormalOutcome(context, interceptor, async, CommandSource.LinqQuery);
                Assert.True(interceptor.DataReaderClosingCalled);
                Assert.True(interceptor.DataReaderDisposingCalled);

                Assert.Contains("Scan count", message);

                results[0].Type = "Big Hole Bang";

                _ = async
                    ? await context.SaveChangesAsync()
                    : context.SaveChanges();

                AssertNormalOutcome(context, interceptor, async, CommandSource.SaveChanges);
                Assert.True(interceptor.DataReaderClosingCalled);
                Assert.True(interceptor.DataReaderDisposingCalled);

                Assert.Contains("Scan count", message);

                AssertExecutedEvents(listener);

                connection.InfoMessage -= ConnectionOnInfoMessage;

                void ConnectionOnInfoMessage(object sender, SqlInfoMessageEventArgs args)
                {
                    Assert.Same(connection, sender);
                    message = args.Message;
                }
            }
        }

        return interceptor.CommandText;
    }

    protected class StatisticsCommandInterceptor : CommandInterceptorBase
    {
        public StatisticsCommandInterceptor()
            : base(DbCommandMethod.ExecuteReader)
        {
        }

        public override InterceptionResult<DbDataReader> ReaderExecuting(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result)
        {
            command.CommandText = "SET STATISTICS IO ON;" + command.CommandText;

            return base.ReaderExecuting(command, eventData, result);
        }

        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
            DbCommand command,
            CommandEventData eventData,
            InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            command.CommandText = "SET STATISTICS IO ON;" + command.CommandText;

            return base.ReaderExecutingAsync(command, eventData, result, cancellationToken);
        }

        public override InterceptionResult DataReaderClosing(
            DbCommand command,
            DataReaderClosingEventData eventData,
            InterceptionResult result)
        {
            eventData.DataReader.NextResult();

            return base.DataReaderClosing(command, eventData, result);
        }

        public override async ValueTask<InterceptionResult> DataReaderClosingAsync(
            DbCommand command,
            DataReaderClosingEventData eventData,
            InterceptionResult result)
        {
            await eventData.DataReader.NextResultAsync();

            return await base.DataReaderClosingAsync(command, eventData, result);
        }
    }

    public abstract class InterceptionSqlServerFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName
            => "CommandInterception";

        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlServer(), injectedInterceptors);
    }

    public class CommandInterceptionSqlServerTest(CommandInterceptionSqlServerTest.InterceptionSqlServerFixture fixture)
        : CommandInterceptionSqlServerTestBase(fixture), IClassFixture<CommandInterceptionSqlServerTest.InterceptionSqlServerFixture>
    {
        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
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

    public class CommandInterceptionWithDiagnosticsSqlServerTest(
        CommandInterceptionWithDiagnosticsSqlServerTest.InterceptionSqlServerFixture fixture)
        : CommandInterceptionSqlServerTestBase(fixture),
            IClassFixture<CommandInterceptionWithDiagnosticsSqlServerTest.InterceptionSqlServerFixture>
    {
        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
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
