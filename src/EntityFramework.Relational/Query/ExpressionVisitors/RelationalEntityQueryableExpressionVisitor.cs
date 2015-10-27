// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.ExpressionVisitors.Internal;
using Microsoft.Data.Entity.Query.Internal;
using Microsoft.Data.Entity.Query.ResultOperators.Internal;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class RelationalEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        private readonly IModel _model;
        private readonly IKeyValueFactorySource _keyValueFactorySource;
        private readonly ISelectExpressionFactory _selectExpressionFactory;
        private readonly IMaterializerFactory _materializerFactory;
        private readonly IShaperCommandContextFactory _shaperCommandContextFactory;
        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;
        private readonly IQuerySource _querySource;

        public RelationalEntityQueryableExpressionVisitor(
            [NotNull] IModel model,
            [NotNull] IKeyValueFactorySource keyValueFactorySource,
            [NotNull] ISelectExpressionFactory selectExpressionFactory,
            [NotNull] IMaterializerFactory materializerFactory,
            [NotNull] IShaperCommandContextFactory shaperCommandContextFactory,
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider,
            [NotNull] RelationalQueryModelVisitor queryModelVisitor,
            [CanBeNull] IQuerySource querySource)
            : base(Check.NotNull(queryModelVisitor, nameof(queryModelVisitor)))
        {
            Check.NotNull(model, nameof(model));
            Check.NotNull(keyValueFactorySource, nameof(keyValueFactorySource));
            Check.NotNull(selectExpressionFactory, nameof(selectExpressionFactory));
            Check.NotNull(materializerFactory, nameof(materializerFactory));
            Check.NotNull(shaperCommandContextFactory, nameof(shaperCommandContextFactory));
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));

            _model = model;
            _keyValueFactorySource = keyValueFactorySource;
            _selectExpressionFactory = selectExpressionFactory;
            _materializerFactory = materializerFactory;
            _shaperCommandContextFactory = shaperCommandContextFactory;
            _relationalAnnotationProvider = relationalAnnotationProvider;
            _querySource = querySource;
        }

        private new RelationalQueryModelVisitor QueryModelVisitor => (RelationalQueryModelVisitor)base.QueryModelVisitor;

        protected override Expression VisitSubQuery(SubQueryExpression subQueryExpression)
        {
            Check.NotNull(subQueryExpression, nameof(subQueryExpression));

            var queryModelVisitor = (RelationalQueryModelVisitor)CreateQueryModelVisitor();

            queryModelVisitor.VisitQueryModel(subQueryExpression.QueryModel);

            if (_querySource != null)
            {
                QueryModelVisitor.RegisterSubQueryVisitor(_querySource, queryModelVisitor);
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
                            _relationalAnnotationProvider.For(property).ColumnName,
                            property,
                            querySource),
                    bindSubQueries: true);

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
                            _relationalAnnotationProvider.For(property).ColumnName,
                            property,
                            querySource),
                    bindSubQueries: true);

            return base.VisitMethodCall(methodCallExpression);
        }

        protected override Expression VisitEntityQueryable(Type elementType)
        {
            Check.NotNull(elementType, nameof(elementType));

            var relationalQueryCompilationContext = QueryModelVisitor.QueryCompilationContext;
            var entityType = _model.FindEntityType(elementType);

            var selectExpression = _selectExpressionFactory.Create();

            QueryModelVisitor.AddQuery(_querySource, selectExpression);

            var name = _relationalAnnotationProvider.For(entityType).TableName;

            var tableAlias
                = _querySource.HasGeneratedItemName()
                    ? name[0].ToString().ToLowerInvariant()
                    : _querySource.ItemName;

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
                        name,
                        _relationalAnnotationProvider.For(entityType).Schema,
                        tableAlias,
                        _querySource));
            }
            else
            {
                selectExpression.AddTable(
                    new FromSqlExpression(
                        fromSqlAnnotation.Sql,
                        fromSqlAnnotation.ArgumentsParameterName,
                        tableAlias,
                        _querySource));

                var useQueryComposition
                    = fromSqlAnnotation.Sql
                        .TrimStart()
                        .StartsWith("SELECT ", StringComparison.OrdinalIgnoreCase);

                if (!useQueryComposition)
                {
                    if (relationalQueryCompilationContext.QueryAnnotations
                        .OfType<IncludeResultOperator>().Any())
                    {
                        throw new InvalidOperationException(
                            RelationalStrings.StoredProcedureIncludeNotSupported);
                    }
                }

                if (useQueryComposition
                    && fromSqlAnnotation.QueryModel.IsIdentityQuery()
                    && !fromSqlAnnotation.QueryModel.ResultOperators.Any())
                {
                    useQueryComposition = false;
                }

                if (!useQueryComposition)
                {
                    QueryModelVisitor.RequiresClientEval = true;

                    querySqlGeneratorFunc = ()
                        => selectExpression.CreateFromSqlQuerySqlGenerator(
                            fromSqlAnnotation.Sql,
                            fromSqlAnnotation.ArgumentsParameterName);
                }
            }

            var shaper = CreateShaper(elementType, entityType, selectExpression);

            return Expression.Call(
                QueryModelVisitor.QueryCompilationContext.QueryMethodProvider // TODO: Don't use ShapedQuery when projecting
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
                var materializer
                    = _materializerFactory
                        .CreateMaterializer(
                            entityType,
                            selectExpression,
                            (p, se) =>
                                se.AddToProjection(
                                    _relationalAnnotationProvider.For(p).ColumnName,
                                    p,
                                    _querySource),
                            _querySource).Compile();

                shaper
                    = (Shaper)_createEntityShaperMethodInfo.MakeGenericMethod(elementType)
                        .Invoke(null, new object[]
                        {
                            _querySource,
                            entityType.DisplayName(),
                            QueryModelVisitor.QueryCompilationContext.IsTrackingQuery,
                            _keyValueFactorySource.GetKeyFactory(entityType.FindPrimaryKey()),
                            materializer,
                            QueryModelVisitor.QueryCompilationContext.IsQueryBufferRequired
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
            string entityType,
            bool trackingQuery,
            KeyValueFactory keyValueFactory,
            Func<ValueBuffer, object> materializer,
            bool useQueryBuffer)
            where TEntity : class
            => !useQueryBuffer
                ? (IShaper<TEntity>)new UnbufferedEntityShaper<TEntity>(
                    querySource,
                    entityType,
                    keyValueFactory,
                    materializer)
                : new BufferedEntityShaper<TEntity>(
                    querySource,
                    entityType,
                    trackingQuery,
                    keyValueFactory,
                    materializer);
    }
}
