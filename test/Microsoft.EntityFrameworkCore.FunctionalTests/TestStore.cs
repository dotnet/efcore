// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.Data.Entity.FunctionalTests
{
    public abstract class TestStore : IDisposable
    {
        private static readonly HashSet<string> _createdDatabases = new HashSet<string>();

        private static readonly ConcurrentDictionary<string, object> _creationLocks
            = new ConcurrentDictionary<string, object>();

        protected virtual void CreateShared(string name, Action initializeDatabase)
        {
            if (!_createdDatabases.Contains(name))
            {
                var creationLock = _creationLocks.GetOrAdd(name, new object());

                lock (creationLock)
                {
                    if (!_createdDatabases.Contains(name))
                    {
                        initializeDatabase?.Invoke();

                        _createdDatabases.Add(name);

                        object _;
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
