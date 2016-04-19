// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class TestStore : IDisposable
    {
        private static readonly HashSet<string> _createdDatabases = new HashSet<string>();

        private static readonly ConcurrentDictionary<string, object> _creationLocks
            = new ConcurrentDictionary<string, object>();

        private static readonly object _hashSetLock = new object();

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

                        lock (_hashSetLock)
                        {
                            _createdDatabases.Add(name);
                        }

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
