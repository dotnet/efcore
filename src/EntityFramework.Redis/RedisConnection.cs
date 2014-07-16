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
        private readonly string _hostName;
        private readonly int _port;
        private readonly int _database = -1;

        /// <summary>
        ///     For testing. Improper usage may lead to NullReference exceptions
        /// </summary>
        internal RedisConnection()
        {
        }

        public RedisConnection([NotNull] DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");
            var optionsExtension = RedisOptionsExtension.Extract(configuration);

            _hostName = optionsExtension.HostName;
            _port = optionsExtension.Port;
            _database = optionsExtension.Database;
        }

        public virtual string ConnectionString
        {
            get { return _hostName + ":" + _port; }
        }

        public virtual int Database
        {
            get { return _database; }
        }
    }
}
