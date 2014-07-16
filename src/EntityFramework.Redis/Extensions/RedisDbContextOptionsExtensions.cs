// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Redis.Utilities;

namespace Microsoft.Data.Entity.Redis.Extensions
{
    public static class RedisDbContextOptionsExtensions
    {
        public static DbContextOptions UseRedis([NotNull] this DbContextOptions options,
            [NotNull] string hostName = "127.0.0.1", int port = 6379, int database = 0)
        {
            Check.NotNull(options, "options");

            ((IDbContextOptionsExtensions)options).AddOrUpdateExtension<RedisOptionsExtension>(
                optionsExtension =>
                    {
                        optionsExtension.HostName = hostName;
                        optionsExtension.Port = port;
                        optionsExtension.Database = database;
                    });

            return options;
        }
    }
}
