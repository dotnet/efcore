// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Redis.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Redis
{
    public class RedisOptionsExtension : DbContextOptionsExtension
    {
        public virtual string HostName { get; internal set; }

        public virtual int Port { get; internal set; }

        public virtual int Database { get; internal set; }

        public static RedisOptionsExtension Extract([NotNull] IDbContextOptions options)
        {
            Check.NotNull(options, "options");

            var redisOptionsExtensions = options.Extensions
                .OfType<RedisOptionsExtension>()
                .ToArray();

            if (redisOptionsExtensions.Length == 0)
            {
                throw new InvalidOperationException( /* TODO add message */);
            }

            if (redisOptionsExtensions.Length > 1)
            {
                throw new InvalidOperationException( /* TODO add message */);
            }

            return redisOptionsExtensions[0];
        }

        protected override void ApplyServices(EntityServicesBuilder builder)
        {
            builder.AddRedis();
        }
    }
}
