// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    public abstract class Shaper
    {
        private IQuerySource _querySource;
        private Expression _accessorExpression;

        protected Shaper([NotNull] IQuerySource querySource)
        {
            _querySource = querySource;
        }

        public virtual bool IsShaperForQuerySource([NotNull] IQuerySource querySource)
            => _querySource == querySource;

        public virtual void UpdateQuerySource([NotNull] IQuerySource querySource)
        {
            _querySource = querySource;
        }

        public abstract Type Type { get; }

        protected virtual IQuerySource QuerySource => _querySource;

        public virtual void SaveAccessorExpression([NotNull] QuerySourceMapping querySourceMapping)
        {
            _accessorExpression = querySourceMapping.GetExpression(_querySource);
        }

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
