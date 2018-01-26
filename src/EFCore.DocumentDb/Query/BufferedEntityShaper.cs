// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class BufferedEntityShaper<TEntity> : EntityShaper, IShaper<TEntity>
    {
        public BufferedEntityShaper(bool trackingQuery, IKey key, Func<ValueBuffer, DbContext, object> materializer)
            : base(trackingQuery, key, materializer)
        {
        }

        public TEntity Shape(QueryContext queryContext, ValueBuffer valueBuffer)
        {
            var entity
                = (TEntity)queryContext.QueryBuffer
                    .GetEntity(
                        Key,
                        new EntityLoadInfo(valueBuffer, queryContext.Context, Materializer),
                        queryStateManager: IsTrackingQuery,
                        throwOnNullKey: false);

            return entity;
        }

        public override Type Type => typeof(TEntity);
    }

    public abstract class EntityShaper : Shaper
    {
        protected EntityShaper(
            bool trackingQuery,
            IKey key,
            Func<ValueBuffer, DbContext, object> materializer)
        {
            IsTrackingQuery = trackingQuery;
            Key = key;
            Materializer = materializer;
        }

        public bool IsTrackingQuery { get; }
        public IKey Key { get; }
        public Func<ValueBuffer, DbContext, object> Materializer { get; }
    }
}
