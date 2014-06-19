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
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStore
    {
        private readonly ILogger _logger;
        private readonly IModel _model;

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

            _logger = configuration.LoggerFactory.Create(GetType().Name);
            _model = configuration.Model;
        }

        public virtual ILogger Logger
        {
            get { return _logger; }
        }

        public virtual IModel Model
        {
            get { return _model; }
        }

        public abstract Task<int> SaveChangesAsync(
            [NotNull] IReadOnlyList<StateEntry> stateEntries,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract IEnumerable<TResult> Query<TResult>(
            [NotNull] QueryModel queryModel,
            [NotNull] StateManager stateManager);

        public abstract IAsyncEnumerable<TResult> AsyncQuery<TResult>(
            [NotNull] QueryModel queryModel,
            [NotNull] StateManager stateManager);
    }
}
