// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Annotations;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class RelationalEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        private static readonly ParameterExpression _valueBufferParameter
            = Expression.Parameter(typeof(ValueBuffer));

        private readonly IQuerySource _querySource;

        public RelationalEntityQueryableExpressionVisitor(
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [NotNull] IQuerySource querySource)
            : base(Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)))
        {
            Check.NotNull(querySource, nameof(querySource));

            _querySource = querySource;
        }

        private new RelationalQueryModelVisitor QueryModelVisitor => (RelationalQueryModelVisitor)base.QueryModelVisitor;

        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            Check.NotNull(subQueryExpression, nameof(subQueryExpression));

            var queryModelVisitor = (RelationalQueryModelVisitor)CreateQueryModelVisitor();

            queryModelVisitor.VisitQueryModel(subQueryExpression.QueryModel);

            if (queryModelVisitor.Queries.Count == 1
                && !queryModelVisitor.RequiresClientFilter
                && !queryModelVisitor.RequiresClientSelectMany
                && !queryModelVisitor.RequiresClientProjection
                && !queryModelVisitor.RequiresClientResultOperator)
            {
                QueryModelVisitor.AddSubquery(_querySource, queryModelVisitor.Queries.First());
            }

            return queryModelVisitor.Expression;
        }

        protected override Expression VisitMember(MemberExpression memberExpression)
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

            return base.VisitMember(memberExpression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
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

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override Expression VisitEntityQueryable(Type elementType)
        {
            Check.NotNull(elementType, nameof(elementType));

            var queryMethodInfo = RelationalQueryModelVisitor.CreateValueBufferMethodInfo;
            var relationalQueryCompilationContext = QueryModelVisitor.QueryCompilationContext;
            var entityType = relationalQueryCompilationContext.Model.GetEntityType(elementType);
            var selectExpression = new SelectExpression();
            var name = relationalQueryCompilationContext.GetTableName(entityType);

            var tableAlias
                = _querySource.HasGeneratedItemName()
                    ? name[0].ToString().ToLower()
                    : _querySource.ItemName;

            var fromSqlAnnotation
                = relationalQueryCompilationContext
                    .GetCustomQueryAnnotations(RelationalQueryableExtensions.FromSqlMethodInfo)
                    .LastOrDefault(a => a.QuerySource == _querySource);

            var composable = true;
            var sqlString = "";
            object[] sqlParameters = null;

            if (fromSqlAnnotation == null)
            {
                selectExpression.AddTable(
                    new TableExpression(
                        name,
                        relationalQueryCompilationContext.GetSchema(entityType),
                        tableAlias,
                        _querySource));
            }
            else
            {
                sqlString = (string)fromSqlAnnotation.Arguments[1];
                sqlParameters = (object[])fromSqlAnnotation.Arguments[2];

                selectExpression.AddTable(
                    new RawSqlDerivedTableExpression(
                        sqlString,
                        sqlParameters,
                        tableAlias,
                        _querySource));

                var sqlStart = sqlString.SkipWhile(char.IsWhiteSpace).Take(7).ToArray();

                if (sqlStart.Length != 7
                    || !char.IsWhiteSpace(sqlStart.Last())
                    || !new string(sqlStart).StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
                {
                    if (relationalQueryCompilationContext.QueryAnnotations
                        .OfType<IncludeQueryAnnotation>().Any())
                    {
                        throw new InvalidOperationException(Strings.StoredProcedureIncludeNotSupported);
                    }

                    QueryModelVisitor.RequiresClientEval = true;

                    composable = false;
                }

                if (!fromSqlAnnotation.QueryModel.BodyClauses.Any()
                    && !fromSqlAnnotation.QueryModel.ResultOperators.Any())
                {
                    composable = false;
                }
            }

            QueryModelVisitor.AddQuery(_querySource, selectExpression);

            var queryMethodArguments
                = new List<Expression>
                {
                    Expression.Constant(_querySource),
                    EntityQueryModelVisitor.QueryContextParameter,
                    EntityQueryModelVisitor.QueryResultScopeParameter,
                    _valueBufferParameter,
                    Expression.Constant(0)
                };

            if (QueryModelVisitor.QuerySourceRequiresMaterialization(_querySource)
                || QueryModelVisitor.RequiresClientEval)
            {
                var materializer
                    = new MaterializerFactory(
                        relationalQueryCompilationContext
                            .EntityMaterializerSource)
                        .CreateMaterializer(
                            entityType,
                            selectExpression,
                            (p, se) =>
                                se.AddToProjection(
                                    relationalQueryCompilationContext.GetColumnName(p),
                                    p,
                                    _querySource),
                            _querySource);

                queryMethodInfo
                    = RelationalQueryModelVisitor.CreateEntityMethodInfo
                        .MakeGenericMethod(elementType);

                var keyProperties
                    = entityType.GetPrimaryKey().Properties;

                var keyFactory
                    = relationalQueryCompilationContext.EntityKeyFactorySource
                        .GetKeyFactory(keyProperties);

                queryMethodArguments.AddRange(
                    new[]
                    {
                        Expression.Constant(entityType),
                        Expression.Constant(QueryModelVisitor.QuerySourceRequiresTracking(_querySource)),
                        Expression.Constant(keyFactory),
                        Expression.Constant(keyProperties),
                        materializer
                    });
            }

            Func<ISqlQueryGenerator> sqlQueryGeneratorFactory;

            if (composable)
            {
                sqlQueryGeneratorFactory = () =>
                    relationalQueryCompilationContext.CreateSqlQueryGenerator(selectExpression);
            }
            else
            {
                sqlQueryGeneratorFactory = () =>
                    new RawSqlQueryGenerator(selectExpression, sqlString, sqlParameters, relationalQueryCompilationContext.TypeMapper);
            }

            return Expression.Call(
                relationalQueryCompilationContext.QueryMethodProvider.ShapedQueryMethod
                    .MakeGenericMethod(queryMethodInfo.ReturnType),
                EntityQueryModelVisitor.QueryContextParameter,
                Expression.Constant(
                    new CommandBuilder(
                        sqlQueryGeneratorFactory,
                        relationalQueryCompilationContext.ValueBufferFactoryFactory)),
                Expression.Lambda(
                    Expression.Call(queryMethodInfo, queryMethodArguments),
                    _valueBufferParameter));
        }
    }
}
