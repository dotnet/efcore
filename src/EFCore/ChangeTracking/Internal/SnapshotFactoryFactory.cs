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
using Microsoft.EntityFrameworkCore.Query;

namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public abstract class SnapshotFactoryFactory
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Func<ISnapshot> CreateEmpty([NotNull] IEntityType entityType)
        {
            return GetPropertyCount(entityType) == 0
                ? (() => Snapshot.Empty)
                : Expression.Lambda<Func<ISnapshot>>(
                        CreateConstructorExpression(entityType, null))
                    .Compile();
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual Expression CreateSnapshotExpression(
            [CanBeNull] Type entityType,
            [NotNull] ParameterExpression parameter,
            [NotNull] Type[] types,
            [NotNull] IList<IPropertyBase> propertyBases)
        {
            var count = types.Length;

            var arguments = new Expression[count];

            var entityVariable = entityType == null
                ? null
                : Expression.Variable(entityType, "entity");

            for (var i = 0; i < count; i++)
            {
                var propertyBase = propertyBases[i];
                if (propertyBase == null)
                {
                    arguments[i] = Expression.Constant(null);
                    types[i] = typeof(object);
                    continue;
                }

                if (propertyBase is IProperty property)
                {
                    var storeGeneratedIndex = property.GetStoreGeneratedIndex();
                    if (storeGeneratedIndex != -1)
                    {
                        arguments[i] = CreateReadValueExpression(parameter, property);
                        continue;
                    }
                }

                if (propertyBase.IsShadowProperty())
                {
                    arguments[i] = CreateSnapshotValueExpression(
                        CreateReadShadowValueExpression(parameter, propertyBase),
                        propertyBase);
                    continue;
                }

                var memberInfo = propertyBase.GetMemberInfo(forMaterialization: false, forSet: false);
                var memberAccess = propertyBase.IsIndexerProperty()
                    ? Expression.MakeIndex(entityVariable, (PropertyInfo)memberInfo, new[] { Expression.Constant(propertyBase.Name) })
                    : (Expression)Expression.MakeMemberAccess(entityVariable, memberInfo);

                if (memberAccess.Type != propertyBase.ClrType)
                {
                    memberAccess = Expression.Convert(memberAccess, propertyBase.ClrType);
                }

                arguments[i] = (propertyBase as INavigation)?.IsCollection() ?? false
                    ? Expression.Call(
                        null,
                        _snapshotCollectionMethod,
                        memberAccess)
                    : CreateSnapshotValueExpression(memberAccess, propertyBase);
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

        private Expression CreateSnapshotValueExpression(Expression expression, IPropertyBase propertyBase)
        {
            if (propertyBase is IProperty property)
            {
                var comparer = GetValueComparer(property);

                if (comparer != null)
                {
                    var snapshotExpression = ReplacingExpressionVisitor.Replace(
                        comparer.SnapshotExpression.Parameters.Single(),
                        expression,
                        comparer.SnapshotExpression.Body);

                    expression = propertyBase.ClrType.IsNullableType()
                        ? Expression.Condition(
                            Expression.Equal(expression, Expression.Constant(null, propertyBase.ClrType)),
                            Expression.Constant(null, propertyBase.ClrType),
                            snapshotExpression)
                        : snapshotExpression;
                }
            }

            return expression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract ValueComparer GetValueComparer([NotNull] IProperty property);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual Expression CreateReadShadowValueExpression(
            [CanBeNull] ParameterExpression parameter, [NotNull] IPropertyBase property)
            => Expression.Call(
                parameter,
                InternalEntityEntry.ReadShadowValueMethod.MakeGenericMethod(property.ClrType),
                Expression.Constant(property.GetShadowIndex()));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual Expression CreateReadValueExpression(
            [CanBeNull] ParameterExpression parameter, [NotNull] IPropertyBase property)
            => Expression.Call(
                parameter,
                InternalEntityEntry.GetCurrentValueMethod.MakeGenericMethod(property.ClrType),
                Expression.Constant(property, typeof(IProperty)));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract int GetPropertyIndex([NotNull] IPropertyBase propertyBase);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract int GetPropertyCount([NotNull] IEntityType entityType);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
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
