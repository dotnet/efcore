// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Redis;
using Microsoft.Data.Entity.Redis.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Data.Entity
{
    public static class RedisDatabaseExtensions
    {
        public static RedisDatabase AsRedis([NotNull] this Database database)
        {
            Check.NotNull(database, "database");

            var sqliteDatabase = database as RedisDatabase;

            if (sqliteDatabase == null)
            {
                throw new InvalidOperationException(Strings.RedisNotInUse);
            }

            return sqliteDatabase;
        }
    }
}
