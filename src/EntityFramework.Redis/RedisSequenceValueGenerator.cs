// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisSequenceValueGenerator : BlockOfSequentialValuesGenerator
    {
        private readonly RedisDatabase _redisDatabase;

        public RedisSequenceValueGenerator(
            [NotNull] RedisDatabase redisDatabase,
            [NotNull] string sequenceName,
            int blockSize)
            : base(sequenceName, blockSize)
        {
            Check.NotNull(redisDatabase, "redisDatabase");

            _redisDatabase = redisDatabase;
        }

        protected override long GetNewCurrentValue([NotNull]IProperty property, [NotNull]DbContextService<DataStoreServices> dataStoreServices)
        {
            return _redisDatabase.GetNextGeneratedValue(property, BlockSize, SequenceName);
        }

        protected override async Task<long> GetNewCurrentValueAsync([NotNull]IProperty property, [NotNull]DbContextService<DataStoreServices> dataStoreServices, CancellationToken cancellationToken)
        {
            Check.NotNull(dataStoreServices, "stateEntry");
            Check.NotNull(property, "property");

            cancellationToken.ThrowIfCancellationRequested();

            return
                await _redisDatabase.GetNextGeneratedValueAsync(
                    property, BlockSize, SequenceName, cancellationToken)
                    .WithCurrentCulture();
        }
    }
}
