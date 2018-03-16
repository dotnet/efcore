// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.InMemory.Query.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class InMemoryMaterializerFactory : IInMemoryMaterializerFactory
    {
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public InMemoryMaterializerFactory([NotNull] IEntityMaterializerSource entityMaterializerSource)
        {
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));

            _entityMaterializerSource = entityMaterializerSource;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual Expression<Func<IEntityType, MaterializationContext, object>> CreateMaterializer(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var entityTypeParameter
                = Expression.Parameter(typeof(IEntityType), "entityType");

            var materializationContextParameter
                = Expression.Parameter(typeof(MaterializationContext), "materializationContext");

            var concreteEntityTypes
                = entityType.GetConcreteTypesInHierarchy().ToList();

            if (concreteEntityTypes.Count == 1)
            {
                return Expression.Lambda<Func<IEntityType, MaterializationContext, object>>(
                    _entityMaterializerSource
                        .CreateMaterializeExpression(
                            concreteEntityTypes[0], materializationContextParameter),
                    entityTypeParameter,
                    materializationContextParameter);
            }

            var returnLabelTarget = Expression.Label(typeof(object));

            var blockExpressions
                = new Expression[]
                {
                    Expression.IfThen(
                        Expression.Equal(
                            entityTypeParameter,
                            Expression.Constant(concreteEntityTypes[0])),
                        Expression.Return(
                            returnLabelTarget,
                            _entityMaterializerSource
                                .CreateMaterializeExpression(
                                    concreteEntityTypes[0], materializationContextParameter))),
                    Expression.Label(
                        returnLabelTarget,
                        Expression.Default(returnLabelTarget.Type))
                };

            foreach (var concreteEntityType in concreteEntityTypes.Skip(1))
            {
                blockExpressions[0]
                    = Expression.IfThenElse(
                        Expression.Equal(
                            entityTypeParameter,
                            Expression.Constant(concreteEntityType)),
                        Expression.Return(
                            returnLabelTarget,
                            _entityMaterializerSource
                                .CreateMaterializeExpression(concreteEntityType, materializationContextParameter)),
                        blockExpressions[0]);
            }

            return Expression.Lambda<Func<IEntityType, MaterializationContext, object>>(
                Expression.Block(blockExpressions),
                entityTypeParameter,
                materializationContextParameter);
        }
    }
}
