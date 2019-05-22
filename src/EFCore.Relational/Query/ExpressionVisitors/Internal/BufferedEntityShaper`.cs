// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class BufferedEntityShaper<TEntity> : EntityShaper, IShaper<TEntity>
        where TEntity : class
    {
        private readonly Dictionary<Type, int[]> _typeIndexMap;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public BufferedEntityShaper(
            [NotNull] IQuerySource querySource,
            bool trackingQuery,
            [NotNull] IKey key,
            [NotNull] Func<MaterializationContext, object> materializer,
            [CanBeNull] Dictionary<Type, int[]> typeIndexMap)
            : base(querySource, trackingQuery, key, materializer, materializerExpression: null)
        {
            _typeIndexMap = typeIndexMap;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Type Type => typeof(TEntity);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual TEntity Shape(QueryContext queryContext, in ValueBuffer valueBuffer)
        {
            Debug.Assert(queryContext != null);

            return (TEntity)queryContext.QueryBuffer
                .GetEntity(
                    Key,
                    new EntityLoadInfo(
                        new MaterializationContext(valueBuffer, queryContext.Context),
                        Materializer,
                        _typeIndexMap),
                    queryStateManager: IsTrackingQuery,
                    throwOnNullKey: !AllowNullResult);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IShaper<TDerived> Cast<TDerived>()
            => new BufferedOffsetEntityShaper<TDerived>(
                QuerySource,
                IsTrackingQuery,
                Key,
                Materializer,
                _typeIndexMap);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Shaper WithOffset(int offset)
            => new BufferedOffsetEntityShaper<TEntity>(
                    QuerySource,
                    IsTrackingQuery,
                    Key,
                    Materializer,
                    _typeIndexMap)
                .AddOffset(offset);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString() => "BufferedEntityShaper<" + typeof(TEntity).Name + ">";
    }
}
