// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Relational.Query;
using Microsoft.Data.Entity.Relational.Query.Methods;
using Microsoft.Data.Entity.Relational.Update;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalDataStore : DataStore, IRelationalDataStore
    {
        private readonly ICommandBatchPreparer _batchPreparer;
        private readonly IBatchExecutor _batchExecutor;
        private readonly IRelationalConnection _connection;
        private readonly IDbContextOptions _options;

        protected RelationalDataStore(
            [NotNull] IModel model,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IRelationalConnection connection,
            [NotNull] ICommandBatchPreparer batchPreparer,
            [NotNull] IBatchExecutor batchExecutor,
            [NotNull] IDbContextOptions options,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalValueReaderFactoryFactory valueReaderFactoryFactory)
            : base(
                Check.NotNull(model, nameof(model)),
                Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource)),
                Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource)),
                Check.NotNull(loggerFactory, nameof(loggerFactory)))
        {
            ValueReaderFactoryFactory = valueReaderFactoryFactory;
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(batchPreparer, nameof(batchPreparer));
            Check.NotNull(batchExecutor, nameof(batchExecutor));
            Check.NotNull(options, nameof(options));
            Check.NotNull(options, nameof(options));
            Check.NotNull(valueReaderFactoryFactory, nameof(valueReaderFactoryFactory));

            _batchPreparer = batchPreparer;
            _batchExecutor = batchExecutor;
            _connection = connection;
            _options = options;
        }

        public virtual IRelationalValueReaderFactoryFactory ValueReaderFactoryFactory { get; }

        public virtual IDbContextOptions DbContextOptions => _options;

        public override int SaveChanges(
            IReadOnlyList<InternalEntityEntry> entries)
        {
            Check.NotNull(entries, nameof(entries));

            var commandBatches = _batchPreparer.BatchCommands(entries, _options);

            return _batchExecutor.Execute(commandBatches, _connection);
        }

        public override Task<int> SaveChangesAsync(
            IReadOnlyList<InternalEntityEntry> entries,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            Check.NotNull(entries, nameof(entries));

            var commandBatches = _batchPreparer.BatchCommands(entries, _options);

            return _batchExecutor.ExecuteAsync(commandBatches, _connection, cancellationToken);
        }

        public override Func<QueryContext, IEnumerable<TResult>> CompileQuery<TResult>(QueryModel queryModel)
        {
            Check.NotNull(queryModel, nameof(queryModel));

            var queryCompilationContext
                = CreateQueryCompilationContext(
                    new LinqOperatorProvider(),
                    new RelationalResultOperatorHandler(),
                    new QueryMethodProvider(),
                    new CompositeMethodCallTranslator(Logger));

            return queryCompilationContext
                .CreateQueryModelVisitor()
                .CreateQueryExecutor<TResult>(queryModel);
        }

        public override Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>(QueryModel queryModel)
        {
            var queryCompilationContext
                = CreateQueryCompilationContext(
                    new AsyncLinqOperatorProvider(),
                    new RelationalResultOperatorHandler(),
                    new AsyncQueryMethodProvider(),
                    new CompositeMethodCallTranslator(Logger));

            return queryCompilationContext
                .CreateQueryModelVisitor()
                .CreateAsyncQueryExecutor<TResult>(queryModel);
        }

        protected virtual RelationalQueryCompilationContext CreateQueryCompilationContext(
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            [NotNull] IMethodCallTranslator methodCallTranslator)
        {
            Check.NotNull(linqOperatorProvider, nameof(linqOperatorProvider));
            Check.NotNull(resultOperatorHandler, nameof(resultOperatorHandler));
            Check.NotNull(queryMethodProvider, nameof(queryMethodProvider));
            Check.NotNull(methodCallTranslator, nameof(methodCallTranslator));

            return new RelationalQueryCompilationContext(
                Model,
                Logger,
                linqOperatorProvider,
                resultOperatorHandler,
                EntityMaterializerSource,
                EntityKeyFactorySource,
                queryMethodProvider,
                methodCallTranslator,
                ValueReaderFactoryFactory);
        }
    }
}
