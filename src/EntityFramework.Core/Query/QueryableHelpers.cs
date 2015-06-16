// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Parsing.ExpressionVisitors;

namespace Microsoft.Data.Entity.Query
{
    public static class QueryableHelpers
    {
        public static IQueryable<T> CreateQuery<T, TR>(
            [NotNull] IQueryable<T> source,
            [NotNull] Expression<Func<IQueryable<T>, TR>> expression)
            => Check.NotNull(source, nameof(source))
                .Provider.CreateQuery<T>(
                    ReplacingExpressionVisitor.Replace(
                        Check.NotNull(expression, nameof(expression)).Parameters[0],
                        source.Expression,
                        expression.Body));
    }
}
