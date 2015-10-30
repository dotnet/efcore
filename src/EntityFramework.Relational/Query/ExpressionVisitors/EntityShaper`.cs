// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Storage;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class EntityShaper<TEntity> : EntityShaper, IShaper<TEntity>
        where TEntity : class
    {
        private readonly string _entityType;
        private readonly bool _trackingQuery;
        private readonly KeyValueFactory _keyValueFactory;
        private readonly Func<ValueBuffer, object> _materializer;
        private readonly bool _useQueryBuffer;

        public EntityShaper(
            [NotNull] IQuerySource querySource,
            [NotNull] string entityType,
            bool trackingQuery,
            [NotNull] KeyValueFactory keyValueFactory,
            [NotNull] Func<ValueBuffer, object> materializer,
            bool useQueryBuffer)
            : base(querySource)
        {
            _entityType = entityType;
            _trackingQuery = trackingQuery;
            _keyValueFactory = keyValueFactory;
            _materializer = materializer;
            _useQueryBuffer = useQueryBuffer;
        }

        public override Type Type => typeof(TEntity);

        public virtual TEntity Shape(QueryContext queryContext, ValueBuffer valueBuffer)
        {
            valueBuffer = valueBuffer.WithOffset(ValueBufferOffset);

            var keyValue = _keyValueFactory.Create(valueBuffer);

            TEntity entity = null;

            if (keyValue.IsInvalid)
            {
                if (!AllowNullResult)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.InvalidKeyValue(_entityType));
                }
            }
            else if (_useQueryBuffer)
            {
                entity
                    = (TEntity)queryContext.QueryBuffer
                        .GetEntity(
                            keyValue,
                            new EntityLoadInfo(valueBuffer, _materializer),
                            queryStateManager: _trackingQuery);
            }
            else
            {
                entity = (TEntity)_materializer(valueBuffer);
            }

            return entity;
        }
    }
}
