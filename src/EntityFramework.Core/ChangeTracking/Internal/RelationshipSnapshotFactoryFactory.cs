// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public class RelationshipSnapshotFactoryFactory
    {
        public virtual Func<InternalEntityEntry, object[]> Create([NotNull] IEntityType entityType)
        {
            var entryParameter = Expression.Parameter(typeof(InternalEntityEntry), "entry");
            var valuesVariable = Expression.Variable(typeof(object[]), "values");
            var entityVariable = entityType.ClrType == null ? null : Expression.Variable(entityType.ClrType, "entity");

            var variables = new List<ParameterExpression>
            {
                valuesVariable
            };

            var blockExpressions = new List<Expression>
            {
                Expression.Assign(
                    valuesVariable,
                    Expression.NewArrayBounds(
                        typeof(object),
                        Expression.Constant(entityType.RelationshipPropertyCount())))
            };

            if (entityVariable != null)
            {
                variables.Add(entityVariable);

                blockExpressions.Add(
                    Expression.Assign(
                        entityVariable,
                        Expression.Convert(
                            Expression.Property(entryParameter, "Entity"),
                            entityType.ClrType)));
            }

            foreach (var propertyBase in entityType.GetPropertiesAndNavigations())
            {
                var index = propertyBase.GetRelationshipIndex();

                if (index >= 0)
                {
                    var navigation = propertyBase as INavigation;
                    var property = propertyBase as IProperty;

                    blockExpressions.Add(
                        Expression.Assign(
                            Expression.ArrayAccess(
                                valuesVariable,
                                Expression.Constant(index)),
                            navigation != null
                            && navigation.IsCollection()
                                ? Expression.Call(
                                    null,
                                    _snapshotCollectionMethod,
                                    Expression.Property(
                                        entityVariable,
                                        propertyBase.DeclaringEntityType.ClrType.GetAnyProperty(propertyBase.Name)))
                                : property != null
                                  && property.IsShadowProperty
                                    ? Expression.Call(
                                        entryParameter,
                                        InternalEntityEntry.ReadShadowValueMethod,
                                        Expression.Constant(property.GetShadowIndex()))
                                    : (Expression)Expression.Convert(
                                        Expression.Property(
                                            entityVariable,
                                            propertyBase.DeclaringEntityType.ClrType.GetAnyProperty(propertyBase.Name)),
                                        typeof(object))));
                }
            }

            blockExpressions.Add(valuesVariable);

            return Expression.Lambda<Func<InternalEntityEntry, object[]>>(
                Expression.Block(variables, blockExpressions),
                entryParameter)
                .Compile();
        }

        private static readonly MethodInfo _snapshotCollectionMethod
            = typeof(RelationshipSnapshotFactoryFactory).GetTypeInfo().GetDeclaredMethod(nameof(SnapshotCollection));

        [UsedImplicitly]
        private static HashSet<object> SnapshotCollection(IEnumerable collection)
        {
            if (collection == null)
            {
                return null;
            }

            var snapshot = new HashSet<object>(ReferenceEqualityComparer.Instance);
            foreach (var product in collection)
            {
                snapshot.Add(product);
            }

            return snapshot;
        }
    }
}
