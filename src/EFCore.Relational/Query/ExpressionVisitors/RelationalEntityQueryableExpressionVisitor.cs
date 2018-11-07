// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;
using Microsoft.EntityFrameworkCore.Query.Sql;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors
{
    /// <summary>
    ///     A visitor that performs basic relational query translation of EF query roots.
    /// </summary>
    public class RelationalEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        private readonly IModel _model;
        private readonly ISelectExpressionFactory _selectExpressionFactory;
        private readonly IMaterializerFactory _materializerFactory;
        private readonly IShaperCommandContextFactory _shaperCommandContextFactory;
        private readonly IQuerySource _querySource;

        /// <summary>
        ///     Creates a new instance of <see cref="RelationalEntityQueryableExpressionVisitor" />.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        /// <param name="queryModelVisitor"> The query model visitor. </param>
        /// <param name="querySource"> The query source. </param>
        public RelationalEntityQueryableExpressionVisitor(
            [NotNull] RelationalEntityQueryableExpressionVisitorDependencies dependencies,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] IQuerySource querySource)
            : base(Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)))
        {
            Check.NotNull(dependencies, nameof(dependencies));

            _model = dependencies.Model;
            _selectExpressionFactory = dependencies.SelectExpressionFactory;
            _materializerFactory = dependencies.MaterializerFactory;
            _shaperCommandContextFactory = dependencies.ShaperCommandContextFactory;
            _querySource = querySource;
        }

        private new RelationalQueryModelVisitor QueryModelVisitor => (RelationalQueryModelVisitor)base.QueryModelVisitor;

        /// <summary>
        ///     Visit a sub-query expression.
        /// </summary>
        /// <param name="expression"> The expression. </param>
        /// <returns>
        ///     An Expression corresponding to the translated sub-query.
        /// </returns>
        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            Check.NotNull(expression, nameof(expression));

            var queryModelVisitor = (RelationalQueryModelVisitor)CreateQueryModelVisitor();
            var queryModelMapping = new Dictionary<QueryModel, QueryModel>();

            expression.QueryModel.PopulateQueryModelMapping(queryModelMapping);

            queryModelVisitor.VisitQueryModel(expression.QueryModel);

            expression.QueryModel.RecreateQueryModelFromMapping(queryModelMapping);

            if (_querySource != null)
            {
                QueryModelVisitor.RegisterSubQueryVisitor(_querySource, queryModelVisitor);
            }

            return queryModelVisitor.Expression;
        }

        /// <summary>
        ///     Visit a member expression.
        /// </summary>
        /// <param name="node"> The expression to visit. </param>
        /// <returns>
        ///     An Expression corresponding to the translated member.
        /// </returns>
        protected override Expression VisitMember(MemberExpression node)
        {
            Check.NotNull(node, nameof(node));

            QueryModelVisitor
                .BindMemberExpression(
                    node,
                    (property, querySource, selectExpression)
                        => selectExpression.AddToProjection(
                            property,
                            querySource),
                    bindSubQueries: true);

            return base.VisitMember(node);
        }

        /// <summary>
        ///     Visit a method call expression.
        /// </summary>
        /// <param name="node"> The expression to visit. </param>
        /// <returns>
        ///     An Expression corresponding to the translated method call.
        /// </returns>
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Check.NotNull(node, nameof(node));

            QueryModelVisitor
                .BindMethodCallExpression(
                    node,
                    (property, querySource, selectExpression)
                        => selectExpression.AddToProjection(
                            property,
                            querySource),
                    bindSubQueries: true);

            return base.VisitMethodCall(node);
        }

        /// <summary>
        ///     Visit an entity query root.
        /// </summary>
        /// <param name="elementType"> The CLR type of the entity root. </param>
        /// <returns>
        ///     An Expression corresponding to the translated entity root.
        /// </returns>
        protected override Expression VisitEntityQueryable(Type elementType)
        {
            Check.NotNull(elementType, nameof(elementType));

            var relationalQueryCompilationContext = QueryModelVisitor.QueryCompilationContext;

            var entityType = relationalQueryCompilationContext.FindEntityType(_querySource)
                             ?? _model.FindEntityType(elementType);

            var selectExpression = _selectExpressionFactory.Create(relationalQueryCompilationContext);

            QueryModelVisitor.AddQuery(_querySource, selectExpression);

            var tableName = entityType.Relational().TableName;

            var tableAlias
                = relationalQueryCompilationContext.CreateUniqueTableAlias(
                    _querySource.HasGeneratedItemName()
                        ? tableName[0].ToString().ToLowerInvariant()
                        : (_querySource as GroupJoinClause)?.JoinClause.ItemName
                          ?? _querySource.ItemName);

            var fromSqlAnnotation
                = relationalQueryCompilationContext
                    .QueryAnnotations
                    .OfType<FromSqlResultOperator>()
                    .LastOrDefault(a => a.QuerySource == _querySource);

            Func<IQuerySqlGenerator> querySqlGeneratorFunc = selectExpression.CreateDefaultQuerySqlGenerator;

            if (fromSqlAnnotation == null)
            {
                selectExpression.AddTable(
                    new TableExpression(
                        tableName,
                        entityType.Relational().Schema,
                        tableAlias,
                        _querySource));
            }
            else
            {
                selectExpression.AddTable(
                    new FromSqlExpression(
                        fromSqlAnnotation.Sql,
                        fromSqlAnnotation.Arguments,
                        tableAlias,
                        _querySource));

                var trimmedSql = fromSqlAnnotation.Sql.TrimStart('\r', '\n', '\t', ' ');

                var useQueryComposition
                    = trimmedSql.StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase)
                      || trimmedSql.StartsWith("SELECT" + Environment.NewLine, StringComparison.OrdinalIgnoreCase)
                      || trimmedSql.StartsWith("SELECT\t", StringComparison.OrdinalIgnoreCase);

                var requiresClientEval = !useQueryComposition;

                if (!useQueryComposition
                    && relationalQueryCompilationContext.IsIncludeQuery)
                {
                    throw new InvalidOperationException(
                        RelationalStrings.StoredProcedureIncludeNotSupported);
                }

                if (useQueryComposition
                    && fromSqlAnnotation.QueryModel.IsIdentityQuery()
                    && !fromSqlAnnotation.QueryModel.ResultOperators.Any()
                    && !relationalQueryCompilationContext.IsIncludeQuery
                    && entityType.BaseType == null
                    && !entityType.GetDerivedTypes().Any())
                {
                    useQueryComposition = false;
                }

                if (!useQueryComposition)
                {
                    QueryModelVisitor.RequiresClientEval = requiresClientEval;

                    querySqlGeneratorFunc = ()
                        => selectExpression.CreateFromSqlQuerySqlGenerator(
                            fromSqlAnnotation.Sql,
                            fromSqlAnnotation.Arguments);
                }
            }

            var shaper = CreateShaper(elementType, entityType, selectExpression);

            DiscriminateProjectionQuery(entityType, selectExpression, _querySource);

            return Expression.Call(
                QueryModelVisitor.QueryCompilationContext.QueryMethodProvider
                    .ShapedQueryMethod
                    .MakeGenericMethod(shaper.Type),
                EntityQueryModelVisitor.QueryContextParameter,
                Expression.Constant(_shaperCommandContextFactory.Create(querySqlGeneratorFunc)),
                Expression.Constant(shaper));
        }

        private Shaper CreateShaper(Type elementType, IEntityType entityType, SelectExpression selectExpression)
        {
            Shaper shaper;

            if (QueryModelVisitor.QueryCompilationContext
                    .QuerySourceRequiresMaterialization(_querySource)
                || QueryModelVisitor.RequiresClientEval)
            {
                var materializerExpression
                    = _materializerFactory
                        .CreateMaterializer(
                            entityType,
                            selectExpression,
                            (p, se) =>
                                se.AddToProjection(
                                    p,
                                    _querySource),
                            out var typeIndexMap);

                var materializer = materializerExpression.Compile();

                shaper
                    = (Shaper)_createEntityShaperMethodInfo.MakeGenericMethod(elementType)
                        .Invoke(
                            obj: null,
                            parameters: new object[]
                            {
                                _querySource,
                                QueryModelVisitor.QueryCompilationContext.IsTrackingQuery
                                && !entityType.IsQueryType,
                                entityType.FindPrimaryKey(),
                                materializer,
                                materializerExpression,
                                typeIndexMap,
                                QueryModelVisitor.QueryCompilationContext.IsQueryBufferRequired
                                && !entityType.IsQueryType
                            });
            }
            else
            {
                shaper = new ValueBufferShaper(_querySource);
            }

            return shaper;
        }

        private static readonly MethodInfo _createEntityShaperMethodInfo
            = typeof(RelationalEntityQueryableExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateEntityShaper));

        [UsedImplicitly]
        private static IShaper<TEntity> CreateEntityShaper<TEntity>(
            IQuerySource querySource,
            bool trackingQuery,
            IKey key,
            Func<MaterializationContext, object> materializer,
            Expression materializerExpression,
            Dictionary<Type, int[]> typeIndexMap,
            bool useQueryBuffer)
            where TEntity : class
            => !useQueryBuffer
                ? (IShaper<TEntity>)new UnbufferedEntityShaper<TEntity>(
                    querySource,
                    trackingQuery,
                    key,
                    materializer,
                    materializerExpression)
                : new BufferedEntityShaper<TEntity>(
                    querySource,
                    trackingQuery,
                    key,
                    materializer,
                    typeIndexMap);

        private static void FindPaths(
            IEntityType entityType, ICollection<IEntityType> sharedTypes,
            Stack<IEntityType> currentPath, ICollection<List<IEntityType>> result)
        {
            var identifyingFks = entityType.FindForeignKeys(entityType.FindPrimaryKey().Properties)
                .Where(
                    fk => fk.PrincipalKey.IsPrimaryKey()
                          && fk.PrincipalEntityType != entityType
                          && sharedTypes.Contains(fk.PrincipalEntityType))
                .ToList();

            if (identifyingFks.Count == 0)
            {
                result.Add(new List<IEntityType>(currentPath));
                return;
            }

            foreach (var fk in identifyingFks)
            {
                currentPath.Push(fk.PrincipalEntityType);
                FindPaths(fk.PrincipalEntityType.RootType(), sharedTypes, currentPath, result);
                currentPath.Pop();
            }
        }

        private static Expression GenerateDiscriminatorExpression(
            IEntityType entityType, SelectExpression selectExpression, IQuerySource querySource)
        {
            var concreteEntityTypes
                = entityType.GetConcreteTypesInHierarchy().ToList();

            if (concreteEntityTypes.Count == 1
                && concreteEntityTypes[0].RootType() == concreteEntityTypes[0])
            {
                return null;
            }

            var discriminatorColumn
                = selectExpression.BindProperty(
                    concreteEntityTypes[0].Relational().DiscriminatorProperty,
                    querySource);

            var firstDiscriminatorValue
                = Expression.Constant(
                    concreteEntityTypes[0].Relational().DiscriminatorValue,
                    discriminatorColumn.Type);

            var discriminatorPredicate
                = Expression.Equal(discriminatorColumn, firstDiscriminatorValue);

            if (concreteEntityTypes.Count > 1)
            {
                discriminatorPredicate
                    = concreteEntityTypes
                        .Skip(1)
                        .Select(
                            concreteEntityType
                                => Expression.Constant(
                                    concreteEntityType.Relational().DiscriminatorValue,
                                    discriminatorColumn.Type))
                        .Aggregate(
                            discriminatorPredicate, (current, discriminatorValue) =>
                                Expression.OrElse(
                                    Expression.Equal(discriminatorColumn, discriminatorValue),
                                    current));
            }

            return discriminatorPredicate;
        }

        private void DiscriminateProjectionQuery(
            IEntityType entityType, SelectExpression selectExpression, IQuerySource querySource)
        {
            Expression discriminatorPredicate;

            if (entityType.IsQueryType)
            {
                discriminatorPredicate = GenerateDiscriminatorExpression(entityType, selectExpression, querySource);
            }
            else
            {
                var sharedTypes = new HashSet<IEntityType>(
                    _model.GetEntityTypes()
                        .Where(e => !e.IsQueryType)
                        .Where(
                            et => et.Relational().TableName == entityType.Relational().TableName
                                  && et.Relational().Schema == entityType.Relational().Schema));

                var currentPath = new Stack<IEntityType>();
                currentPath.Push(entityType);

                var allPaths = new List<List<IEntityType>>();
                FindPaths(entityType.RootType(), sharedTypes, currentPath, allPaths);

                discriminatorPredicate = allPaths
                    .Select(
                        p => p.Select(
                                et => GenerateDiscriminatorExpression(et, selectExpression, querySource))
                            .Aggregate(
                                (Expression)null,
                                (result, current) => result != null
                                    ? current != null
                                        ? Expression.AndAlso(result, current)
                                        : result
                                    : current))
                    .Aggregate(
                        (Expression)null,
                        (result, current) => result != null
                            ? current != null
                                ? Expression.OrElse(result, current)
                                : result
                            : current);
            }

            if (discriminatorPredicate != null)
            {
                selectExpression.Predicate = new DiscriminatorPredicateExpression(discriminatorPredicate, querySource);
            }
        }
    }
}
