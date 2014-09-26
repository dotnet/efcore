// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Utilities;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisSequenceValueGenerator : BlockOfSequentialValuesGenerator
    {
        private RedisDatabase _redisDatabase;

        public RedisSequenceValueGenerator(
            [NotNull] RedisDatabase redisDatabase,
            [NotNull] string sequenceName,
            int blockSize)
            : base(sequenceName, blockSize)
        {
            Check.NotNull(redisDatabase, "redisDatabase");

            _redisDatabase = redisDatabase;
        }

        public override long GetNewCurrentValue(StateEntry stateEntry, IProperty property)
        {
            return _redisDatabase.GetNextGeneratedValue(property, BlockSize, SequenceName);
        }

        public override async Task<long> GetNewCurrentValueAsync(
            StateEntry stateEntry, IProperty property, CancellationToken cancellationToken)
        {
            return await 
                Task.FromResult<long>(_redisDatabase.GetNextGeneratedValue(property, BlockSize, SequenceName))
                .ConfigureAwait(continueOnCapturedContext: false);
        }
    }
}
