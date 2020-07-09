// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ManyToManyNoTrackingQueryTestBase<TFixture> : ManyToManyQueryTestBase<TFixture>
        where TFixture : ManyToManyQueryFixtureBase, new()
    {
        private static readonly MethodInfo _asNoTrackingMethodInfo
               = typeof(EntityFrameworkQueryableExtensions)
                   .GetTypeInfo().GetDeclaredMethod(nameof(EntityFrameworkQueryableExtensions.AsNoTracking));

        protected ManyToManyNoTrackingQueryTestBase(TFixture fixture)
            : base(fixture)
        {
        }
        protected override bool IgnoreEntryCount => true;

        protected override Expression RewriteServerQueryExpression(Expression serverQueryExpression)
        {
            serverQueryExpression = base.RewriteServerQueryExpression(serverQueryExpression);

            var elementType = serverQueryExpression.Type.TryGetSequenceType();

            if (elementType.UnwrapNullableType().IsValueType
                && serverQueryExpression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.DeclaringType == typeof(Queryable))
            {
                return methodCallExpression.Update(
                    null, new[] { ApplyNoTracking(methodCallExpression.Arguments[0]) }
                                .Concat(methodCallExpression.Arguments.Skip(1)));
            }

            return ApplyNoTracking(serverQueryExpression);

            static Expression ApplyNoTracking(Expression source)
            {
                return Expression.Call(
                  _asNoTrackingMethodInfo.MakeGenericMethod(source.Type.TryGetSequenceType()),
                   source);
            }
        }
    }
}
