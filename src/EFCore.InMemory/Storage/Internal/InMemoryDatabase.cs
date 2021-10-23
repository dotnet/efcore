// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.InMemory.Storage.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    /// <remarks>
    ///     The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///     <see cref="DbContext" /> instance will use its own instance of this service.
    ///     The implementation may depend on other services registered with any lifetime.
    ///     The implementation does not need to be thread-safe.
    /// </remarks>
    public class InMemoryDatabase : Database, IInMemoryDatabase
    {
        private readonly IInMemoryStore _store;
        private readonly IUpdateAdapterFactory _updateAdapterFactory;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Update> _updateLogger;
        private readonly IDesignTimeModel _designTimeModel;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public InMemoryDatabase(
            DatabaseDependencies dependencies,
            IInMemoryStoreCache storeCache,
            IDbContextOptions options,
            IDesignTimeModel designTimeModel,
            IUpdateAdapterFactory updateAdapterFactory,
            IDiagnosticsLogger<DbLoggerCategory.Update> updateLogger)
            : base(dependencies)
        {
            Check.NotNull(storeCache, nameof(storeCache));
            Check.NotNull(options, nameof(options));
            Check.NotNull(updateAdapterFactory, nameof(updateAdapterFactory));
            Check.NotNull(updateLogger, nameof(updateLogger));

            _store = storeCache.GetStore(options);
            _designTimeModel = designTimeModel;
            _updateAdapterFactory = updateAdapterFactory;
            _updateLogger = updateLogger;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IInMemoryStore Store
            => _store;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override int SaveChanges(IList<IUpdateEntry> entries)
            => _store.ExecuteTransaction(Check.NotNull(entries, nameof(entries)), _updateLogger);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Task<int> SaveChangesAsync(
            IList<IUpdateEntry> entries,
            CancellationToken cancellationToken = default)
            => Task.FromResult(_store.ExecuteTransaction(Check.NotNull(entries, nameof(entries)), _updateLogger));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool EnsureDatabaseCreated()
            => _store.EnsureCreated(_updateAdapterFactory, _designTimeModel.Model, _updateLogger);
    }
}
