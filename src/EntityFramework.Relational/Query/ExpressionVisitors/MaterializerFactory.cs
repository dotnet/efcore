// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Relational.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class MaterializerFactory : IMaterializerFactory
    {
        private readonly IEntityMaterializerSource _entityMaterializerSource;
        private readonly IRelationalMetadataExtensionProvider _relationalMetadataExtensionProvider;

        public MaterializerFactory(
            [NotNull] IEntityMaterializerSource entityMaterializerSource,
            [NotNull] IRelationalMetadataExtensionProvider relationalMetadataExtensionProvider)
        {
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));
            Check.NotNull(relationalMetadataExtensionProvider, nameof(relationalMetadataExtensionProvider));

            _entityMaterializerSource = entityMaterializerSource;
            _relationalMetadataExtensionProvider = relationalMetadataExtensionProvider;
        }

        public virtual Expression CreateMaterializer(
            [NotNull] IEntityType entityType,
            [NotNull] SelectExpression selectExpression,
            [NotNull] Func<IProperty, SelectExpression, int> projectionAdder,
            [CanBeNull] IQuerySource querySource)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(selectExpression, nameof(selectExpression));
            Check.NotNull(projectionAdder, nameof(projectionAdder));

            var valueBufferParameter
                = Expression.Parameter(typeof(ValueBuffer));

            var concreteEntityTypes
                = entityType.GetConcreteTypesInHierarchy().ToArray();

            var indexMap = new int[concreteEntityTypes[0].GetProperties().Count()];
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

            if (concreteEntityTypes.Length == 1
                && concreteEntityTypes[0].RootType() == concreteEntityTypes[0])
            {
                return Expression.Lambda<Func<ValueBuffer, object>>(materializer, valueBufferParameter);
            }

            var discriminatorProperty = _relationalMetadataExtensionProvider.For(concreteEntityTypes[0]).DiscriminatorProperty;

            var discriminatorColumn
                = selectExpression.Projection
                    .OfType<AliasExpression>()
                    .Single(c => c.TryGetColumnExpression()?.Property == discriminatorProperty);

            var firstDiscriminatorValue
                = Expression.Constant(
                    _relationalMetadataExtensionProvider.For(concreteEntityTypes[0]).DiscriminatorValue);

            var discriminatorPredicate
                = Expression.Equal(discriminatorColumn, firstDiscriminatorValue);

            if (concreteEntityTypes.Length == 1)
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
                                discriminatorProperty.Index)),
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
                indexMap = new int[concreteEntityType.GetProperties().Count()];
                propertyIndex = 0;

                foreach (var property in concreteEntityType.GetProperties())
                {
                    indexMap[propertyIndex++]
                        = projectionAdder(property, selectExpression);
                }

                var discriminatorValue
                    = Expression.Constant(
                        _relationalMetadataExtensionProvider.For(concreteEntityType).DiscriminatorValue);

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
            => new InvalidOperationException(Strings.UnableToDiscriminate(entityType.Name));
    }
}
