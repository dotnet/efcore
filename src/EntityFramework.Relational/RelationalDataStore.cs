// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
using JetBrains.Annotations;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalDataStore : DataStore, IRelationalDataStore
    {
        private readonly ICommandBatchPreparer _batchPreparer;
        private readonly IBatchExecutor _batchExecutor;
        private readonly IRelationalConnection _connection;
        private readonly IEntityOptions _options;
        private readonly IMethodCallTranslator _compositeMethodCallTranslator;
        private readonly IMemberTranslator _compositeMemberTranslator;

        protected RelationalDataStore(
            [NotNull] IModel model,
            [NotNull] IEntityKeyFactorySource entityKeyFactorySource,
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IClrAccessorSource<IClrPropertyGetter> clrPropertyGetterSource,
            [NotNull] IRelationalConnection connection,
            [NotNull] ICommandBatchPreparer batchPreparer,
            [NotNull] IBatchExecutor batchExecutor,
            [NotNull] IEntityOptions options,
            [NotNull] ILoggerFactory loggerFactory,
            [NotNull] IRelationalValueBufferFactoryFactory valueBufferFactoryFactory,
            [NotNull] IMethodCallTranslator compositeMethodCallTranslator,
            [NotNull] IMemberTranslator compositeMemberTranslator)
            : base(
                Check.NotNull(model, nameof(model)),
                Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource)),
                Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource)),
                Check.NotNull(clrPropertyGetterSource, nameof(clrPropertyGetterSource)),
                Check.NotNull(loggerFactory, nameof(loggerFactory)))
        {
            Check.NotNull(connection, nameof(connection));
            Check.NotNull(batchPreparer, nameof(batchPreparer));
            Check.NotNull(batchExecutor, nameof(batchExecutor));
            Check.NotNull(options, nameof(options));
            Check.NotNull(options, nameof(options));
            Check.NotNull(valueBufferFactoryFactory, nameof(valueBufferFactoryFactory));
            Check.NotNull(compositeMethodCallTranslator, nameof(compositeMethodCallTranslator));
            Check.NotNull(compositeMemberTranslator, nameof(compositeMemberTranslator));

            _batchPreparer = batchPreparer;
            _batchExecutor = batchExecutor;
            _connection = connection;
            _options = options;
            _compositeMethodCallTranslator = compositeMethodCallTranslator;
            _compositeMemberTranslator = compositeMemberTranslator;

            ValueBufferFactoryFactory = valueBufferFactoryFactory;
        }

        public virtual IRelationalValueBufferFactoryFactory ValueBufferFactoryFactory { get; }

        public virtual IEntityOptions EntityOptions => _options;

        public override int SaveChanges(
            IReadOnlyList<InternalEntityEntry> entries)
            => _batchExecutor.Execute(
                _batchPreparer.BatchCommands(
                    Check.NotNull(entries, nameof(entries)), _options),
                _connection);

        public override Task<int> SaveChangesAsync(
            IReadOnlyList<InternalEntityEntry> entries,
            CancellationToken cancellationToken = default(CancellationToken))
            => _batchExecutor.ExecuteAsync(
                _batchPreparer.BatchCommands(
                    Check.NotNull(entries, nameof(entries)), _options),
                _connection,
                cancellationToken);

        public override Func<QueryContext, IEnumerable<TResult>> CompileQuery<TResult>(QueryModel queryModel)
            => CreateQueryCompilationContext(
                new LinqOperatorProvider(),
                new RelationalResultOperatorHandler(),
                new QueryMethodProvider(),
                _compositeMethodCallTranslator,
                _compositeMemberTranslator)
                .CreateQueryModelVisitor()
                .CreateQueryExecutor<TResult>(
                    Check.NotNull(queryModel, nameof(queryModel)));

        public override Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>(QueryModel queryModel)
            => CreateQueryCompilationContext(
                new AsyncLinqOperatorProvider(),
                new RelationalResultOperatorHandler(),
                new AsyncQueryMethodProvider(),
                _compositeMethodCallTranslator,
                _compositeMemberTranslator)
                .CreateQueryModelVisitor()
                .CreateAsyncQueryExecutor<TResult>(
                    Check.NotNull(queryModel, nameof(queryModel)));

        protected virtual RelationalQueryCompilationContext CreateQueryCompilationContext(
            [NotNull] ILinqOperatorProvider linqOperatorProvider,
            [NotNull] IResultOperatorHandler resultOperatorHandler,
            [NotNull] IQueryMethodProvider queryMethodProvider,
            [NotNull] IMethodCallTranslator compositeMethodCallTranslator,
            [NotNull] IMemberTranslator compositeMemberTranslator)
            => new RelationalQueryCompilationContext(
                Model,
                Logger,
                Check.NotNull(linqOperatorProvider, nameof(linqOperatorProvider)),
                Check.NotNull(resultOperatorHandler, nameof(resultOperatorHandler)),
                EntityMaterializerSource,
                EntityKeyFactorySource,
                ClrPropertyGetterSource,
                Check.NotNull(queryMethodProvider, nameof(queryMethodProvider)),
                Check.NotNull(compositeMethodCallTranslator, nameof(compositeMethodCallTranslator)),
                Check.NotNull(compositeMemberTranslator, nameof(compositeMemberTranslator)),
                ValueBufferFactoryFactory);
    }
}
