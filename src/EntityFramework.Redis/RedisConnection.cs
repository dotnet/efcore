// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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
        private readonly string _connectionString;
        private readonly int _database;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected RedisConnection()
        {
        }

        public RedisConnection([NotNull] LazyRef<IDbContextOptions> options, [NotNull] ILoggerFactory loggerFactory)
            : base(loggerFactory)
        {
            Check.NotNull(options, "options");

            var extracted = RedisOptionsExtension.Extract(options.Value);
            _connectionString = extracted.HostName + ":" + extracted.Port;
            _database = extracted.Database;
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
