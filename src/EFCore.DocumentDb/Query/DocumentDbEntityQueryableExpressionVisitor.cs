// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.ExpressionVisitors;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class DocumentDbEntityQueryableExpressionVisitor : EntityQueryableExpressionVisitor
    {
        private readonly IDocumentDbClientService _documentDbClientService;
        private readonly IModel _model;
        private readonly IQuerySource _querySource;
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public DocumentDbEntityQueryableExpressionVisitor(
            IDocumentDbClientService documentDbClientService,
            IModel model,
            EntityQueryModelVisitor entityQueryModelVisitor,
            IQuerySource querySource,
            IEntityMaterializerSource entityMaterializerSource)
            : base(entityQueryModelVisitor)
        {
            _documentDbClientService = documentDbClientService;
            _model = model;
            _querySource = querySource;
            _entityMaterializerSource = entityMaterializerSource;
        }

        private new DocumentDbQueryModelVisitor QueryModelVisitor => (DocumentDbQueryModelVisitor)base.QueryModelVisitor;

        protected override Expression VisitEntityQueryable(Type elementType)
        {
            var entityType = QueryModelVisitor.QueryCompilationContext.FindEntityType(_querySource)
                             ?? _model.FindEntityType(elementType);

            var collectionName = entityType.DocumentDb().CollectionName;

            var selectExpression = new SelectExpression(_querySource, "t");

            QueryModelVisitor.AddQuery(_querySource, selectExpression);

            selectExpression.AddSource(new CollectionExpression(_querySource, "c", collectionName));

            var documentCommandContext = new DocumentCommandContext(
                _documentDbClientService.Client,
                collectionName,
                () => selectExpression.GetSqlGenerator());

            var shaper = CreateShaper(elementType, entityType, selectExpression);

            return new ShapedQueryExpression(
                shaper.Type,
                documentCommandContext,
                Expression.Constant(shaper));
        }

        private Shaper CreateShaper(Type elementType, IEntityType entityType, SelectExpression selectExpression)
        {
            if (QueryModelVisitor.QueryCompilationContext.QuerySourceRequiresMaterialization(_querySource))
            {
                var entityShaper = _createEntityShaperMethodInfo.MakeGenericMethod(elementType)
                .Invoke(
                    null, new object[]
                    {
                        QueryModelVisitor.QueryCompilationContext.IsTrackingQuery,
                        entityType.FindPrimaryKey(),
                        CreateMaterializer(entityType, selectExpression).Compile(),
                        QueryModelVisitor.QueryCompilationContext.IsQueryBufferRequired
                    });

                return (Shaper)entityShaper;
            }

            return new ValueBufferShaper();
        }

        private static readonly MethodInfo _createEntityShaperMethodInfo
            = typeof(DocumentDbEntityQueryableExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateEntityShaper));

        [UsedImplicitly]
        private static IShaper<TEntity> CreateEntityShaper<TEntity>(
            bool trackingQuery,
            IKey key,
            Func<ValueBuffer, DbContext, object> materializer,
            bool useQueryBuffer)
            where TEntity : class
            => !useQueryBuffer
                ? (IShaper<TEntity>)new UnbufferedEntityShaper<TEntity>(
                    trackingQuery,
                    key,
                    materializer)
                : new BufferedEntityShaper<TEntity>(
                    trackingQuery,
                    key,
                    materializer);

        private Expression<Func<ValueBuffer, DbContext, object>> CreateMaterializer(
            IEntityType entityType,
            SelectExpression selectExpression)
        {
            Check.NotNull(entityType, nameof(entityType));

            var valueBufferParameter = Expression.Parameter(typeof(ValueBuffer), "valueBuffer");
            var contextParameter = Expression.Parameter(typeof(DbContext), "context");

            var concreteEntityTypes = entityType.GetConcreteTypesInHierarchy().ToList();

            foreach (var property in concreteEntityTypes[0].GetProperties())
            {
                selectExpression.AddToProjection(property, _querySource);
            }

            var materializer = _entityMaterializerSource
                .CreateMaterializeExpression(
                    concreteEntityTypes[0],
                    valueBufferParameter,
                    contextParameter);

            if (concreteEntityTypes.Count == 1
                && concreteEntityTypes[0].RootType() == concreteEntityTypes[0])
            {
                return Expression.Lambda<Func<ValueBuffer, DbContext, object>>(
                    materializer, valueBufferParameter, contextParameter);
            }

            var discriminatorProperty = concreteEntityTypes[0].DocumentDb().DiscriminatorProperty;

            var discriminatorColumn
                = selectExpression.Projection.Last(c => (c as ColumnExpression)?.Property == discriminatorProperty);

            var firstDiscriminatorValue = Expression.Constant(
                    concreteEntityTypes[0].DocumentDb().DiscriminatorValue,
                    discriminatorColumn.Type);

            var discriminatorPredicate = Expression.Equal(discriminatorColumn, firstDiscriminatorValue);

            if (concreteEntityTypes.Count == 1)
            {
                selectExpression.Predicate = discriminatorPredicate;

                return Expression.Lambda<Func<ValueBuffer, DbContext, object>>(
                    materializer, valueBufferParameter, contextParameter);
            }

            var discriminatorValueVariable = Expression.Variable(discriminatorProperty.ClrType);

            var returnLabelTarget = Expression.Label(typeof(object));

            var blockExpressions
                = new Expression[]
                {
                    Expression.Assign(
                        discriminatorValueVariable,
                        _entityMaterializerSource
                            .CreateReadValueExpression(
                                valueBufferParameter,
                                discriminatorProperty.ClrType,
                                discriminatorProperty.GetIndex(),
                                discriminatorProperty)),
                    Expression.IfThenElse(
                        Expression.Equal(discriminatorValueVariable, firstDiscriminatorValue),
                        Expression.Return(returnLabelTarget, materializer),
                        Expression.Throw(
                            Expression.Call(
                                _createUnableToDiscriminateException,
                                Expression.Constant(concreteEntityTypes[0])))),
                    Expression.Label(
                        returnLabelTarget,
                        Expression.Default(returnLabelTarget.Type))
                };

            foreach (var concreteEntityType in concreteEntityTypes.Skip(1))
            {
                foreach (var property in concreteEntityType.GetProperties())
                {
                    selectExpression.AddToProjection(property, _querySource);
                }

                var discriminatorValue
                    = Expression.Constant(
                        concreteEntityType.DocumentDb().DiscriminatorValue,
                        discriminatorColumn.Type);

                materializer
                    = _entityMaterializerSource
                        .CreateMaterializeExpression(concreteEntityType, valueBufferParameter, contextParameter);

                blockExpressions[1]
                    = Expression.IfThenElse(
                        Expression.Equal(discriminatorValueVariable, discriminatorValue),
                        Expression.Return(returnLabelTarget, materializer),
                        blockExpressions[1]);

                discriminatorPredicate
                    = Expression.OrElse(
                        Expression.Equal(discriminatorColumn, discriminatorValue),
                        discriminatorPredicate);
            }

            selectExpression.Predicate = discriminatorPredicate;

            return Expression.Lambda<Func<ValueBuffer, DbContext, object>>(
                Expression.Block(new[] { discriminatorValueVariable }, blockExpressions),
                valueBufferParameter,
                contextParameter);
        }

        private static readonly MethodInfo _createUnableToDiscriminateException
            = typeof(DocumentDbEntityQueryableExpressionVisitor).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateUnableToDiscriminateException));

        [UsedImplicitly]
        private static Exception CreateUnableToDiscriminateException(IEntityType entityType)
            => new InvalidOperationException(/*RelationalStrings.UnableToDiscriminate(entityType.DisplayName())*/"ERROR");
    }

    public class DocumentDbProjectionExpressionVisitorFactory : IProjectionExpressionVisitorFactory
    {
        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public DocumentDbProjectionExpressionVisitorFactory(ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
            IEntityMaterializerSource entityMaterializerSource)
        {
            _sqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
            _entityMaterializerSource = entityMaterializerSource;
        }
        public ExpressionVisitor Create(EntityQueryModelVisitor entityQueryModelVisitor, IQuerySource querySource)
        {
            return new DocumentDbProjectionExpressionVisitor((DocumentDbQueryModelVisitor)entityQueryModelVisitor, querySource, _sqlTranslatingExpressionVisitorFactory
                , _entityMaterializerSource);
        }
    }

    public class DocumentDbProjectionExpressionVisitor : ProjectionExpressionVisitor
    {
        private readonly IQuerySource _querySource;
        private readonly ISqlTranslatingExpressionVisitorFactory _sqlTranslatingExpressionVisitorFactory;
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public DocumentDbProjectionExpressionVisitor([NotNull] DocumentDbQueryModelVisitor entityQueryModelVisitor, IQuerySource querySource,
            ISqlTranslatingExpressionVisitorFactory sqlTranslatingExpressionVisitorFactory,
            IEntityMaterializerSource entityMaterializerSource)
            : base(entityQueryModelVisitor)
        {
            _querySource = querySource;
            _sqlTranslatingExpressionVisitorFactory = sqlTranslatingExpressionVisitorFactory;
            _entityMaterializerSource = entityMaterializerSource;
        }

        public new DocumentDbQueryModelVisitor QueryModelVisitor => (DocumentDbQueryModelVisitor)base.QueryModelVisitor;

        protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
        {
            if (methodCallExpression.Method.IsEFPropertyMethod())
            {
                var selectExpression = QueryModelVisitor.TryGetQuery(_querySource);

                var sqlTranslatingExpressionVisitor = _sqlTranslatingExpressionVisitorFactory
                    .Create(
                    QueryModelVisitor,
                    selectExpression);

                var sqlExpression = sqlTranslatingExpressionVisitor.Visit(methodCallExpression);

                if (sqlExpression != null)
                {
                    var targetExpression
                            = QueryModelVisitor.QueryCompilationContext.QuerySourceMapping
                                .GetExpression(_querySource);

                    if (targetExpression.Type == typeof(ValueBuffer))
                    {
                        var index = selectExpression.AddToProjection(sqlExpression);

                        var readValueExpression
                            = _entityMaterializerSource
                                .CreateReadValueExpression(
                                    targetExpression,
                                    methodCallExpression.Type.MakeNullable(),
                                    index,
                                    (sqlExpression as ColumnExpression)?.Property);

                        return Expression.Convert(readValueExpression, methodCallExpression.Type);
                    }
                }
            }


            return base.VisitMethodCall(methodCallExpression);
        }
    }

}
