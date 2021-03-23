// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Microsoft.Data.Sqlite
{
    internal class SqliteConnectionPool
    {
        private static readonly Random _random = new();

        private readonly SqliteConnectionStringBuilder _connectionOptions;
        private readonly List<SqliteConnectionInternal> _connections = new();
        private readonly ConcurrentStack<SqliteConnectionInternal> _warmPool = new();
        private readonly ConcurrentStack<SqliteConnectionInternal> _coldPool = new();
        private readonly Semaphore _poolSemaphore = new(0, int.MaxValue);

        private Timer? _pruneTimer;
        private State _state = State.Active;

        public SqliteConnectionPool(SqliteConnectionStringBuilder connectionOptions)
        {
            lock (_random)
            {
                // 2-4 minutes in 10 second intervals
                var prunePeriod = TimeSpan.FromSeconds(_random.Next(2 * 6, 4 * 6) * 10);
                _pruneTimer = new Timer(PruneCallback, null, prunePeriod, prunePeriod);
            }

            _connectionOptions = connectionOptions;
        }

        public int Count
            => _connections.Count;

        public void Shutdown()
        {
            _state = State.Disabled;

            _pruneTimer?.Dispose();
            _pruneTimer = null;
        }

        public SqliteConnectionInternal GetConnection()
        {
            SqliteConnectionInternal? connection = null;
            do
            {
                if (_poolSemaphore.WaitOne(0))
                {
                    if (!_warmPool.TryPop(out connection)
                        && !_coldPool.TryPop(out connection))
                    {
                        Debug.Fail("Inconceivable!");
                    }
                }
                else if (Count % 2 == 1 || !ReclaimLeakedConnections())
                {
                    connection = new SqliteConnectionInternal(_connectionOptions, this);

                    lock (_connections)
                    {
                        _connections.Add(connection);
                    }
                }
            }
            while (connection == null);

            return connection;
        }

        public void Return(SqliteConnectionInternal connection)
        {
            connection.Deactivate();

            if (_state != State.Disabled
                && connection.CanBePooled)
            {
                _warmPool.Push(connection);
                _poolSemaphore.Release();
            }
            else
            {
                DisposeConnection(connection);
            }
        }

        public void Clear()
        {
            lock (_connections)
            {
                foreach (var connection in _connections)
                {
                    connection.DoNotPool();
                }
            }

            while (_warmPool.TryPop(out var connection))
            {
                DisposeConnection(connection);
            }

            while (_coldPool.TryPop(out var connection))
            {
                DisposeConnection(connection);
            }

            ReclaimLeakedConnections();
        }

        private void PruneCallback(object? _)
        {
            while (Count > 0)
            {
                if (!_poolSemaphore.WaitOne(0))
                {
                    break;
                }

                if (_coldPool.TryPop(out var connection))
                {
                    DisposeConnection(connection);
                }
                else
                {
                    _poolSemaphore.Release();
                    break;
                }
            }

            if (_poolSemaphore.WaitOne(0))
            {
                while (_warmPool.TryPop(out var connection))
                {
                    _coldPool.Push(connection);
                }

                _poolSemaphore.Release();
            }
        }

        private void DisposeConnection(SqliteConnectionInternal connection)
        {
            lock (_connections)
            {
                _connections.Remove(connection);
            }

            connection.Dispose();
        }

        private bool ReclaimLeakedConnections()
        {
            var leakedConnectionsFound = false;

            lock (_connections)
            {
                foreach (var connection in _connections)
                {
                    if (connection.Leaked)
                    {
                        Return(connection);

                        leakedConnectionsFound = true;
                    }
                }
            }

            return leakedConnectionsFound;
        }

        private enum State
        {
            Active,
            Disabled
        }
    }
}
