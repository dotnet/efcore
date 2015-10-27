// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors.Internal
{
    public class UnbufferedOffsetEntityShaper<TEntity> : EntityShaper, IShaper<TEntity>
        where TEntity : class
    {
        public UnbufferedOffsetEntityShaper(
            [NotNull] IQuerySource querySource,
            [NotNull] string entityType,
            [NotNull] KeyValueFactory keyValueFactory,
            [NotNull] Func<ValueBuffer, object> materializer)
            : base(querySource, entityType, keyValueFactory, materializer)
        {
        }

        public override Type Type => typeof(TEntity);

        public virtual TEntity Shape(QueryContext queryContext, ValueBuffer valueBuffer)
        {
            valueBuffer = valueBuffer.WithOffset(ValueBufferOffset);

            var keyValue = KeyValueFactory.Create(valueBuffer);

            TEntity entity = null;

            if (keyValue.IsInvalid)
            {
                if (!AllowNullResult)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.InvalidKeyValue(EntityType));
                }
            }
            else
            {
                entity = (TEntity)Materializer(valueBuffer);
            }

            return entity;
        }
    }
}
