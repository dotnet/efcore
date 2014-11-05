// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.Redis.FunctionalTests
{
    public class RedisTestStore : TestStore
    {
        public Action CleanupAction { get; set; }

        public override void Dispose()
        {
            if (CleanupAction != null)
            {
                try
                {
                    CleanupAction();
                    CleanupAction = null;
                }
                catch (Exception)
                {
                }
            }
        }

        public static Task<RedisTestStore> GetOrCreateSharedAsync(string name, Func<Task> initializeDatabase)
        {
            return new RedisTestStore().CreateSharedAsync(name, initializeDatabase);
        }

        private new async Task<RedisTestStore> CreateSharedAsync(string name, Func<Task> initializeDatabase)
        {
            await base.CreateSharedAsync(typeof(RedisTestStore).Name + name, initializeDatabase);

            return this;
        }
    }
}
