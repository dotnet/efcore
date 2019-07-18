// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
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
            var (context, interceptor) = CreateContext<ConnectionInterceptor>();
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
            var (context, interceptor) = CreateContext<ConnectionOverridingInterceptor>();
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
            using (var context = CreateContext(
                new IInterceptor[]
                {
                    new NoOpConnectionInterceptor(), interceptor1, interceptor2
                },
                new IInterceptor[]
                {
                    interceptor3, interceptor4, new NoOpConnectionInterceptor()
                }))
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
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Intercept_connection_that_throws_on_open(bool async)
        {
            var interceptor = new ConnectionInterceptor();

            using (var context = CreateBadUniverse(new DbContextOptionsBuilder().AddInterceptors(interceptor)))
            {
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
        }

        protected abstract BadUniverseContext CreateBadUniverse(DbContextOptionsBuilder optionsBuilder);

        protected class BadUniverseContext : UniverseContext
        {
            public BadUniverseContext(DbContextOptions options)
                : base(options)
            {
            }
        }

        protected class NoOpConnectionInterceptor : DbConnectionInterceptor
        {
        }

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

            public override async Task<InterceptionResult> ConnectionOpeningAsync(
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
            public DbContext Context { get; set; }
            public Exception Exception { get; set; }
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

            public virtual Task<InterceptionResult> ConnectionOpeningAsync(
                DbConnection connection,
                ConnectionEventData eventData,
                InterceptionResult result,
                CancellationToken cancellationToken = default)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertOpening(eventData);

                return Task.FromResult(result);
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

            public virtual Task<InterceptionResult> ConnectionClosingAsync(
                DbConnection connection,
                ConnectionEventData eventData,
                InterceptionResult result)
            {
                Assert.True(eventData.IsAsync);
                AsyncCalled = true;
                AssertClosing(eventData);

                return Task.FromResult(result);
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
                Assert.NotEqual(default, eventData.Context.ContextId);

                Context = eventData.Context;
                ContextId = Context.ContextId;
                ConnectionId = eventData.ConnectionId;
                OpeningCalled = true;
            }

            protected virtual void AssertOpened(ConnectionEndEventData eventData)
            {
                Assert.Same(Context, eventData.Context);
                Assert.Equal(ConnectionId, eventData.ConnectionId);
                Assert.Equal(ContextId, eventData.Context.ContextId);

                OpenedCalled = true;
            }

            protected virtual void AssertClosing(ConnectionEventData eventData)
            {
                Assert.NotNull(eventData.Context);
                Assert.NotEqual(default, eventData.ConnectionId);
                Assert.NotEqual(default, eventData.Context.ContextId);

                Context = eventData.Context;
                ContextId = Context.ContextId;
                ConnectionId = eventData.ConnectionId;
                ClosingCalled = true;
            }

            protected virtual void AssertClosed(ConnectionEndEventData eventData)
            {
                Assert.Same(Context, eventData.Context);
                Assert.Equal(ConnectionId, eventData.ConnectionId);
                Assert.Equal(ContextId, eventData.Context.ContextId);

                ClosedCalled = true;
            }

            protected virtual void AssertFailed(ConnectionErrorEventData eventData)
            {
                Assert.Same(Context, eventData.Context);
                Assert.Equal(ConnectionId, eventData.ConnectionId);
                Assert.Equal(ContextId, eventData.Context.ContextId);
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
}
