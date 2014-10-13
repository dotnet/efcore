// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class RelationalEntityQueryableExpressionTreeVisitor : EntityQueryableExpressionTreeVisitor
    {
        private static readonly ParameterExpression _readerParameter
            = Expression.Parameter(typeof(DbDataReader));

        private readonly IQuerySource _querySource;

        public RelationalEntityQueryableExpressionTreeVisitor(
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [NotNull] IQuerySource querySource)
            : base(Check.NotNull(queryModelVisitor, "queryModelVisitor"))
        {
            Check.NotNull(querySource, "querySource");

            _querySource = querySource;
        }

        private new RelationalQueryModelVisitor QueryModelVisitor
        {
            get { return (RelationalQueryModelVisitor)base.QueryModelVisitor; }
        }

        protected override Expression VisitMemberExpression(MemberExpression memberExpression)
        {
            QueryModelVisitor
                .BindMemberExpression(
                    memberExpression,
                    (property, querySource, selectExpression)
                        => selectExpression.AddToProjection(property, querySource));

            return base.VisitMemberExpression(memberExpression);
        }

        protected override Expression VisitMethodCallExpression(MethodCallExpression methodCallExpression)
        {
            QueryModelVisitor
                .BindMethodCallExpression(
                    methodCallExpression,
                    (property, querySource, selectExpression)
                        => selectExpression.AddToProjection(property, querySource));

            return base.VisitMethodCallExpression(methodCallExpression);
        }

        protected override Expression VisitEntityQueryable(Type elementType)
        {
            var queryMethodInfo = RelationalQueryModelVisitor.CreateValueReaderMethodInfo;
            var entityType = QueryModelVisitor.QueryCompilationContext.Model.GetEntityType(elementType);

            var selectExpression = new SelectExpression();
            var tableName = entityType.TableName();

            selectExpression
                .AddTable(
                    new TableExpression(
                        tableName,
                        entityType.Schema(),
                        _querySource.ItemName.StartsWith("<generated>_")
                            ? tableName.First().ToString().ToLower()
                            : _querySource.ItemName,
                        _querySource));

            QueryModelVisitor.AddQuery(_querySource, selectExpression);

            var queryMethodArguments
                = new List<Expression>
                    {
                        Expression.Constant(_querySource),
                        EntityQueryModelVisitor.QueryContextParameter,
                        EntityQueryModelVisitor.QuerySourceScopeParameter,
                        _readerParameter
                    };

            if (QueryModelVisitor.QuerySourceRequiresMaterialization(_querySource))
            {
                foreach (var property in entityType.Properties)
                {
                    selectExpression.AddToProjection(property, _querySource);
                }

                queryMethodInfo = RelationalQueryModelVisitor.CreateEntityMethodInfo.MakeGenericMethod(elementType);

                queryMethodArguments.Add(Expression.Constant(0));
                queryMethodArguments.Add(Expression.Constant(entityType));
            }

            return Expression.Call(
                QueryModelVisitor.QueryCompilationContext.QueryMethodProvider.QueryMethod
                    .MakeGenericMethod(queryMethodInfo.ReturnType),
                EntityQueryModelVisitor.QueryContextParameter,
                Expression.Constant(new CommandBuilder(selectExpression, QueryModelVisitor.QueryCompilationContext)),
                Expression.Lambda(
                    Expression.Call(queryMethodInfo, queryMethodArguments),
                    _readerParameter));
        }
    }
}
