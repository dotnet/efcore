// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Data.Sqlite
{
    internal class SqliteConnectionPoolGroup
    {
        private SqliteConnectionPool? _pool;
        private State _state = State.Active;

        public SqliteConnectionPoolGroup(SqliteConnectionStringBuilder connectionOptions, string connectionString, bool isNonPooled)
        {
            ConnectionOptions = connectionOptions;
            ConnectionString = connectionString;
            IsNonPooled = isNonPooled;
        }

        public SqliteConnectionStringBuilder ConnectionOptions { get; }
        public string ConnectionString { get; }
        public bool IsNonPooled { get; }

        public bool IsDisabled
            => _state == State.Disabled;

        public SqliteConnectionPool? GetPool()
        {
            if (IsNonPooled)
            {
                lock (this)
                {
                    KeepAlive();
                }

                return null;
            }

            if (_pool == null)
            {
                lock (this)
                {
                    if (_pool == null
                        && KeepAlive())
                    {
                        _pool = new SqliteConnectionPool(ConnectionOptions);
                    }
                }
            }

            return _pool;
        }

        public bool Clear()
        {
            lock (this)
            {
                if (_pool != null)
                {
                    SqliteConnectionFactory.Instance.ReleasePool(_pool, clearing: true);
                    _pool = null;
                }
            }

            return _pool != null;
        }

        public bool Prune()
        {
            lock (this)
            {
                if (_pool?.Count == 0)
                {
                    SqliteConnectionFactory.Instance.ReleasePool(_pool, clearing: false);
                    _pool = null;
                }

                if (_pool == null)
                {
                    if (_state == State.Active)
                    {
                        _state = State.Idle;
                    }
                    else if (_state == State.Idle)
                    {
                        _state = State.Disabled;
                    }
                }

                return _state == State.Disabled;
            }
        }

        private bool KeepAlive()
        {
            if (_state == State.Idle)
            {
                _state = State.Active;
            }

            return _state == State.Active;
        }

        private enum State
        {
            Active,
            Idle,
            Disabled
        }
    }
}
