// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class TestStore : IDisposable
    {
        private static readonly HashSet<string> _createdDatabases = new HashSet<string>();

        private static readonly ConcurrentDictionary<string, AsyncLock> _creationLocks
            = new ConcurrentDictionary<string, AsyncLock>();

        protected virtual async Task CreateSharedAsync(string name, Func<Task> initializeDatabase)
        {
            if (!_createdDatabases.Contains(name))
            {
                var asyncLock = _creationLocks.GetOrAdd(name, new AsyncLock());

                using (await asyncLock.LockAsync().WithCurrentCulture())
                {
                    if (!_createdDatabases.Contains(name))
                    {
                        await initializeDatabase().WithCurrentCulture();

                        _createdDatabases.Add(name);

                        AsyncLock _;
                        _creationLocks.TryRemove(name, out _);
                    }
                }
            }
        }

        public virtual void Dispose()
        {
        }
    }
}
