// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class SaveChangesInterceptionSqlServerTestBase : SaveChangesInterceptionTestBase
{
    protected SaveChangesInterceptionSqlServerTestBase(InterceptionSqlServerFixtureBase fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public virtual async Task Intercept_concurrency_with_relational_specific_data(bool async, bool inject, bool noAcceptChanges)
    {
        var saveChangesInterceptor = new RelationalConcurrencySaveChangesInterceptor();
        var commandInterceptor = new TestCommandInterceptor();

        var context = await CreateContextAsync(saveChangesInterceptor, commandInterceptor);

        using var _ = context;

        using var transaction = context.Database.BeginTransaction();

        var entry = context.Entry(new Singularity { Id = 35, Type = "Red Dwarf" });
        entry.State = EntityState.Modified;

        using var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId);

        Exception thrown = null;

        try
        {
            var __ = noAcceptChanges
                ? async
                    ? await context.SaveChangesAsync()
                    : context.SaveChanges()
                : async
                    ? await context.SaveChangesAsync(acceptAllChangesOnSuccess: false)
                    : context.SaveChanges(acceptAllChangesOnSuccess: false);
        }
        catch (Exception e)
        {
            thrown = e;
        }

        Assert.Equal(async, saveChangesInterceptor.AsyncCalled);
        Assert.NotEqual(async, saveChangesInterceptor.SyncCalled);
        Assert.NotEqual(saveChangesInterceptor.AsyncCalled, saveChangesInterceptor.SyncCalled);
        Assert.False(saveChangesInterceptor.FailedCalled);
        Assert.Same(context, saveChangesInterceptor.Context);
        Assert.Same(thrown, saveChangesInterceptor.Exception);

        Assert.True(saveChangesInterceptor.ConcurrencyExceptionCalled);
        Assert.Equal(1, saveChangesInterceptor.Entries.Count);
        Assert.Same(entry.Entity, saveChangesInterceptor.Entries[0].Entity);

        Assert.Same(commandInterceptor.Connection, saveChangesInterceptor.Connection);
        Assert.Same(commandInterceptor.Command, saveChangesInterceptor.Command);
        Assert.Same(commandInterceptor.DataReader, saveChangesInterceptor.DataReader);
        Assert.Equal(commandInterceptor.ConnectionId, saveChangesInterceptor.ConnectionId);
        Assert.Equal(commandInterceptor.CommandId, saveChangesInterceptor.CommandId);
    }

    protected class RelationalConcurrencySaveChangesInterceptor : SaveChangesInterceptorBase
    {
        public DbConnection Connection { get; set; }
        public DbCommand Command { get; set; }
        public DbDataReader DataReader { get; set; }
        public Guid CommandId { get; set; }
        public Guid ConnectionId { get; set; }

        public override InterceptionResult ThrowingConcurrencyException(ConcurrencyExceptionEventData eventData, InterceptionResult result)
        {
            RecordEventData((RelationalConcurrencyExceptionEventData)eventData);

            return base.ThrowingConcurrencyException(eventData, result);
        }

        public override ValueTask<InterceptionResult> ThrowingConcurrencyExceptionAsync(
            ConcurrencyExceptionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            RecordEventData((RelationalConcurrencyExceptionEventData)eventData);

            return base.ThrowingConcurrencyExceptionAsync(eventData, result, cancellationToken);
        }

        private void RecordEventData(RelationalConcurrencyExceptionEventData eventData)
        {
            Assert.NotNull(eventData.Connection);
            Assert.NotNull(eventData.Command);
            Assert.NotNull(eventData.DataReader);

            DataReader = eventData.DataReader;
            Command = eventData.Command;
            Connection = eventData.Connection;
            ConnectionId = eventData.ConnectionId;
            CommandId = eventData.CommandId;
        }
    }

    protected class TestCommandInterceptor : IDbCommandInterceptor
    {
        public DbConnection Connection { get; set; }
        public DbCommand Command { get; set; }
        public DbDataReader DataReader { get; set; }
        public Guid CommandId { get; set; }
        public Guid ConnectionId { get; set; }

        public virtual DbDataReader ReaderExecuted(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result)
        {
            RecordEventData(command, eventData, result);

            return result;
        }

        public virtual ValueTask<DbDataReader> ReaderExecutedAsync(
            DbCommand command,
            CommandExecutedEventData eventData,
            DbDataReader result,
            CancellationToken cancellationToken = default)
        {
            RecordEventData(command, eventData, result);

            return ValueTask.FromResult(result);
        }

        private void RecordEventData(DbCommand command, CommandExecutedEventData eventData, DbDataReader result)
        {
            DataReader = result;
            Command = command;
            Connection = command.Connection;
            ConnectionId = eventData.ConnectionId;
            CommandId = eventData.CommandId;
        }
    }

    public abstract class InterceptionSqlServerFixtureBase : InterceptionFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => SqlServerTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlServer(), injectedInterceptors);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            new SqlServerDbContextOptionsBuilder(base.AddOptions(builder))
                .ExecutionStrategy(d => new SqlServerExecutionStrategy(d));
            return builder;
        }
    }

    public class SaveChangesInterceptionSqlServerTest(SaveChangesInterceptionSqlServerTest.InterceptionSqlServerFixture fixture)
        : SaveChangesInterceptionSqlServerTestBase(fixture),
            IClassFixture<SaveChangesInterceptionSqlServerTest.InterceptionSqlServerFixture>
    {
        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
            protected override string StoreName
                => "SaveChangesInterception";

            protected override bool ShouldSubscribeToDiagnosticListener
                => false;
        }
    }

    public class SaveChangesInterceptionWithDiagnosticsSqlServerTest(
        SaveChangesInterceptionWithDiagnosticsSqlServerTest.InterceptionSqlServerFixture fixture)
        : SaveChangesInterceptionSqlServerTestBase(fixture),
            IClassFixture<SaveChangesInterceptionWithDiagnosticsSqlServerTest.InterceptionSqlServerFixture>
    {
        public class InterceptionSqlServerFixture : InterceptionSqlServerFixtureBase
        {
            protected override string StoreName
                => "SaveChangesInterceptionWithDiagnostics";

            protected override bool ShouldSubscribeToDiagnosticListener
                => true;
        }
    }
}
