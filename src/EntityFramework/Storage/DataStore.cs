// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStore
    {
        private readonly DbContextConfiguration _configuration;
        private readonly ILogger _logger;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected DataStore()
        {
        }

        protected DataStore([NotNull] DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            _configuration = configuration;
            _logger = configuration.LoggerFactory.Create(GetType().Name);
        }

        public virtual ILogger Logger
        {
            get { return _logger; }
        }

        public virtual IModel Model
        {
            get { return _configuration.Model; }
        }

        public virtual StateManager StateManager
        {
            get { return _configuration.StateManager; }
        }

        protected virtual IQueryBuffer CreateQueryBuffer()
        {
            return new StateEntryQueryBuffer(
                StateManager,
                _configuration.Services.EntityKeyFactorySource,
                _configuration.Services.StateEntryFactory);
        }

        public abstract Task<int> SaveChangesAsync(
            [NotNull] IReadOnlyList<StateEntry> stateEntries,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract IEnumerable<TResult> Query<TResult>([NotNull] QueryModel queryModel);

        public abstract IAsyncEnumerable<TResult> AsyncQuery<TResult>(
            [NotNull] QueryModel queryModel,
            CancellationToken cancellationToken);
    }
}
