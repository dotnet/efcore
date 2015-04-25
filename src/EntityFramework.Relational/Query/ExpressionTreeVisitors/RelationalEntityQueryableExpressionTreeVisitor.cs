// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.Query.Annotations;
using Microsoft.Data.Entity.Query.ExpressionTreeVisitors;
using Microsoft.Data.Entity.Relational.Query.Annotations;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Relational.Query.ExpressionTreeVisitors
{
    public class RelationalEntityQueryableExpressionTreeVisitor : EntityQueryableExpressionTreeVisitor
    {
        private static readonly ParameterExpression _readerParameter
            = Expression.Parameter(typeof(DbDataReader));

        public RelationalEntityQueryableExpressionTreeVisitor(
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [NotNull] IQuerySource querySource)
            : base(
                Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)),
                Check.NotNull(querySource, nameof(querySource)))
        {
        }

        private new RelationalQueryModelVisitor QueryModelVisitor => (RelationalQueryModelVisitor)base.QueryModelVisitor;

        protected override Expression VisitMemberExpression([NotNull] MemberExpression memberExpression)
        {
            Check.NotNull(memberExpression, nameof(memberExpression));

            QueryModelVisitor
                .BindMemberExpression(
                    memberExpression,
                    (property, querySource, selectExpression)
                        => selectExpression.AddToProjection(
                            QueryModelVisitor.QueryCompilationContext
                                .GetColumnName(property),
                            property,
                            querySource));

            return base.VisitMemberExpression(memberExpression);
        }

        protected override Expression VisitMethodCallExpression([NotNull] MethodCallExpression methodCallExpression)
        {
            Check.NotNull(methodCallExpression, nameof(methodCallExpression));

            QueryModelVisitor
                .BindMethodCallExpression(
                    methodCallExpression,
                    (property, querySource, selectExpression)
                        => selectExpression.AddToProjection(
                            QueryModelVisitor.QueryCompilationContext
                                .GetColumnName(property),
                            property,
                            querySource));

            return base.VisitMethodCallExpression(methodCallExpression);
        }

        protected override Expression VisitEntityQueryable(Type elementType)
        {
            Check.NotNull(elementType, nameof(elementType));

            var queryMethodInfo = RelationalQueryModelVisitor.CreateValueBufferMethodInfo;

            var entityType = QueryModelVisitor.QueryCompilationContext.Model.GetEntityType(elementType);

            var selectExpression = new SelectExpression();
            var tableName = QueryModelVisitor.QueryCompilationContext.GetTableName(entityType);

            var alias = QuerySource.ItemName.StartsWith("<generated>_")
                ? tableName[0].ToString().ToLower()
                : QuerySource.ItemName;

            var fromSqlAnnotation
                = QueryModelVisitor.QueryCompilationContext.QueryAnnotations
                    .OfType<FromSqlQueryAnnotation>()
                    .SingleOrDefault(a => a.QuerySource == QuerySource);

            selectExpression.AddTable(
                (fromSqlAnnotation != null)
                    ? (TableExpressionBase)new RawSqlDerivedTableExpression(
                        fromSqlAnnotation.Sql,
                        fromSqlAnnotation.Parameters,
                        alias,
                        QuerySource)
                    : new TableExpression(
                        tableName,
                        QueryModelVisitor.QueryCompilationContext.GetSchema(entityType),
                        alias,
                        QuerySource));

            var composed = true;
            if (fromSqlAnnotation != null)
            {
                if (!fromSqlAnnotation.Sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    if (QueryModelVisitor.QueryCompilationContext.QueryAnnotations.OfType<IncludeQueryAnnotation>().Any())
                    {
                        throw new InvalidOperationException(Strings.StoredProcedureIncludeNotSupported);
                    }
                    // Note: All client eval flags should be set here
                    QueryModelVisitor.RequiresClientFilter = true;
                    composed = false;
                }

                if (fromSqlAnnotation.QueryModel.IsIdentityQuery()
                    && !fromSqlAnnotation.QueryModel.ResultOperators.Any())
                {
                    composed = false;
                }
            }

            QueryModelVisitor.AddQuery(QuerySource, selectExpression);

            var queryMethodArguments
                = new List<Expression>
                    {
                        Expression.Constant(QuerySource),
                        EntityQueryModelVisitor.QueryContextParameter,
                        EntityQueryModelVisitor.QuerySourceScopeParameter,
                        new ValueBufferFactoryExpression(
                            QueryModelVisitor.QueryCompilationContext.ValueBufferFactoryFactory,
                            () => QueryModelVisitor.GetProjectionTypes(QuerySource),
                            0),
                        _readerParameter,
                    };

            if (QueryModelVisitor.QuerySourceRequiresMaterialization(QuerySource))
            {
                var materializer
                    = new MaterializerFactory(
                        QueryModelVisitor
                            .QueryCompilationContext
                            .EntityMaterializerSource)
                        .CreateMaterializer(
                            entityType,
                            selectExpression,
                            (p, se) =>
                                se.AddToProjection(
                                    QueryModelVisitor.QueryCompilationContext.GetColumnName(p),
                                    p,
                                    QuerySource),
                            QuerySource);

                queryMethodInfo
                    = RelationalQueryModelVisitor.CreateEntityMethodInfo
                        .MakeGenericMethod(elementType);

                var keyProperties
                    = entityType.GetPrimaryKey().Properties;

                var keyFactory
                    = QueryModelVisitor.QueryCompilationContext.EntityKeyFactorySource
                        .GetKeyFactory(keyProperties);

                queryMethodArguments.AddRange(
                    new[]
                        {
                            Expression.Constant(entityType),
                            Expression.Constant(QueryModelVisitor.QuerySourceRequiresTracking(QuerySource)),
                            Expression.Constant(keyFactory),
                            Expression.Constant(keyProperties),
                            materializer
                        });
            }

            var sqlQueryGenerator = composed
                ? QueryModelVisitor.QueryCompilationContext.CreateSqlQueryGenerator(selectExpression)
                : new RawSqlQueryGenerator(fromSqlAnnotation.Sql, fromSqlAnnotation.Parameters);

            return Expression.Call(
                QueryModelVisitor.QueryCompilationContext.QueryMethodProvider.QueryMethod
                    .MakeGenericMethod(queryMethodInfo.ReturnType),
                EntityQueryModelVisitor.QueryContextParameter,
                Expression.Constant(new CommandBuilder(sqlQueryGenerator)),
                Expression.Lambda(
                    Expression.Call(queryMethodInfo, queryMethodArguments),
                    _readerParameter));
        }
    }
}
