// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStore
    {
        private readonly StateManager _stateManager;
        private readonly LazyRef<IModel> _model;
        private readonly EntityKeyFactorySource _entityKeyFactorySource;
        private readonly EntityMaterializerSource _entityMaterializerSource;
        private readonly ClrCollectionAccessorSource _collectionAccessorSource;
        private readonly ClrPropertySetterSource _propertySetterSource;
        private readonly LazyRef<ILogger> _logger;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected DataStore()
        {
        }

        protected DataStore(
            [NotNull] StateManager stateManager,
            [NotNull] LazyRef<IModel> model,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] EntityMaterializerSource entityMaterializerSource,
            [NotNull] ClrCollectionAccessorSource collectionAccessorSource,
            [NotNull] ClrPropertySetterSource propertySetterSource,
            [NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(stateManager, "stateManager");
            Check.NotNull(model, "model");
            Check.NotNull(entityKeyFactorySource, "entityKeyFactorySource");
            Check.NotNull(entityMaterializerSource, "entityMaterializerSource");
            Check.NotNull(collectionAccessorSource, "collectionAccessorSource");
            Check.NotNull(propertySetterSource, "propertySetterSource");
            Check.NotNull(loggerFactory, "loggerFactory");

            _stateManager = stateManager;
            _model = model;
            _entityKeyFactorySource = entityKeyFactorySource;
            _entityMaterializerSource = entityMaterializerSource;
            _collectionAccessorSource = collectionAccessorSource;
            _propertySetterSource = propertySetterSource;
            _logger = new LazyRef<ILogger>(loggerFactory.Create<DataStore>);
        }

        public virtual ILogger Logger
        {
            get { return _logger.Value; }
        }

        public virtual IModel Model
        {
            get { return _model.Value; }
        }

        public virtual EntityKeyFactorySource EntityKeyFactorySource
        {
            get { return _entityKeyFactorySource; }
        }

        public virtual EntityMaterializerSource EntityMaterializerSource
        {
            get { return _entityMaterializerSource; }
        }

        protected virtual IQueryBuffer CreateQueryBuffer()
        {
            return new QueryBuffer(
                _stateManager,
                EntityKeyFactorySource,
                EntityMaterializerSource,
                _collectionAccessorSource,
                _propertySetterSource);
        }

        public abstract int SaveChanges(
            [NotNull] IReadOnlyList<StateEntry> stateEntries);

        public abstract Task<int> SaveChangesAsync(
            [NotNull] IReadOnlyList<StateEntry> stateEntries,
            CancellationToken cancellationToken = default(CancellationToken));

        public abstract IEnumerable<TResult> Query<TResult>([NotNull] QueryModel queryModel);

        public abstract IAsyncEnumerable<TResult> AsyncQuery<TResult>(
            [NotNull] QueryModel queryModel,
            CancellationToken cancellationToken);
    }
}
