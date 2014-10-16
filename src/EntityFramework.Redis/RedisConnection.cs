// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Redis.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisConnection : DataStoreConnection
    {
        private readonly string _connectionString;
        private readonly int _database = -1;

        /// <summary>
        ///     For testing. Improper usage may lead to NullReference exceptions
        /// </summary>
        protected RedisConnection()
        {
        }

        public RedisConnection([NotNull] DbContextConfiguration configuration)
            : base(configuration.LoggerFactory)
        {
            Check.NotNull(configuration, "configuration");
            var optionsExtension = RedisOptionsExtension.Extract(configuration);

            _connectionString = optionsExtension.HostName + ":" + optionsExtension.Port;
            _database = optionsExtension.Database;
        }

        public virtual string ConnectionString
        {
            get { return _connectionString; }
        }

        public virtual int Database
        {
            get { return _database; }
        }
    }
}
