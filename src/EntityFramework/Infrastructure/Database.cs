// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.Infrastructure
{
    public abstract class Database : IDatabaseInternals
    {
        private readonly LazyRef<IModel> _model;
        private readonly DataStoreCreator _dataStoreCreator;
        private readonly DataStoreConnection _connection;
        private readonly LazyRef<ILogger> _logger;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected Database()
        {
        }

        protected Database(
            [NotNull] LazyRef<IModel> model,
            [NotNull] DataStoreCreator dataStoreCreator,
            [NotNull] DataStoreConnection connection,
            [NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(model, "model");
            Check.NotNull(dataStoreCreator, "dataStoreCreator");
            Check.NotNull(connection, "connection");
            Check.NotNull(loggerFactory, "loggerFactory");

            _model = model;
            _dataStoreCreator = dataStoreCreator;
            _connection = connection;
            _logger = new LazyRef<ILogger>(loggerFactory.Create<Database>);
        }

        public virtual DataStoreConnection Connection
        {
            get { return _connection; }
        }

        // TODO: Make sure API docs say that return value indicates whether or not the database or tables were created
        public virtual bool EnsureCreated()
        {
            return DataStoreCreator.EnsureCreated(Model);
        }

        // TODO: Make sure API docs say that return value indicates whether or not the database or tables were created
        public virtual Task<bool> EnsureCreatedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DataStoreCreator.EnsureCreatedAsync(Model, cancellationToken);
        }

        // TODO: Make sure API docs say that return value indicates whether or not the database was deleted
        public virtual bool EnsureDeleted()
        {
            return DataStoreCreator.EnsureDeleted(Model);
        }

        // TODO: Make sure API docs say that return value indicates whether or not the database was deleted
        public virtual Task<bool> EnsureDeletedAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return DataStoreCreator.EnsureDeletedAsync(Model, cancellationToken);
        }

        protected virtual DataStoreCreator DataStoreCreator
        {
            get { return _dataStoreCreator; }
        }

        protected virtual ILogger Logger
        {
            get { return _logger.Value; }
        }

        protected virtual IModel Model
        {
            get { return _model.Value; }
        }

        DataStoreCreator IDatabaseInternals.DataStoreCreator
        {
            get { return DataStoreCreator; }
        }

        ILogger IDatabaseInternals.Logger
        {
            get { return Logger; }
        }

        IModel IDatabaseInternals.Model
        {
            get { return Model; }
        }
    }
}
