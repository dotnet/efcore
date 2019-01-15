// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
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
    public class UnbufferedEntityShaper<TEntity> : EntityShaper, IShaper<TEntity>
        where TEntity : class
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public UnbufferedEntityShaper(
            [NotNull] IQuerySource querySource,
            bool trackingQuery,
            [NotNull] IKey key,
            [NotNull] Func<MaterializationContext, object> materializer,
            [CanBeNull] Expression materializerExpression)
            : base(querySource, trackingQuery, key, materializer, materializerExpression)
        {
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
            if (IsTrackingQuery)
            {
                var entry = queryContext.StateManager.TryGetEntry(Key, new object[] { }, !AllowNullResult, out var _);

                if (entry != null)
                {
                    return (TEntity)entry.Entity;
                }
            }

            return (TEntity)Materializer(
                new MaterializationContext(
                    valueBuffer,
                    queryContext.Context));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override IShaper<TDerived> Cast<TDerived>()
            => new UnbufferedOffsetEntityShaper<TDerived>(
                QuerySource,
                IsTrackingQuery,
                Key,
                Materializer);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Shaper WithOffset(int offset)
            => new UnbufferedOffsetEntityShaper<TEntity>(
                    QuerySource,
                    IsTrackingQuery,
                    Key,
                    Materializer)
                .AddOffset(offset);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override string ToString() => "UnbufferedEntityShaper<" + typeof(TEntity).Name + ">";
    }
}
