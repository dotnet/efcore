// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public class UnbufferedEntityShaper<TEntity> : EntityShaper, IShaper<TEntity>
        where TEntity : class
    {
        public UnbufferedEntityShaper(
            [NotNull] IQuerySource querySource,
            [NotNull] string entityType,
            bool trackingQuery,
            [NotNull] IKey key,
            [NotNull] Func<ValueBuffer, object> materializer)
            : base(querySource, entityType, trackingQuery, key, materializer)
        {
        }

        public override Type Type => typeof(TEntity);

        public virtual object GetKey(QueryContext queryContext, ValueBuffer valueBuffer) =>
            queryContext.StateManager.TryGetEntryKey(Key, valueBuffer, !AllowNullResult);

        public virtual TEntity Shape(QueryContext queryContext, ValueBuffer valueBuffer)
        {
            if (IsTrackingQuery)
            {
                var entry = queryContext.StateManager.TryGetEntry(Key, valueBuffer, !AllowNullResult);

                if (entry != null)
                {
                    return (TEntity)entry.Entity;
                }
            }

            return (TEntity)Materializer(valueBuffer);
        }

        public override IShaper<TDerived> Cast<TDerived>()
            => new UnbufferedOffsetEntityShaper<TDerived>(
                QuerySource,
                EntityType,
                IsTrackingQuery,
                Key,
                Materializer);

        public override EntityShaper WithOffset(int offset)
            => new UnbufferedOffsetEntityShaper<TEntity>(
                QuerySource,
                EntityType,
                IsTrackingQuery,
                Key,
                Materializer)
                .SetOffset(offset);

        public override string ToString() => "UnbufferedEntityShaper<" + typeof(TEntity).Name + ">";
    }
}
