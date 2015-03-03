// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Redis;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Framework.DependencyInjection
{
    public static class RedisEntityServicesBuilderExtensions
    {
        public static EntityServicesBuilder AddRedis([NotNull] this EntityServicesBuilder builder)
        {
            Check.NotNull(builder, "builder");

            builder.ServiceCollection
                .AddScoped<DataStoreSource, RedisDataStoreSource>()
                .TryAdd(new ServiceCollection()
                    .AddScoped<RedisValueGeneratorSelector>()
                    .AddScoped<RedisValueGeneratorCache>()                
                    .AddScoped<RedisDataStoreServices>()
                    .AddScoped<RedisDataStore>()
                    .AddScoped<RedisConnection>()
                    .AddScoped<RedisDatabase>()
                    .AddScoped<RedisValueGeneratorFactory>()
                    .AddScoped<RedisDataStoreCreator>());

            return builder;
        }
    }
}
