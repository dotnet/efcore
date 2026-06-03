// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Microsoft.Data.Sqlite
{
    internal class SqliteConnectionPool
    {
        private static readonly Random _random = new();

        private readonly SqliteConnectionStringBuilder _connectionOptions;
        private readonly List<SqliteConnectionInternal> _connections = [];
        private readonly Stack<SqliteConnectionInternal> _warmPool = new();
        private readonly Stack<SqliteConnectionInternal> _coldPool = new();

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
                lock (_connections)
                {
                    if (!TryPop(_warmPool, out connection)
                        && !TryPop(_coldPool, out connection)
                        && (Count % 2 == 1 || !ReclaimLeakedConnections()))
                    {
                        connection = new SqliteConnectionInternal(_connectionOptions, this);

                        _connections.Add(connection);
                    }
                }
            }
            while (connection == null);

            return connection;
        }

        private static bool TryPop(Stack<SqliteConnectionInternal> stack, out SqliteConnectionInternal? connection)
        {
#if NET5_0_OR_GREATER
            return stack.TryPop(out connection);
#else
            if (stack.Count > 0)
            {
                connection = stack.Pop();
                return true;
            }

            connection = null;
            return false;
#endif
        }

        public void Return(SqliteConnectionInternal connection)
        {
            lock (_connections)
            {
                connection.Deactivate();

                if (_state != State.Disabled
                    && connection.CanBePooled)
                {
                    _warmPool.Push(connection);
                }
                else
                {
                    DisposeConnection(connection);
                }
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

                while (TryPop(_warmPool, out var connection))
                {
                    DisposeConnection(connection!);
                }

                while (TryPop(_coldPool, out var connection))
                {
                    DisposeConnection(connection!);
                }

                ReclaimLeakedConnections();
            }
        }

        private void PruneCallback(object? _)
        {
            lock (_connections)
            {
                while (TryPop(_coldPool, out var connection))
                {
                    DisposeConnection(connection!);
                }

                while (TryPop(_warmPool, out var connection))
                {
                    _coldPool.Push(connection!);
                }
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

            List<SqliteConnectionInternal> leakedConnections;
            lock (_connections)
            {
                leakedConnections = _connections.Where(c => c.Leaked).ToList();
            }

            foreach (var connection in leakedConnections)
            {
                leakedConnectionsFound = true;

                Return(connection);
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
