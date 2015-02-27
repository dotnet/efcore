// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStore : IDataStore
    {
        private readonly LazyRef<ILogger> _logger;

        protected DataStore(
            [NotNull] IModel model,
            [NotNull] EntityKeyFactorySource entityKeyFactorySource,
            [NotNull] EntityMaterializerSource entityMaterializerSource,
            [NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(entityKeyFactorySource, nameof(entityKeyFactorySource));
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            Model = model;

            EntityKeyFactorySource = entityKeyFactorySource;
            EntityMaterializerSource = entityMaterializerSource;

            _logger = new LazyRef<ILogger>(loggerFactory.Create<DataStore>);
        }

        public virtual ILogger Logger => _logger.Value;

        public virtual IModel Model { get; }

        public virtual EntityKeyFactorySource EntityKeyFactorySource { get; }

        public virtual EntityMaterializerSource EntityMaterializerSource { get; }

        public abstract int SaveChanges(IReadOnlyList<InternalEntityEntry> entries);

        public abstract Task<int> SaveChangesAsync(
            IReadOnlyList<InternalEntityEntry> entries,
            CancellationToken cancellationToken = default(CancellationToken));

        public static readonly MethodInfo CompileQueryMethod
            = typeof(IDataStore).GetTypeInfo().GetDeclaredMethod("CompileQuery");

        public abstract Func<QueryContext, IEnumerable<TResult>> CompileQuery<TResult>(QueryModel queryModel);

        public static readonly MethodInfo CompileAsyncQueryMethod
            = typeof(IDataStore).GetTypeInfo().GetDeclaredMethod("CompileAsyncQuery");

        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>(QueryModel queryModel)
        {
            throw new NotImplementedException();
        }
    }
}
