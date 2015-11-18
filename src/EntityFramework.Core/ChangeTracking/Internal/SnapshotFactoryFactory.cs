// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;

namespace Microsoft.Data.Entity.ChangeTracking.Internal
{
    public abstract class SnapshotFactoryFactory
    {
        public virtual Func<ISnapshot> CreateEmpty([NotNull] IEntityType entityType)
        {
            if (GetPropertyCount(entityType) == 0)
            {
                return () => Snapshot.Empty;
            }

            return Expression.Lambda<Func<ISnapshot>>(
                CreateConstructorExpression(entityType, null))
                .Compile();
        }

        protected virtual Expression CreateConstructorExpression(
            [NotNull] IEntityType entityType,
            [CanBeNull] ParameterExpression parameter)
        {
            var count = GetPropertyCount(entityType);

            var types = new Type[count];
            var propertyBases = new IPropertyBase[count];

            foreach (var propertyBase in entityType.GetPropertiesAndNavigations())
            {
                var index = GetPropertyIndex(propertyBase);

                if (index >= 0)
                {
                    types[index] = (propertyBase as IProperty)?.ClrType ?? typeof(object);
                    propertyBases[index] = propertyBase;
                }
            }

            Expression constructorExpression;
            if (count > Snapshot.MaxGenericTypes)
            {
                var snapshotExpressions = new List<Expression>();

                for (var i = 0; i < count; i += Snapshot.MaxGenericTypes)
                {
                    snapshotExpressions.Add(
                        CreateSnapshotExpression(
                            entityType.ClrType,
                            parameter,
                            types.Skip(i).Take(Snapshot.MaxGenericTypes).ToArray(),
                            propertyBases.Skip(i).Take(Snapshot.MaxGenericTypes).ToList()));
                }

                constructorExpression =
                    Expression.Convert(
                        Expression.New(
                            MultiSnapshot.Constructor,
                            Expression.NewArrayInit(typeof(ISnapshot), snapshotExpressions)),
                        typeof(ISnapshot));
            }
            else
            {
                constructorExpression = CreateSnapshotExpression(entityType.ClrType, parameter, types, propertyBases);
            }
            return constructorExpression;
        }

        private Expression CreateSnapshotExpression(
            Type entityType,
            ParameterExpression parameter,
            Type[] types,
            IList<IPropertyBase> propertyBases)
        {
            var count = types.Length;

            var arguments = new Expression[count];

            var entityVariable = entityType == null
                ? null
                : Expression.Variable(entityType, "entity");

            for (var i = 0; i < count; i++)
            {
                var propertyBase = propertyBases[i];

                var navigation = propertyBase as INavigation;
                var property = propertyBase as IProperty;

                arguments[i] =
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
                            ? CreateReadShadowValueExpression(parameter, property)
                            : Expression.Property(
                                entityVariable,
                                propertyBase.DeclaringEntityType.ClrType.GetAnyProperty(propertyBase.Name));
            }

            var constructorExpression = Expression.Convert(
                Expression.New(
                    Snapshot.CreateSnapshotType(types).GetDeclaredConstructor(types),
                    arguments),
                typeof(ISnapshot));

            return UseEntityVariable
                   && entityVariable != null
                ? (Expression)Expression.Block(
                    new List<ParameterExpression> { entityVariable },
                    new List<Expression>
                    {
                        Expression.Assign(
                            entityVariable,
                            Expression.Convert(
                                Expression.Property(parameter, "Entity"),
                                entityType)),
                        constructorExpression
                    })
                : constructorExpression;
        }

        protected virtual Expression CreateReadShadowValueExpression(
            [CanBeNull] ParameterExpression parameter, [NotNull] IProperty property)
            => Expression.Convert(
                Expression.Call(
                    parameter,
                    InternalEntityEntry.ReadShadowValueMethod,
                    Expression.Constant(property.GetShadowIndex())),
                property.ClrType);

        protected abstract int GetPropertyIndex([NotNull] IPropertyBase propertyBase);

        protected abstract int GetPropertyCount([NotNull] IEntityType entityType);

        protected virtual bool UseEntityVariable => true;

        private static readonly MethodInfo _snapshotCollectionMethod
            = typeof(SnapshotFactoryFactory).GetTypeInfo().GetDeclaredMethod(nameof(SnapshotCollection));

        [UsedImplicitly]
        private static HashSet<object> SnapshotCollection(IEnumerable<object> collection)
            => collection == null
                ? null
                : new HashSet<object>(collection, ReferenceEqualityComparer.Instance);
    }
}
