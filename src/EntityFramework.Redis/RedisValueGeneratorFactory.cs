// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisValueGeneratorFactory : IValueGeneratorFactory
    {
        public const int DefaultBlockSize = 10;

        private readonly RedisDatabase _redisDatabase;

        public RedisValueGeneratorFactory([NotNull] RedisDatabase redisDatabase)
        {
            Check.NotNull(redisDatabase, "redisDatabase");

            _redisDatabase = redisDatabase;
        }

        IValueGenerator IValueGeneratorFactory.Create(IProperty property)
        {
            return new RedisSequenceValueGenerator(_redisDatabase, GetSequenceName(property), GetBlockSize(property));
        }

        // TODO: investigate how to make pool size configurable
        int IValueGeneratorFactory.GetPoolSize(IProperty property)
        {
            Check.NotNull(property, "property");

            return 1;
        }

        string IValueGeneratorFactory.GetCacheKey(IProperty property)
        {
            Check.NotNull(property, "property");

            return GetSequenceName(property);
        }

        // TODO: investigate how to make block size configurable
        public virtual int GetBlockSize([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return DefaultBlockSize;
        }

        public virtual string GetSequenceName([NotNull] IProperty property)
        {
            Check.NotNull(property, "property");

            return RedisDatabase.ConstructRedisValueGeneratorKeyName(property);
        }
    }
}
