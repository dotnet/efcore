// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class EntityProjectionExpression : Expression, IPrintableExpression, IAccessExpression
    {
        private readonly IDictionary<IProperty, IAccessExpression> _propertyExpressionsMap
            = new Dictionary<IProperty, IAccessExpression>();

        private readonly IDictionary<INavigation, IAccessExpression> _navigationExpressionsMap
            = new Dictionary<INavigation, IAccessExpression>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public EntityProjectionExpression([NotNull] IEntityType entityType, [NotNull] Expression accessExpression)
        {
            EntityType = entityType;
            AccessExpression = accessExpression;
            Name = (accessExpression as IAccessExpression)?.Name;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public sealed override ExpressionType NodeType
            => ExpressionType.Extension;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override Type Type
            => EntityType.ClrType;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression AccessExpression { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IEntityType EntityType { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string Name { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            return Update(visitor.Visit(AccessExpression));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression Update([NotNull] Expression accessExpression)
            => accessExpression != AccessExpression
                ? new EntityProjectionExpression(EntityType, accessExpression)
                : this;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression BindProperty([NotNull] IProperty property, bool clientEval)
        {
            if (!EntityType.IsAssignableFrom(property.DeclaringEntityType)
                && !property.DeclaringEntityType.IsAssignableFrom(EntityType))
            {
                throw new InvalidOperationException(
                    CosmosStrings.UnableToBindMemberToEntityProjection("property", property.Name, EntityType.DisplayName()));
            }

            if (!_propertyExpressionsMap.TryGetValue(property, out var expression))
            {
                expression = new KeyAccessExpression(property, AccessExpression);
                _propertyExpressionsMap[property] = expression;
            }

            if (!clientEval
                // TODO: Remove once __jObject is translated to the access root in a better fashion and
                // would not otherwise be found to be non-translatable. See issues #17670 and #14121.
                && property.Name != StoreKeyConvention.JObjectPropertyName
                && expression.Name.Length == 0)
            {
                // Non-persisted property can't be translated
                return null;
            }

            return (Expression)expression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression BindNavigation([NotNull] INavigation navigation, bool clientEval)
        {
            if (!EntityType.IsAssignableFrom(navigation.DeclaringEntityType)
                && !navigation.DeclaringEntityType.IsAssignableFrom(EntityType))
            {
                throw new InvalidOperationException(
                    CosmosStrings.UnableToBindMemberToEntityProjection("navigation", navigation.Name, EntityType.DisplayName()));
            }

            if (!_navigationExpressionsMap.TryGetValue(navigation, out var expression))
            {
                expression = navigation.IsCollection
                    ? new ObjectArrayProjectionExpression(navigation, AccessExpression)
                    : (IAccessExpression)new EntityProjectionExpression(
                        navigation.TargetEntityType,
                        new ObjectAccessExpression(navigation, AccessExpression));

                _navigationExpressionsMap[navigation] = expression;
            }

            if (!clientEval
                && expression.Name.Length == 0)
            {
                // Non-persisted navigation can't be translated
                return null;
            }

            return (Expression)expression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression BindMember(
            [NotNull] string name,
            [NotNull] Type entityType,
            bool clientEval,
            [NotNull] out IPropertyBase propertyBase)
            => BindMember(MemberIdentity.Create(name), entityType, clientEval, out propertyBase);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual Expression BindMember(
            [NotNull] MemberInfo memberInfo,
            [NotNull] Type entityType,
            bool clientEval,
            [NotNull] out IPropertyBase propertyBase)
            => BindMember(MemberIdentity.Create(memberInfo), entityType, clientEval, out propertyBase);

        private Expression BindMember(MemberIdentity member, Type entityClrType, bool clientEval, out IPropertyBase propertyBase)
        {
            var entityType = EntityType;
            if (entityClrType != null
                && !entityClrType.IsAssignableFrom(entityType.ClrType))
            {
                entityType = entityType.GetDerivedTypes().First(e => entityClrType.IsAssignableFrom(e.ClrType));
            }

            var property = member.MemberInfo == null
                ? entityType.FindProperty(member.Name)
                : entityType.FindProperty(member.MemberInfo);
            if (property != null)
            {
                propertyBase = property;
                return BindProperty(property, clientEval);
            }

            var navigation = member.MemberInfo == null
                ? entityType.FindNavigation(member.Name)
                : entityType.FindNavigation(member.MemberInfo);
            if (navigation != null)
            {
                propertyBase = navigation;
                return BindNavigation(navigation, clientEval);
            }

            // Entity member not found
            propertyBase = null;
            return null;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual EntityProjectionExpression UpdateEntityType([NotNull] IEntityType derivedType)
        {
            Check.NotNull(derivedType, nameof(derivedType));

            if (!derivedType.GetAllBaseTypes().Contains(EntityType))
            {
                throw new InvalidOperationException(
                    CosmosStrings.InvalidDerivedTypeInEntityProjection(
                        derivedType.DisplayName(), EntityType.DisplayName()));
            }

            return new EntityProjectionExpression(derivedType, AccessExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        void IPrintableExpression.Print(ExpressionPrinter expressionPrinter)
        {
            Check.NotNull(expressionPrinter, nameof(expressionPrinter));

            expressionPrinter.Visit(AccessExpression);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override bool Equals(object obj)
            => obj != null
                && (ReferenceEquals(this, obj)
                    || obj is EntityProjectionExpression entityProjectionExpression
                    && Equals(entityProjectionExpression));

        private bool Equals(EntityProjectionExpression entityProjectionExpression)
            => Equals(EntityType, entityProjectionExpression.EntityType)
                && AccessExpression.Equals(entityProjectionExpression.AccessExpression);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public override int GetHashCode()
            => HashCode.Combine(EntityType, AccessExpression);
    }
}
