// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Redis.Extensions;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisDataStoreSource : DataStoreSource<RedisDataStoreServices, RedisOptionsExtension>
    {
        public RedisDataStoreSource([NotNullAttribute]DbContextServices services, [NotNullAttribute]DbContextService<IDbContextOptions> options)
            : base(services, options)
        {
        }

        public override string Name
        {
            get { return typeof(RedisDataStore).Name; }
        }

        public override void AutoConfigure()
        {
            ContextOptions.UseRedis();
        }
    }
}
