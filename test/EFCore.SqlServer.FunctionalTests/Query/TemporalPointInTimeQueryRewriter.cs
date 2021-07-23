﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class TemporalPointInTimeQueryRewriter : ExpressionVisitor
    {
        private static readonly MethodInfo _setMethodInfo
            = typeof(ISetSource).GetMethod(nameof(ISetSource.Set));

        private static readonly MethodInfo _asOfMethodInfo
            = typeof(SqlServerDbSetExtensions).GetMethod(nameof(SqlServerDbSetExtensions.TemporalAsOf));

        private readonly DateTime _pointInTime;

        // TODO: need model instead
        private readonly List<Type> _temporalEntityTypes;

        public TemporalPointInTimeQueryRewriter(DateTime pointInTime, List<Type> temporalEntityTypes)
        {
            _pointInTime = pointInTime;
            _temporalEntityTypes = temporalEntityTypes;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is QueryRootExpression queryRootExpression
                && queryRootExpression.EntityType.GetRootType().IsTemporal())
            {
                return new TemporalAsOfQueryRootExpression(
                    queryRootExpression.QueryProvider,
                    queryRootExpression.EntityType,
                    _pointInTime);
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            // TODO: issue #25236 - also match named sets
            // in case we want to reuse this on queries that are not using AssertQuery infra 
            if (methodCallExpression.Method.IsGenericMethod
                && methodCallExpression.Method.GetGenericMethodDefinition() == _setMethodInfo)
            {
                var entityType = methodCallExpression.Method.GetGenericArguments()[0];
                if (_temporalEntityTypes.Contains(entityType))
                {
                    var method = _asOfMethodInfo.MakeGenericMethod(methodCallExpression.Method.GetGenericArguments()[0]);

                    // temporal methods are defined on DBSet so we need to hard cast here.
                    // This rewrite is only done for actual queries (and not expected), so the cast is safe to do
                    var dbSetType = typeof(DbSet<>).MakeGenericType(entityType);

                    return Expression.Call(
                        method,
                        Expression.Convert(
                            methodCallExpression,
                            dbSetType),
                        Expression.Constant(_pointInTime));
                }
            }

            return base.VisitMethodCall(methodCallExpression);
        }
    }
}
