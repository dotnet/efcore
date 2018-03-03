// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class UnbufferedEntityShaper<TEntity> : EntityShaper, IShaper<TEntity>
    {
        public UnbufferedEntityShaper(bool trackingQuery, IKey key, Func<ValueBuffer, DbContext, object> materializer)
            : base(trackingQuery, key, materializer)
        {
        }

        public TEntity Shape(QueryContext queryContext, ValueBuffer valueBuffer)
        {
            if (IsTrackingQuery)
            {
                var entry = queryContext.StateManager.TryGetEntry(Key, valueBuffer, false);

                if (entry != null)
                {
                    return (TEntity)entry.Entity;
                }
            }

            return (TEntity)Materializer(valueBuffer, queryContext.Context);
        }

        public override Type Type => typeof(TEntity);
    }
}
