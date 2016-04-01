// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public class BufferedOffsetEntityShaper<TEntity> : BufferedEntityShaper<TEntity>
        where TEntity : class
    {
        public BufferedOffsetEntityShaper(
            [NotNull] IQuerySource querySource,
            [NotNull] string entityType,
            bool trackingQuery,
            [NotNull] IKey key,
            [NotNull] Func<ValueBuffer, object> materializer)
            : base(querySource, entityType, trackingQuery, key, materializer)
        {
        }

        public override TEntity Shape(QueryContext queryContext, ValueBuffer valueBuffer)
            => base.Shape(queryContext, valueBuffer.WithOffset(ValueBufferOffset));

        public override string ToString() 
            => "BufferedOffsetEntityShaper<" + typeof(TEntity).Name + ">";
    }
}
