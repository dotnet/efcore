// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MaterializerFactory : IMaterializerFactory
    {
        private readonly IEntityMaterializerSource _entityMaterializerSource;
        private readonly IRelationalAnnotationProvider _relationalAnnotationProvider;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MaterializerFactory(
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IRelationalAnnotationProvider relationalAnnotationProvider)
        {
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));
            Check.NotNull(relationalAnnotationProvider, nameof(relationalAnnotationProvider));

            _entityMaterializerSource = entityMaterializerSource;
            _relationalAnnotationProvider = relationalAnnotationProvider;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression<Func<ValueBuffer, object>> CreateMaterializer(
            IEntityType entityType,
            SelectExpression selectExpression,
            Func<IProperty, SelectExpression, int> projectionAdder,
            IQuerySource querySource)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(selectExpression, nameof(selectExpression));
            Check.NotNull(projectionAdder, nameof(projectionAdder));

            var valueBufferParameter
                = Expression.Parameter(typeof(ValueBuffer), "valueBuffer");

            var concreteEntityTypes
                = entityType.GetConcreteTypesInHierarchy().ToList();

            var indexMap = new int[concreteEntityTypes[0].PropertyCount()];
            var propertyIndex = 0;

            foreach (var property in concreteEntityTypes[0].GetProperties())
            {
                indexMap[propertyIndex++]
                    = projectionAdder(property, selectExpression);
            }

            var materializer
                = _entityMaterializerSource
                    .CreateMaterializeExpression(
                        concreteEntityTypes[0], valueBufferParameter, indexMap);

            if (concreteEntityTypes.Count == 1
                && concreteEntityTypes[0].RootType() == concreteEntityTypes[0])
            {
                return Expression.Lambda<Func<ValueBuffer, object>>(materializer, valueBufferParameter);
            }

            var discriminatorProperty = _relationalAnnotationProvider.For(concreteEntityTypes[0]).DiscriminatorProperty;

            var discriminatorColumn
                = selectExpression.Projection
                    .OfType<AliasExpression>()
                    .Last(c => c.TryGetColumnExpression()?.Property == discriminatorProperty);

            var firstDiscriminatorValue
                = Expression.Constant(
                    _relationalAnnotationProvider.For(concreteEntityTypes[0]).DiscriminatorValue);

            var discriminatorPredicate
                = Expression.Equal(discriminatorColumn, firstDiscriminatorValue);

            if (concreteEntityTypes.Count == 1)
            {
                selectExpression.Predicate
                    = new DiscriminatorPredicateExpression(discriminatorPredicate, querySource);

                return Expression.Lambda<Func<ValueBuffer, object>>(materializer, valueBufferParameter);
            }

            var discriminatorValueVariable
                = Expression.Variable(discriminatorProperty.ClrType);

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
                                discriminatorProperty.GetIndex())),
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
                indexMap = new int[concreteEntityType.PropertyCount()];
                propertyIndex = 0;

                foreach (var property in concreteEntityType.GetProperties())
                {
                    indexMap[propertyIndex++]
                        = projectionAdder(property, selectExpression);
                }

                var discriminatorValue
                    = Expression.Constant(
                        _relationalAnnotationProvider.For(concreteEntityType).DiscriminatorValue);

                materializer
                    = _entityMaterializerSource
                        .CreateMaterializeExpression(concreteEntityType, valueBufferParameter, indexMap);

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

            selectExpression.Predicate
                = new DiscriminatorPredicateExpression(discriminatorPredicate, querySource);

            return Expression.Lambda<Func<ValueBuffer, object>>(
                Expression.Block(new[] { discriminatorValueVariable }, blockExpressions),
                valueBufferParameter);
        }

        private static readonly MethodInfo _createUnableToDiscriminateException
            = typeof(MaterializerFactory).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateUnableToDiscriminateException));

        [UsedImplicitly]
        private static Exception CreateUnableToDiscriminateException(IEntityType entityType)
            => new InvalidOperationException(RelationalStrings.UnableToDiscriminate(entityType.DisplayName()));
    }
}
