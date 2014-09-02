// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Redis.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Redis.Query
{
    public class RedisQueryContext : QueryContext
    {
        protected RedisDatabase _redisDatabase;

        public RedisQueryContext(
            [NotNull] IModel model,
            [NotNull] ILogger logger,
            [NotNull] IMaterializationStrategy materializationStrategy,
            [NotNull] RedisDatabase redisDatabase)
            : base(model, logger, materializationStrategy)
        {
            Check.NotNull(redisDatabase, "redisDatabase");
            _redisDatabase = redisDatabase;
        }

        public virtual IEnumerable<TResult> GetResultsFromRedis<TResult>(
            [NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, "entityType");

            return _redisDatabase.GetMaterializedResults<TResult>(entityType, MaterializationStrategy);
        }

        public virtual IEnumerable<object[]> GetResultsFromRedis([NotNull] RedisQuery redisQuery)
        {
            Check.NotNull(redisQuery, "redisQuery");

            return _redisDatabase.GetResults(redisQuery);
        }
    }
}
