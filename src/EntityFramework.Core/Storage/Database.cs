// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.Logging;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class Database : IDatabase
    {
        private readonly LazyRef<ILogger> _logger;

        protected Database(
            [NotNull] IModel model,
            [NotNull] ILoggerFactory loggerFactory)
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(loggerFactory, nameof(loggerFactory));

            Model = model;

            _logger = new LazyRef<ILogger>(loggerFactory.CreateLogger<Database>);
        }

        public virtual IModel Model { get; }

        public virtual ILogger Logger => _logger.Value;

        public abstract int SaveChanges(IReadOnlyList<InternalEntityEntry> entries);

        public abstract Task<int> SaveChangesAsync(
            IReadOnlyList<InternalEntityEntry> entries,
            CancellationToken cancellationToken = default(CancellationToken));

        public static MethodInfo CompileQueryMethod { get; }
            = typeof(IDatabase).GetTypeInfo().GetDeclaredMethod("CompileQuery");

        public abstract Func<QueryContext, IEnumerable<TResult>> CompileQuery<TResult>(QueryModel queryModel);

        public static MethodInfo CompileAsyncQueryMethod { get; }
            = typeof(IDatabase).GetTypeInfo().GetDeclaredMethod("CompileAsyncQuery");

        public virtual Func<QueryContext, IAsyncEnumerable<TResult>> CompileAsyncQuery<TResult>(QueryModel queryModel)
        {
            throw new NotImplementedException();
        }
    }
}
