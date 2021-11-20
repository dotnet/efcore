// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestStoreIndex
    {
        private readonly HashSet<string> _createdDatabases = new();
        private readonly ConcurrentDictionary<string, object> _creationLocks = new();
        private readonly object _hashSetLock = new();

        public virtual void CreateShared(string name, Action initializeDatabase)
        {
            // ReSharper disable once InconsistentlySynchronizedField
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

                        _creationLocks.TryRemove(name, out _);
                    }
                }
            }
        }

        public virtual void RemoveShared(string name)
            => _createdDatabases.Remove(name);

        public virtual void CreateNonShared(string name, Action initializeDatabase)
        {
            var creationLock = _creationLocks.GetOrAdd(name, new object());

            if (Monitor.TryEnter(creationLock))
            {
                try
                {
                    initializeDatabase?.Invoke();
                }
                finally
                {
                    Monitor.Exit(creationLock);
                    if (!_creationLocks.TryRemove(name, out _))
                    {
                        throw new InvalidOperationException(
                            $"An attempt was made to initialize a non-shared store {name} from two different threads.");
                    }
                }
            }
            else
            {
                _creationLocks.TryRemove(name, out _);
                throw new InvalidOperationException(
                    $"An attempt was made to initialize a non-shared store {name} from two different threads.");
            }
        }
    }
}
