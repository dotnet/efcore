// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Redis.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Redis.Query
{
    public partial class RedisQueryModelVisitor : EntityQueryModelVisitor
    {
        private readonly Dictionary<IQuerySource, RedisQuery> _queriesBySource
            = new Dictionary<IQuerySource, RedisQuery>();

        public RedisQueryModelVisitor(
            [NotNull] RedisQueryCompilationContext queryCompilationContext)
            : base(queryCompilationContext)
        {
        }

        private RedisQueryModelVisitor([NotNull] RedisQueryModelVisitor visitor)
            : this((RedisQueryCompilationContext)visitor.QueryCompilationContext)
        {
        }

        private RedisQuery FindOrCreateQuery(IQuerySource querySource)
        {
            var entityType = QueryCompilationContext.Model.GetEntityType(querySource.ItemType);
            RedisQuery redisQuery;
            if (!_queriesBySource.TryGetValue(querySource, out redisQuery))
            {
                redisQuery = new RedisQuery(entityType);
                _queriesBySource[querySource] = redisQuery;
            }

            return redisQuery;
        }

        protected override ExpressionTreeVisitor CreateQueryingExpressionTreeVisitor(IQuerySource querySource)
        {
            return new RedisQueryingExpressionTreeVisitor(this, querySource);
        }

        protected override Expression BindMethodCallToValueReader(
            MethodCallExpression methodCallExpression,
            Expression expression)
        {
            Check.NotNull(methodCallExpression, "methodCallExpression");
            Check.NotNull(expression, "expression");

            return BindMethodCallExpression(
                methodCallExpression,
                (property, querySource, redisQuery) =>
                    {
                        var projectionIndex = redisQuery.GetProjectionIndex(property);
                        Contract.Assert(projectionIndex > -1);
                        return BindReadValueMethod(methodCallExpression.Type, expression, projectionIndex);
                    });
        }

        private TResult BindMethodCallExpression<TResult>(
            MethodCallExpression methodCallExpression,
            Func<IProperty, IQuerySource, RedisQuery, TResult> methodCallBinder)
        {
            return base.BindMethodCallExpression(methodCallExpression, null,
                (property, qs) =>
                    {
                        var redisQuery = FindOrCreateQuery(qs);
                        redisQuery.AddProperty(property);
                        return methodCallBinder(property, qs, redisQuery);
                    });
        }

        protected override Expression BindMemberToValueReader(
            MemberExpression memberExpression, Expression expression)
        {
            Check.NotNull(memberExpression, "memberExpression");
            Check.NotNull(expression, "expression");

            return BindMemberExpression(
                memberExpression,
                null,
                (property, querySource, redisQuery) =>
                    {
                        var projectionIndex = redisQuery.GetProjectionIndex(property);
                        Contract.Assert(projectionIndex > -1);
                        return BindReadValueMethod(memberExpression.Type, expression, projectionIndex);
                    });
        }

        private TResult BindMemberExpression<TResult>(
            MemberExpression memberExpression,
            IQuerySource querySource,
            Func<IProperty, IQuerySource, RedisQuery, TResult> memberBinder)
        {
            return base.BindMemberExpression(memberExpression, querySource,
                (property, qs) =>
                    {
                        var redisQuery = FindOrCreateQuery(qs);
                        redisQuery.AddProperty(property);
                        return memberBinder(property, qs, redisQuery);
                    });
        }

        private static readonly MethodInfo _executeMaterializedQueryExpressionMethodInfo =
            typeof(RedisQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("ExecuteMaterializedQueryExpression");

        [UsedImplicitly]
        private static IEnumerable<TResult> ExecuteMaterializedQueryExpression<TResult>(
            QueryContext queryContext, IEntityType entityType)
            where TResult : class, new()
        {
            var redisQueryContext = (RedisQueryContext)queryContext;

            return redisQueryContext.GetResultsFromRedis<TResult>(entityType);
        }

        private static readonly MethodInfo _executeNonMaterializedQueryExpressionMethodInfo =
            typeof(RedisQueryModelVisitor).GetTypeInfo().GetDeclaredMethod("ExecuteValueReader");

        [UsedImplicitly]
        private IEnumerable<IValueReader> ExecuteValueReader(
            IQuerySource querySource, QueryContext queryContext)
        {
            var redisQuery = FindOrCreateQuery(querySource);
            var redisQueryContext = (RedisQueryContext)queryContext;

            return redisQuery.GetValueReaders(redisQueryContext);
        }
    }
}
