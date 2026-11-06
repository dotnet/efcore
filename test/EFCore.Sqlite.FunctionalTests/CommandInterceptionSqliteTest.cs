// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public abstract class CommandInterceptionSqliteTestBase(CommandInterceptionSqliteTestBase.InterceptionSqliteFixtureBase fixture)
    : CommandInterceptionTestBase(fixture)
{
    public override async Task<string> Intercept_query_passively(bool async, bool inject)
    {
        AssertSql(
            """
SELECT "s"."Id", "s"."Type" FROM "Singularity" AS "s"
""",
            await base.Intercept_query_passively(async, inject));

        return null!;
    }

    protected override async Task<string> QueryMutationTest<TInterceptor>(bool async, bool inject)
    {
        AssertSql(
            """
SELECT "s"."Id", "s"."Type" FROM "Brane" AS "s"
""",
            await base.QueryMutationTest<TInterceptor>(async, inject));

        return null!;
    }

    public override async Task<string> Intercept_query_to_replace_execution(bool async, bool inject)
    {
        AssertSql(
            """
SELECT "s"."Id", "s"."Type" FROM "Singularity" AS "s"
""",
            await base.Intercept_query_to_replace_execution(async, inject));

        return null!;
    }

    [Theory, InlineData(false, false), InlineData(true, false), InlineData(false, true), InlineData(true, true)]
    public async Task CommandFailed_is_called_when_SaveChanges_fails_while_advancing_results(bool async, bool inject)
    {
        var (context, interceptor) = await CreateContextAsync<NextResultFailureCommandInterceptor>(inject);
        using (context)
        {
            var singularities = new[]
            {
                new Singularity { Id = 77, Type = "Black Hole updated" },
                new Singularity { Id = 88, Type = "Bing Bang updated" }
            };
            context.AttachRange(singularities);

            foreach (var singularity in singularities)
            {
                context.Entry(singularity).Property(s => s.Type).IsModified = true;
            }

            var exception = async
                ? await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync())
                : Assert.Throws<DbUpdateException>(() => context.SaveChanges());

            Assert.Same(interceptor.FailureException, exception.InnerException);
            Assert.True(interceptor.FailedCalled);
            Assert.Equal(1, interceptor.FailureCount);
        }
    }

    [Theory, InlineData(false, false), InlineData(true, false), InlineData(false, true), InlineData(true, true)]
    public async Task CommandFailed_is_called_for_sqlite_overflow_while_reading(bool async, bool buffered)
    {
        var interceptor = new PassiveReaderCommandInterceptor();
        var options = new DbContextOptionsBuilder(Fixture.CreateOptions([interceptor], []));
        if (buffered)
        {
            options.UseSqlite(o => o.ExecutionStrategy(d => new BufferingExecutionStrategy(d)));
        }

        using var context = new UniverseContext(options.Options);
        var query = context.Database.SqlQueryRaw<long>(
            "SELECT abs(value) AS Value FROM (SELECT 1 AS value UNION ALL SELECT -9223372036854775808)");
        var exception = async
            ? await Assert.ThrowsAsync<Microsoft.Data.Sqlite.SqliteException>(() => query.ToListAsync())
            : Assert.Throws<Microsoft.Data.Sqlite.SqliteException>(() => query.ToList());

        Assert.Equal(1, exception.SqliteErrorCode);
        Assert.True(interceptor.ExecutedCalled);
        Assert.True(interceptor.FailedCalled);
        Assert.Same(exception, interceptor.Exception);
    }

    private sealed class BufferingExecutionStrategy(ExecutionStrategyDependencies dependencies)
        : ExecutionStrategy(dependencies, 1, TimeSpan.Zero)
    {
        protected override bool ShouldRetryOn(Exception exception)
            => false;
    }

    private sealed class NextResultFailureCommandInterceptor() : CommandInterceptorBase(DbCommandMethod.ExecuteReader)
    {
        public InvalidOperationException FailureException { get; } = new("Advancing the result failed.");

        public int FailureCount { get; private set; }

        public override DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            base.ReaderExecuted(command, eventData, result);
            result.Dispose();

            return new NextResultFailureDbDataReader(FailureException);
        }

        public override async ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            await base.ReaderExecutedAsync(command, eventData, result, cancellationToken);
            await result.DisposeAsync().ConfigureAwait(false);

            return new NextResultFailureDbDataReader(FailureException);
        }

        public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
        {
            FailureCount++;
            base.CommandFailed(command, eventData);
        }

        public override Task CommandFailedAsync(
            DbCommand command,
            CommandErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            FailureCount++;
            return base.CommandFailedAsync(command, eventData, cancellationToken);
        }
    }

    private sealed class NextResultFailureDbDataReader(Exception exception) : FakeDbDataReader
    {
        private bool _read;

        public override int FieldCount
            => 1;

        public override bool HasRows
            => true;

        public override bool Read()
        {
            return !_read && (_read = true);
        }

        public override Task<bool> ReadAsync(CancellationToken cancellationToken)
            => Task.FromResult(Read());

        public override int GetInt32(int ordinal)
            => 1;

        public override bool NextResult()
            => throw exception;

        public override Task<bool> NextResultAsync(CancellationToken cancellationToken)
            => Task.FromException<bool>(exception);
    }

    public abstract class InterceptionSqliteFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName
            => "CommandInterception";

        protected override ITestStoreFactory TestStoreFactory
            => SqliteTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlite(), injectedInterceptors);
    }

    public class CommandInterceptionSqliteTest(CommandInterceptionSqliteTest.InterceptionSqliteFixture fixture)
        : CommandInterceptionSqliteTestBase(fixture), IClassFixture<CommandInterceptionSqliteTest.InterceptionSqliteFixture>
    {
        public class InterceptionSqliteFixture : InterceptionSqliteFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class CommandInterceptionWithDiagnosticsSqliteTest(
        CommandInterceptionWithDiagnosticsSqliteTest.InterceptionSqliteFixture fixture)
        : CommandInterceptionSqliteTestBase(fixture), IClassFixture<CommandInterceptionWithDiagnosticsSqliteTest.InterceptionSqliteFixture>
    {
        public class InterceptionSqliteFixture : InterceptionSqliteFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
