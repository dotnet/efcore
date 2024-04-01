// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;

namespace Microsoft.EntityFrameworkCore;

public abstract class ConnectionInterceptionTestBase : InterceptionTestBase
{
    protected ConnectionInterceptionTestBase(InterceptionFixtureBase fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_connection_passively(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<ConnectionInterceptor>();
        using (context)
        {
            // Test infrastructure uses an open connection, so close it first.
            var connection = context.Database.GetDbConnection();
            var startedOpen = connection.State == ConnectionState.Open;
            if (startedOpen)
            {
                connection.Close();
            }

            using (var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId))
            {
                if (async)
                {
                    await context.Database.OpenConnectionAsync();
                }
                else
                {
                    context.Database.OpenConnection();
                }

                AssertNormalOpen(context, interceptor, async);

                interceptor.Reset();

                if (async)
                {
                    await context.Database.CloseConnectionAsync();
                }
                else
                {
                    context.Database.CloseConnection();
                }

                AssertNormalClose(context, interceptor, async);

                AsertOpenCloseEvents(listener);
            }

            if (startedOpen)
            {
                connection.Open();
            }
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_connection_to_override_opening(bool async)
    {
        var (context, interceptor) = await CreateContextAsync<ConnectionOverridingInterceptor>();
        using (context)
        {
            // Test infrastructure uses an open connection, so close it first.
            var connection = context.Database.GetDbConnection();
            var startedOpen = connection.State == ConnectionState.Open;
            if (startedOpen)
            {
                connection.Close();
            }

            using (var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId))
            {
                if (async)
                {
                    await context.Database.OpenConnectionAsync();
                }
                else
                {
                    context.Database.OpenConnection();
                }

                AssertNormalOpen(context, interceptor, async);

                interceptor.Reset();

                if (async)
                {
                    await context.Database.CloseConnectionAsync();
                }
                else
                {
                    context.Database.CloseConnection();
                }

                AssertNormalClose(context, interceptor, async);

                AsertOpenCloseEvents(listener);
            }

            if (startedOpen)
            {
                connection.Open();
            }
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_connection_with_multiple_interceptors(bool async)
    {
        var interceptor1 = new ConnectionInterceptor();
        var interceptor2 = new ConnectionOverridingInterceptor();
        var interceptor3 = new ConnectionInterceptor();
        var interceptor4 = new ConnectionOverridingInterceptor();
        using var context = await CreateContextAsync(
            new IInterceptor[] { new NoOpConnectionInterceptor(), interceptor1, interceptor2 },
            new IInterceptor[] { interceptor3, interceptor4, new NoOpConnectionInterceptor() });
        // Test infrastructure uses an open connection, so close it first.
        var connection = context.Database.GetDbConnection();
        var startedOpen = connection.State == ConnectionState.Open;
        if (startedOpen)
        {
            connection.Close();
        }

        using (var listener = Fixture.SubscribeToDiagnosticListener(context.ContextId))
        {
            if (async)
            {
                await context.Database.OpenConnectionAsync();
            }
            else
            {
                context.Database.OpenConnection();
            }

            AssertNormalOpen(context, interceptor1, async);
            AssertNormalOpen(context, interceptor2, async);
            AssertNormalOpen(context, interceptor3, async);
            AssertNormalOpen(context, interceptor4, async);

            interceptor1.Reset();
            interceptor2.Reset();
            interceptor3.Reset();
            interceptor4.Reset();

            if (async)
            {
                await context.Database.CloseConnectionAsync();
            }
            else
            {
                context.Database.CloseConnection();
            }

            AssertNormalClose(context, interceptor1, async);
            AssertNormalClose(context, interceptor2, async);
            AssertNormalClose(context, interceptor3, async);
            AssertNormalClose(context, interceptor4, async);

            AsertOpenCloseEvents(listener);
        }

        if (startedOpen)
        {
            connection.Open();
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_connection_that_throws_on_open(bool async)
    {
        var interceptor = new ConnectionInterceptor();

        using var context = CreateBadUniverse(new DbContextOptionsBuilder().AddInterceptors(interceptor));
        try
        {
            if (async)
            {
                await context.Database.OpenConnectionAsync();
            }
            else
            {
                context.Database.OpenConnection();
            }

            Assert.False(true);
        }
        catch (Exception exception)
        {
            Assert.Same(interceptor.Exception, exception);
        }

        AssertErrorOnOpen(context, interceptor, async);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_connection_creation_passively(bool async)
    {
        var interceptor = new ConnectionCreationInterceptor();
        var context = new ConnectionStringContext(ConfigureProvider);
        var connectionDisposed = false;
        context.Interceptors.Add(interceptor);

        _ = context.Model;

        Assert.False(interceptor.CreatingCalled);
        Assert.False(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);

        var connection = context.Database.GetDbConnection();
        connection.Disposed += (_, __) => connectionDisposed = true;

        Assert.True(interceptor.CreatingCalled);
        Assert.True(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Same(connection, interceptor.Connection);
        Assert.Equal(connection.ConnectionString, interceptor.ConnectionString ?? "");

        if (async)
        {
            await context.DisposeAsync();
        }
        else
        {
            context.Dispose();
        }

        Assert.True(interceptor.DisposingCalled);
        Assert.True(interceptor.DisposedCalled);
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.True(connectionDisposed);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_connection_to_override_creation(bool async)
    {
        using var tempContext = new ConnectionStringContext(ConfigureProvider);
        var replacementConnection = tempContext.Database.GetDbConnection();
        var connectionDisposed = false;
        replacementConnection.Disposed += (_, __) => connectionDisposed = true;

        var interceptor = new ConnectionCreationOverrideInterceptor(replacementConnection);
        var context = new ConnectionStringContext(ConfigureProvider);
        context.Interceptors.Add(interceptor);

        _ = context.Model;

        Assert.False(interceptor.CreatingCalled);
        Assert.False(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);

        var connection = context.Database.GetDbConnection();
        Assert.Same(replacementConnection, connection);

        Assert.True(interceptor.CreatingCalled);
        Assert.True(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Same(connection, interceptor.Connection);
        Assert.Equal(connection.ConnectionString, interceptor.ConnectionString ?? "");

        if (async)
        {
            await context.DisposeAsync();
        }
        else
        {
            context.Dispose();
        }

        Assert.True(interceptor.DisposingCalled);
        Assert.True(interceptor.DisposedCalled);
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.True(connectionDisposed);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_connection_to_override_connection_after_creation(bool async)
    {
        using var tempContext = new ConnectionStringContext(ConfigureProvider);
        var replacementConnection = tempContext.Database.GetDbConnection();
        var connectionDisposed = false;
        replacementConnection.Disposed += (_, __) => connectionDisposed = true;

        var interceptor = new ConnectionCreationReplaceInterceptor(replacementConnection);
        var context = new ConnectionStringContext(ConfigureProvider);
        context.Interceptors.Add(interceptor);

        _ = context.Model;

        Assert.False(interceptor.CreatingCalled);
        Assert.False(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);

        var connection = context.Database.GetDbConnection();
        Assert.Same(replacementConnection, connection);

        Assert.True(interceptor.CreatingCalled);
        Assert.True(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Same(connection, interceptor.Connection);
        Assert.Equal(connection.ConnectionString, interceptor.ConnectionString ?? "");

        if (async)
        {
            await context.DisposeAsync();
        }
        else
        {
            context.Dispose();
        }

        Assert.True(interceptor.DisposingCalled);
        Assert.True(interceptor.DisposedCalled);
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.True(connectionDisposed);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_connection_to_suppress_dispose(bool async)
    {
        var interceptor = new ConnectionCreationNoDisposeInterceptor();
        var context = new ConnectionStringContext(ConfigureProvider);
        var connectionDisposed = false;
        context.Interceptors.Add(interceptor);

        _ = context.Model;

        Assert.False(interceptor.CreatingCalled);
        Assert.False(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);

        var connection = context.Database.GetDbConnection();
        connection.Disposed += (_, __) => connectionDisposed = true;

        Assert.True(interceptor.CreatingCalled);
        Assert.True(interceptor.CreatedCalled);
        Assert.False(interceptor.DisposingCalled);
        Assert.False(interceptor.DisposedCalled);
        Assert.Same(context, interceptor.Context);
        Assert.Same(connection, interceptor.Connection);
        Assert.Equal(connection.ConnectionString, interceptor.ConnectionString ?? "");

        if (async)
        {
            await context.DisposeAsync();
        }
        else
        {
            context.Dispose();
        }

        Assert.True(interceptor.DisposingCalled);
        Assert.True(interceptor.DisposedCalled);
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.False(connectionDisposed);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Intercept_connection_creation_with_multiple_interceptors(bool async)
    {
        using var tempContext1 = new ConnectionStringContext(ConfigureProvider);
        var replacementConnection1 = tempContext1.Database.GetDbConnection();
        var connectionDisposed1 = false;
        replacementConnection1.Disposed += (_, __) => connectionDisposed1 = true;

        using var tempContext2 = new ConnectionStringContext(ConfigureProvider);
        var replacementConnection2 = tempContext2.Database.GetDbConnection();
        var connectionDisposed2 = false;
        replacementConnection2.Disposed += (_, __) => connectionDisposed2 = true;

        var interceptors = new[]
        {
            new ConnectionCreationInterceptor(),
            new ConnectionCreationOverrideInterceptor(replacementConnection1),
            new ConnectionCreationInterceptor(),
            new ConnectionCreationOverrideInterceptor(replacementConnection2)
        };

        var context = new ConnectionStringContext(ConfigureProvider);
        context.Interceptors.AddRange(interceptors);

        var connection = context.Database.GetDbConnection();
        Assert.Same(replacementConnection2, connection);

        foreach (var interceptor in interceptors)
        {
            Assert.True(interceptor.CreatingCalled);
            Assert.True(interceptor.CreatedCalled);
            Assert.False(interceptor.DisposingCalled);
            Assert.False(interceptor.DisposedCalled);
            Assert.Same(context, interceptor.Context);
        }

        if (async)
        {
            await context.DisposeAsync();
        }
        else
        {
            context.Dispose();
        }

        foreach (var interceptor in interceptors)
        {
            Assert.True(interceptor.DisposingCalled);
            Assert.True(interceptor.DisposedCalled);
            Assert.Equal(async, interceptor.AsyncCalled);
            Assert.NotEqual(async, interceptor.SyncCalled);
        }

        Assert.False(connectionDisposed1);
        Assert.True(connectionDisposed2);
    }

    protected abstract DbContextOptionsBuilder ConfigureProvider(DbContextOptionsBuilder optionsBuilder);

    protected class ConnectionCreationInterceptor : IDbConnectionInterceptor
    {
        public DbContext? Context { get; set; }
        public string? ConnectionString { get; set; }
        public DbConnection? Connection { get; set; }
        public Guid ConnectionId { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public bool CreatingCalled { get; set; }
        public bool CreatedCalled { get; set; }
        public bool AsyncCalled { get; set; }
        public bool SyncCalled { get; set; }
        public bool DisposingCalled { get; set; }
        public bool DisposedCalled { get; set; }

        public virtual InterceptionResult<DbConnection> ConnectionCreating(
            ConnectionCreatingEventData eventData,
            InterceptionResult<DbConnection> result)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotEqual(default, eventData.ConnectionId);
            Assert.NotEqual(default, eventData.StartTime);

            Context = eventData.Context;
            ConnectionString = eventData.ConnectionString;
            ConnectionId = eventData.ConnectionId;
            StartTime = eventData.StartTime;
            CreatingCalled = true;

            return result;
        }

        public virtual DbConnection ConnectionCreated(ConnectionCreatedEventData eventData, DbConnection result)
        {
            Assert.Same(Context, eventData.Context);
            Assert.Equal(ConnectionId, eventData.ConnectionId);
            Assert.Equal(StartTime, eventData.StartTime);
            Assert.NotNull(result);

            Connection = result;
            CreatedCalled = true;

            return result;
        }

        public virtual InterceptionResult ConnectionDisposing(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertDisposing(eventData);

            return result;
        }

        public virtual ValueTask<InterceptionResult> ConnectionDisposingAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertDisposing(eventData);

            return ValueTask.FromResult(result);
        }

        public virtual void ConnectionDisposed(
            DbConnection connection,
            ConnectionEndEventData eventData)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertDisposed(eventData);
        }

        public virtual Task ConnectionDisposedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertDisposed(eventData);

            return Task.CompletedTask;
        }

        protected virtual void AssertDisposing(ConnectionEventData eventData)
        {
            Assert.Same(Context, eventData.Context);
            Assert.Equal(ConnectionId, eventData.ConnectionId);

            DisposingCalled = true;
        }

        protected virtual void AssertDisposed(ConnectionEndEventData eventData)
        {
            Assert.Same(Context, eventData.Context);
            Assert.Equal(ConnectionId, eventData.ConnectionId);

            DisposedCalled = true;
        }
    }

    protected class ConnectionCreationOverrideInterceptor(DbConnection replacementConnection) : ConnectionCreationInterceptor
    {
        private readonly DbConnection _replacementConnection = replacementConnection;

        public override InterceptionResult<DbConnection> ConnectionCreating(
            ConnectionCreatingEventData eventData,
            InterceptionResult<DbConnection> result)
        {
            base.ConnectionCreating(eventData, result);

            return InterceptionResult<DbConnection>.SuppressWithResult(_replacementConnection);
        }
    }

    protected class ConnectionCreationReplaceInterceptor(DbConnection replacementConnection) : ConnectionCreationInterceptor
    {
        private readonly DbConnection _replacementConnection = replacementConnection;

        public override DbConnection ConnectionCreated(ConnectionCreatedEventData eventData, DbConnection result)
        {
            base.ConnectionCreated(eventData, result);

            Connection = _replacementConnection;
            return _replacementConnection;
        }
    }

    protected class ConnectionCreationNoDisposeInterceptor : ConnectionCreationInterceptor
    {
        public override InterceptionResult ConnectionDisposing(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            base.ConnectionDisposing(connection, eventData, result);

            return InterceptionResult.Suppress();
        }

        public override async ValueTask<InterceptionResult> ConnectionDisposingAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            await base.ConnectionDisposingAsync(connection, eventData, result);

            return InterceptionResult.Suppress();
        }
    }

    protected class ConnectionStringContext(Func<DbContextOptionsBuilder, DbContextOptionsBuilder> configureProvider) : DbContext
    {
        private readonly Func<DbContextOptionsBuilder, DbContextOptionsBuilder> _configureProvider = configureProvider;

        public List<ConnectionCreationInterceptor> Interceptors { get; } = [];

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => _configureProvider(optionsBuilder).AddInterceptors(Interceptors);
    }

    protected abstract BadUniverseContext CreateBadUniverse(DbContextOptionsBuilder optionsBuilder);

    protected class BadUniverseContext(DbContextOptions options) : UniverseContext(options);

    protected class NoOpConnectionInterceptor : DbConnectionInterceptor;

    protected class ConnectionOverridingInterceptor : ConnectionInterceptor
    {
        public override InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            base.ConnectionOpening(connection, eventData, result);

            if (!result.IsSuppressed)
            {
                connection.Open();
            }

            return InterceptionResult.Suppress();
        }

        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);

            if (!result.IsSuppressed)
            {
                await connection.OpenAsync(cancellationToken);
            }

            return InterceptionResult.Suppress();
        }
    }

    protected class ConnectionInterceptor : IDbConnectionInterceptor
    {
        public DbContext? Context { get; set; }
        public Exception? Exception { get; set; }
        public DbContextId ContextId { get; set; }
        public Guid ConnectionId { get; set; }
        public bool AsyncCalled { get; set; }
        public bool SyncCalled { get; set; }
        public bool OpeningCalled { get; set; }
        public bool OpenedCalled { get; set; }
        public bool ClosingCalled { get; set; }
        public bool ClosedCalled { get; set; }
        public bool FailedCalled { get; set; }

        public void Reset()
        {
            Context = null;
            Exception = null;
            ContextId = default;
            ConnectionId = default;
            AsyncCalled = false;
            SyncCalled = false;
            OpeningCalled = false;
            OpenedCalled = false;
            ClosingCalled = false;
            ClosedCalled = false;
            FailedCalled = false;
        }

        public virtual InterceptionResult ConnectionOpening(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertOpening(eventData);

            return result;
        }

        public virtual ValueTask<InterceptionResult> ConnectionOpeningAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertOpening(eventData);

            return ValueTask.FromResult(result);
        }

        public virtual void ConnectionOpened(
            DbConnection connection,
            ConnectionEndEventData eventData)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertOpened(eventData);
        }

        public virtual Task ConnectionOpenedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertOpened(eventData);

            return Task.CompletedTask;
        }

        public virtual InterceptionResult ConnectionClosing(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertClosing(eventData);

            return result;
        }

        public virtual ValueTask<InterceptionResult> ConnectionClosingAsync(
            DbConnection connection,
            ConnectionEventData eventData,
            InterceptionResult result)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertClosing(eventData);

            return ValueTask.FromResult(result);
        }

        public virtual void ConnectionClosed(
            DbConnection connection,
            ConnectionEndEventData eventData)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertClosed(eventData);
        }

        public virtual Task ConnectionClosedAsync(
            DbConnection connection,
            ConnectionEndEventData eventData)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertClosed(eventData);

            return Task.CompletedTask;
        }

        public virtual void ConnectionFailed(
            DbConnection connection,
            ConnectionErrorEventData eventData)
        {
            Assert.False(eventData.IsAsync);
            SyncCalled = true;
            AssertFailed(eventData);
        }

        public virtual Task ConnectionFailedAsync(
            DbConnection connection,
            ConnectionErrorEventData eventData,
            CancellationToken cancellationToken = default)
        {
            Assert.True(eventData.IsAsync);
            AsyncCalled = true;
            AssertFailed(eventData);

            return Task.CompletedTask;
        }

        protected virtual void AssertOpening(ConnectionEventData eventData)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotEqual(default, eventData.ConnectionId);
            Assert.NotEqual(default, eventData.Context!.ContextId);

            Context = eventData.Context;
            ContextId = Context.ContextId;
            ConnectionId = eventData.ConnectionId;
            OpeningCalled = true;
        }

        protected virtual void AssertOpened(ConnectionEndEventData eventData)
        {
            Assert.Same(Context, eventData.Context);
            Assert.Equal(ConnectionId, eventData.ConnectionId);
            Assert.Equal(ContextId, eventData.Context!.ContextId);

            OpenedCalled = true;
        }

        protected virtual void AssertClosing(ConnectionEventData eventData)
        {
            Assert.NotNull(eventData.Context);
            Assert.NotEqual(default, eventData.ConnectionId);
            Assert.NotEqual(default, eventData.Context!.ContextId);

            Context = eventData.Context;
            ContextId = Context.ContextId;
            ConnectionId = eventData.ConnectionId;
            ClosingCalled = true;
        }

        protected virtual void AssertClosed(ConnectionEndEventData eventData)
        {
            Assert.Same(Context, eventData.Context);
            Assert.Equal(ConnectionId, eventData.ConnectionId);
            Assert.Equal(ContextId, eventData.Context!.ContextId);

            ClosedCalled = true;
        }

        protected virtual void AssertFailed(ConnectionErrorEventData eventData)
        {
            Assert.Same(Context, eventData.Context);
            Assert.Equal(ConnectionId, eventData.ConnectionId);
            Assert.Equal(ContextId, eventData.Context!.ContextId);
            Assert.NotNull(eventData.Exception);

            Exception = eventData.Exception;
            FailedCalled = true;
        }
    }

    private static void AssertNormalOpen(DbContext context, ConnectionInterceptor interceptor, bool async)
    {
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.True(interceptor.OpeningCalled);
        Assert.True(interceptor.OpenedCalled);
        Assert.False(interceptor.ClosedCalled);
        Assert.False(interceptor.ClosingCalled);
        Assert.False(interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
    }

    private static void AssertNormalClose(DbContext context, ConnectionInterceptor interceptor, bool async)
    {
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.False(interceptor.OpeningCalled);
        Assert.False(interceptor.OpenedCalled);
        Assert.True(interceptor.ClosedCalled);
        Assert.True(interceptor.ClosingCalled);
        Assert.False(interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
    }

    private static void AssertErrorOnOpen(DbContext context, ConnectionInterceptor interceptor, bool async)
    {
        Assert.Equal(async, interceptor.AsyncCalled);
        Assert.NotEqual(async, interceptor.SyncCalled);
        Assert.NotEqual(interceptor.AsyncCalled, interceptor.SyncCalled);
        Assert.True(interceptor.OpeningCalled);
        Assert.False(interceptor.OpenedCalled);
        Assert.False(interceptor.ClosedCalled);
        Assert.False(interceptor.ClosingCalled);
        Assert.True(interceptor.FailedCalled);
        Assert.Same(context, interceptor.Context);
    }

    private static void AsertOpenCloseEvents(ITestDiagnosticListener listener)
        => listener.AssertEventsInOrder(
            RelationalEventId.ConnectionOpening.Name,
            RelationalEventId.ConnectionOpened.Name,
            RelationalEventId.ConnectionClosing.Name,
            RelationalEventId.ConnectionClosed.Name);
}
