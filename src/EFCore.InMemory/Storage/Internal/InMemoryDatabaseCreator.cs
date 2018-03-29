// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InMemoryDatabaseCreator : IDatabaseCreator
    {
        private readonly StateManagerDependencies _stateManagerDependencies;
        private readonly IInMemoryDatabase _database;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InMemoryDatabaseCreator([NotNull] StateManagerDependencies stateManagerDependencies, [NotNull] IInMemoryDatabase database)
        {
            Check.NotNull(stateManagerDependencies, nameof(stateManagerDependencies));
            Check.NotNull(database, nameof(database));

            _stateManagerDependencies = stateManagerDependencies;
            _database = database;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool EnsureDeleted()
            => _database.Store.Clear();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(EnsureDeleted());

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool EnsureCreated() => _database.EnsureDatabaseCreated(_stateManagerDependencies);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default)
            => Task.FromResult(_database.EnsureDatabaseCreated(_stateManagerDependencies));
    }
}
