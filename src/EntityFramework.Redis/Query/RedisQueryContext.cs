// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Redis.Query
{
    public class RedisQueryContext : QueryContext
    {
        protected RedisDataStore _redisDataStore;

        public RedisQueryContext(
            [NotNull] ILogger logger,
            [NotNull] IQueryBuffer queryBuffer,
            [NotNull] RedisDataStore redisDataStore)
            : base(
                Check.NotNull(logger, "logger"),
                Check.NotNull(queryBuffer, "queryBuffer"))
        {
            Check.NotNull(redisDataStore, "redisDatabase");

            _redisDataStore = redisDataStore;
        }

        public virtual IEnumerable<object[]> GetResultsEnumerable([NotNull] RedisQuery redisQuery)
        {
            Check.NotNull(redisQuery, "redisQuery");

            return _redisDataStore.GetResultsEnumerable(redisQuery);
        }

        public virtual IAsyncEnumerable<object[]> GetResultsAsyncEnumerable([NotNull] RedisQuery redisQuery)
        {
            Check.NotNull(redisQuery, "redisQuery");

            return _redisDataStore.GetResultsAsyncEnumerable(redisQuery);
        }
    }
}
