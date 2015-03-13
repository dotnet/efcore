// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.InMemory.Query
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

            var valueReaderParameter
                = Expression.Parameter(typeof(IValueReader));

            var concreteEntityTypes
                = entityType.GetConcreteTypesInHierarchy().ToArray();

            if (concreteEntityTypes.Length == 1)
            {
                return Expression.Lambda<Func<IEntityType, IValueReader, object>>(
                    _entityMaterializerSource
                        .CreateMaterializeExpression(
                            concreteEntityTypes[0], valueReaderParameter),
                    entityTypeParameter,
                    valueReaderParameter);
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
                                        concreteEntityTypes[0], valueReaderParameter))),
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
                                .CreateMaterializeExpression(concreteEntityType, valueReaderParameter)),
                        blockExpressions[0]);
            }

            return Expression.Lambda<Func<IEntityType, IValueReader, object>>(
                Expression.Block(blockExpressions),
                entityTypeParameter,
                valueReaderParameter);
        }
    }
}
