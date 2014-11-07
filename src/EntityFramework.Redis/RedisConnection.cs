// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Redis.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisConnection : DataStoreConnection
    {
        private readonly LazyRef<string> _connectionString;
        private readonly LazyRef<int> _database;
        private readonly LazyRef<RedisOptionsExtension> _options;

        /// <summary>
        ///     For testing. Improper usage may lead to NullReference exceptions
        /// </summary>
        protected RedisConnection()
        {
        }

        public RedisConnection([NotNull] DbContextConfiguration configuration, [NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            Check.NotNull(configuration, "configuration");

            // TODO: Decouple from DbContextConfiguration (Issue #641)
            _options = new LazyRef<RedisOptionsExtension>(() => RedisOptionsExtension.Extract(configuration));
            _connectionString = new LazyRef<string>(() => _options.Value.HostName + ":" + _options.Value.Port);
            _database = new LazyRef<int>(() => _options.Value.Database);
        }

        public virtual string ConnectionString
        {
            get { return _connectionString.Value; }
        }

        public virtual int Database
        {
            get { return _database.Value; }
        }
    }
}
