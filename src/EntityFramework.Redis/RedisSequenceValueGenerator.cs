// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Redis.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisSequenceValueGenerator : BlockOfSequentialValuesGenerator
    {
        public RedisSequenceValueGenerator([NotNull] string sequenceName, int blockSize)
            : base(sequenceName, blockSize)
        {
        }

        protected override long GetNewCurrentValue(IProperty property, LazyRef<DataStoreServices> dataStoreServices)
        {
            Check.NotNull(property, "property");
            Check.NotNull(dataStoreServices, "dataStoreServices");

            var database = (RedisDatabase)dataStoreServices.Value.Database;
            return database.GetNextGeneratedValue(property, BlockSize, SequenceName);
        }

        protected override async Task<long> GetNewCurrentValueAsync(
            IProperty property, LazyRef<DataStoreServices> dataStoreServices, CancellationToken cancellationToken)
        {
            Check.NotNull(property, "property");
            Check.NotNull(dataStoreServices, "dataStoreServices");

            cancellationToken.ThrowIfCancellationRequested();

            var database = (RedisDatabase)dataStoreServices.Value.Database;

            return
                await database.GetNextGeneratedValueAsync(
                    property, BlockSize, SequenceName, cancellationToken)
                    .WithCurrentCulture();
        }
    }
}
