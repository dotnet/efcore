// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Concurrent;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class TestStoreIndex
{
    private readonly HashSet<string> _createdDatabases = [];
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _creationLocks = new();
    private readonly object _hashSetLock = new();

    public virtual async Task CreateSharedAsync(string name, Func<Task> initializeDatabase)
    {
        // ReSharper disable once InconsistentlySynchronizedField
        if (!_createdDatabases.Contains(name))
        {
            var creationLock = _creationLocks.GetOrAdd(name, new SemaphoreSlim(1, 1));
            await creationLock.WaitAsync();
            try
            {
                if (!_createdDatabases.Contains(name))
                {
                    await initializeDatabase();

                    lock (_hashSetLock)
                    {
                        _createdDatabases.Add(name);
                    }
                }
            }
            finally
            {
                creationLock.Release();
            }
        }
    }

    public virtual void RemoveShared(string name)
    {
        lock (_hashSetLock)
        {
            _createdDatabases.Remove(name);
        }
    }

    public virtual Task CreateNonSharedAsync(string name, Func<Task> initializeDatabase)
        => initializeDatabase();
}
