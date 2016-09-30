// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class Shaper
    {
        private IQuerySource _querySource;
        private Expression _accessorExpression;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected Shaper([NotNull] IQuerySource querySource)
        {
            _querySource = querySource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsShaperForQuerySource([NotNull] IQuerySource querySource)
            => _querySource == querySource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void UpdateQuerySource([NotNull] IQuerySource querySource)
        {
            _querySource = querySource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public abstract Type Type { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual IQuerySource QuerySource => _querySource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void SaveAccessorExpression([NotNull] QuerySourceMapping querySourceMapping)
        {
            _accessorExpression = querySourceMapping.GetExpression(_querySource);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression GetAccessorExpression([NotNull] IQuerySource querySource)
        {
            if (_querySource == querySource)
            {
                return _accessorExpression != null
                    ? (Expression)Expression
                        .Lambda(
                            _accessorExpression,
                            _accessorExpression.GetRootExpression<ParameterExpression>())
                    : Expression
                        .Default(typeof(Func<,>)
                            .MakeGenericType(Type, typeof(object)));
            }

            return null;
        }
    }
}
