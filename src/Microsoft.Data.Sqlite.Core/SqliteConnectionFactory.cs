// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.Data.Sqlite
{
    internal class SqliteConnectionFactory
    {
        public static readonly SqliteConnectionFactory Instance = new();

#pragma warning disable IDE0052 // Remove unread private members
        private readonly Timer _pruneTimer;
#pragma warning restore IDE0052 // Remove unread private members
        private readonly List<SqliteConnectionPoolGroup> _idlePoolGroups = [];
        private readonly List<SqliteConnectionPool> _poolsToRelease = [];
        private readonly ReaderWriterLockSlim _lock = new();

        private Dictionary<string, SqliteConnectionPoolGroup> _poolGroups = new();

        protected SqliteConnectionFactory()
        {
            AppDomain.CurrentDomain.DomainUnload += (_, _) => ClearPools();
            AppDomain.CurrentDomain.ProcessExit += (_, _) => ClearPools();

            _pruneTimer = new Timer(PruneCallback, null, TimeSpan.FromMinutes(4), TimeSpan.FromSeconds(30));
        }

        public SqliteConnectionInternal GetConnection(SqliteConnection outerConnection)
        {
            var poolGroup = outerConnection.PoolGroup;
            if (poolGroup is { IsDisabled: true, IsNonPooled: false })
            {
                poolGroup = GetPoolGroup(poolGroup.ConnectionString);
                outerConnection.PoolGroup = poolGroup;
            }

            var pool = poolGroup.GetPool();

            var connection = pool == null
                ? new SqliteConnectionInternal(outerConnection.ConnectionOptions)
                : pool.GetConnection();
            connection.Activate(outerConnection);

            return connection;
        }

        public SqliteConnectionPoolGroup GetPoolGroup(string connectionString)
        {
            _lock.EnterUpgradeableReadLock();

            try
            {
                if (!_poolGroups.TryGetValue(connectionString, out var poolGroup)
                    || (poolGroup.IsDisabled
                        && !poolGroup.IsNonPooled))
                {
                    var connectionOptions = new SqliteConnectionStringBuilder(connectionString);

                    _lock.EnterWriteLock();

                    try
                    {
                        if (!_poolGroups.TryGetValue(connectionString, out poolGroup))
                        {
                            var isNonPooled = connectionOptions.DataSource == ":memory:"
                                || connectionOptions.Mode == SqliteOpenMode.Memory
                                || connectionOptions.DataSource.Length == 0
                                || !connectionOptions.Pooling;

                            poolGroup = new SqliteConnectionPoolGroup(connectionOptions, connectionString, isNonPooled);
                            _poolGroups.Add(connectionString, poolGroup);
                        }
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }

                return poolGroup;
            }
            finally
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public void ReleasePool(SqliteConnectionPool pool, bool clearing)
        {
            pool.Shutdown();

            lock (_poolsToRelease)
            {
                if (clearing)
                {
                    pool.Clear();
                }

                _poolsToRelease.Add(pool);
            }
        }

        public void ClearPools()
        {
            _lock.EnterWriteLock();

            try
            {
                foreach (var entry in _poolGroups)
                {
                    entry.Value.Clear();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void PruneCallback(object? _)
        {
            lock (_poolsToRelease)
            {
                for (var i = _poolsToRelease.Count - 1; i >= 0; i--)
                {
                    var pool = _poolsToRelease[i];

                    pool.Clear();

                    if (pool.Count == 0)
                    {
                        _poolsToRelease.Remove(pool);
                    }
                }
            }

            _lock.EnterWriteLock();

            try
            {

                for (var i = _idlePoolGroups.Count - 1; i >= 0; i--)
                {
                    var poolGroup = _idlePoolGroups[i];

                    if (!poolGroup.Clear())
                    {
                        _idlePoolGroups.Remove(poolGroup);
                    }
                }

                var activePoolGroups = new Dictionary<string, SqliteConnectionPoolGroup>();
                foreach (var entry in _poolGroups)
                {
                    var poolGroup = entry.Value;

                    if (poolGroup.Prune())
                    {
                        _idlePoolGroups.Add(poolGroup);
                    }
                    else
                    {
                        activePoolGroups.Add(entry.Key, poolGroup);
                    }
                }

                _poolGroups = activePoolGroups;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
