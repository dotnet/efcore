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
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.ExpressionVisitors.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class MaterializerFactory : IMaterializerFactory
    {
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public MaterializerFactory(
            [NotNull] IEntityMaterializerSource entityMaterializerSource)
        {
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));

            _entityMaterializerSource = entityMaterializerSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual LambdaExpression CreateMaterializer(
            IEntityType entityType,
            SelectExpression selectExpression,
            Func<IProperty, SelectExpression, int> projectionAdder,
            out Dictionary<Type, int[]> typeIndexMap)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(selectExpression, nameof(selectExpression));
            Check.NotNull(projectionAdder, nameof(projectionAdder));

            typeIndexMap = null;

            var materializationContextParameter
                = Expression.Parameter(typeof(MaterializationContext), "materializationContext");

            var concreteEntityTypes = entityType.GetConcreteTypesInHierarchy().ToList();
            var firstEntityType = concreteEntityTypes[0];
            var indexMap = new int[firstEntityType.PropertyCount()];

            foreach (var property in firstEntityType.GetProperties())
            {
                indexMap[property.GetIndex()] = projectionAdder(property, selectExpression);
            }

            var materializer
                = _entityMaterializerSource
                    .CreateMaterializeExpression(
                        firstEntityType, materializationContextParameter, indexMap);

            if (concreteEntityTypes.Count == 1)
            {
                return Expression.Lambda(materializer, materializationContextParameter);
            }

            var discriminatorProperty = firstEntityType.Relational().DiscriminatorProperty;

            var firstDiscriminatorValue
                = Expression.Constant(
                    firstEntityType.Relational().DiscriminatorValue,
                    discriminatorProperty.ClrType);

            var discriminatorValueVariable
                = Expression.Variable(discriminatorProperty.ClrType);

            var returnLabelTarget = Expression.Label(entityType.ClrType);

            var blockExpressions
                = new Expression[]
                {
                    Expression.Assign(
                        discriminatorValueVariable,
                        _entityMaterializerSource
                            .CreateReadValueExpression(
                                Expression.Call(materializationContextParameter, MaterializationContext.GetValueBufferMethod),
                                discriminatorProperty.ClrType,
                                indexMap[discriminatorProperty.GetIndex()],
                                discriminatorProperty)),
                    Expression.IfThenElse(
                        Expression.Equal(discriminatorValueVariable, firstDiscriminatorValue),
                        Expression.Return(returnLabelTarget, materializer),
                        Expression.Throw(
                            Expression.Call(
                                _createUnableToDiscriminateException,
                                Expression.Constant(firstEntityType)))),
                    Expression.Label(
                        returnLabelTarget,
                        Expression.Default(returnLabelTarget.Type))
                };

            foreach (var concreteEntityType in concreteEntityTypes.Skip(1))
            {
                indexMap = new int[concreteEntityType.PropertyCount()];

                var shadowPropertyExists = false;

                foreach (var property in concreteEntityType.GetProperties())
                {
                    indexMap[property.GetIndex()] = projectionAdder(property, selectExpression);

                    shadowPropertyExists = shadowPropertyExists || property.IsShadowProperty;
                }

                if (shadowPropertyExists)
                {
                    if (typeIndexMap == null)
                    {
                        typeIndexMap = new Dictionary<Type, int[]>();
                    }

                    typeIndexMap[concreteEntityType.ClrType] = indexMap;
                }

                var discriminatorValue
                    = Expression.Constant(
                        concreteEntityType.Relational().DiscriminatorValue,
                        discriminatorProperty.ClrType);

                materializer
                    = _entityMaterializerSource
                        .CreateMaterializeExpression(
                            concreteEntityType, materializationContextParameter, indexMap);

                blockExpressions[1]
                    = Expression.IfThenElse(
                        Expression.Equal(discriminatorValueVariable, discriminatorValue),
                        Expression.Return(returnLabelTarget, materializer),
                        blockExpressions[1]);
            }

            return Expression.Lambda(
                Expression.Block(new[] { discriminatorValueVariable }, blockExpressions),
                materializationContextParameter);
        }

        private static readonly MethodInfo _createUnableToDiscriminateException
            = typeof(MaterializerFactory).GetTypeInfo()
                .GetDeclaredMethod(nameof(CreateUnableToDiscriminateException));

        [UsedImplicitly]
        private static Exception CreateUnableToDiscriminateException(IEntityType entityType)
            => new InvalidOperationException(RelationalStrings.UnableToDiscriminate(entityType.DisplayName()));
    }
}
