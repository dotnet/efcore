// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class TestStore : IDisposable
    {
        private static readonly HashSet<string> _createdDatabases = new HashSet<string>();

        private static readonly ConcurrentDictionary<string, object> _creationLocks
            = new ConcurrentDictionary<string, object>();

        private static readonly object _hashSetLock = new object();
        private readonly Func<DbContextOptions, DbContext> _createContext;
        private readonly Func<DbContextOptionsBuilder, DbContextOptionsBuilder> _addOptions;
        private DbContextOptions _options;
        
        protected DbContextOptions Options => _options ?? (_options = CreateOptions());
        protected IServiceProvider ServiceProvider { get; }
        public string Name { get; protected set; }

        protected TestStore(
            string name,
            IServiceProvider serviceProvider,
            Func<DbContextOptionsBuilder, DbContextOptionsBuilder> addOptions,
            Func<DbContextOptions, DbContext> createContext)
        {
            Name = name;
            ServiceProvider = serviceProvider;
            _addOptions = addOptions ?? (b => b);
            _createContext = createContext;
        }

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

                        _creationLocks.TryRemove(name, out _);
                    }
                }
            }
        }

        protected DbContextOptions CreateOptions()
            => _addOptions(AddProviderOptions(new DbContextOptionsBuilder())).Options;

        protected abstract DbContextOptionsBuilder AddProviderOptions(DbContextOptionsBuilder builder);

        public virtual DbContext CreateContext() => _createContext(Options);

        public virtual void Dispose()
        {
        }
    }
}
