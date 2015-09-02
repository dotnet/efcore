// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query
{
    public class MaterializerFactory
    {
        private readonly IEntityMaterializerSource _entityMaterializerSource;

        public MaterializerFactory([NotNull] IEntityMaterializerSource entityMaterializerSource)
        {
            Check.NotNull(entityMaterializerSource, nameof(entityMaterializerSource));

            _entityMaterializerSource = entityMaterializerSource;
        }

        public virtual Expression CreateMaterializer([NotNull] IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var entityTypeParameter
                = Expression.Parameter(typeof(IEntityType));

            var valueBufferParameter
                = Expression.Parameter(typeof(ValueBuffer));

            var concreteEntityTypes
                = entityType.GetConcreteTypesInHierarchy().ToArray();

            if (concreteEntityTypes.Length == 1)
            {
                return Expression.Lambda<Func<IEntityType, ValueBuffer, object>>(
                    _entityMaterializerSource
                        .CreateMaterializeExpression(
                            concreteEntityTypes[0], valueBufferParameter),
                    entityTypeParameter,
                    valueBufferParameter);
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
                                    concreteEntityTypes[0], valueBufferParameter))),
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
                                .CreateMaterializeExpression(concreteEntityType, valueBufferParameter)),
                        blockExpressions[0]);
            }

            return Expression.Lambda<Func<IEntityType, ValueBuffer, object>>(
                Expression.Block(blockExpressions),
                entityTypeParameter,
                valueBufferParameter);
        }
    }
}
