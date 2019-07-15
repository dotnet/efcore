// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class ObjectArrayProjectionExpression : Expression, IPrintable, IAccessExpression
    {
        public ObjectArrayProjectionExpression(
            INavigation navigation, Expression accessExpression, EntityProjectionExpression innerProjection = null)
        {
            var targetType = navigation.GetTargetType();
            Type = typeof(IEnumerable<>).MakeGenericType(targetType.ClrType);

            Name = targetType.GetCosmosContainingPropertyName();
            if (Name == null)
            {
                throw new InvalidOperationException(
                    $"Navigation '{navigation.DeclaringEntityType.DisplayName()}.{navigation.Name}' doesn't point to an embedded entity.");
            }

            Navigation = navigation;
            AccessExpression = accessExpression;
            InnerProjection = innerProjection ?? new EntityProjectionExpression(
                                  targetType,
                                  new RootReferenceExpression(targetType, ""));
        }

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type { get; }

        public virtual string Name { get; }
        public virtual INavigation Navigation { get; }
        public virtual Expression AccessExpression { get; }
        public virtual EntityProjectionExpression InnerProjection { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var accessExpression = visitor.Visit(AccessExpression);
            var innerProjection = visitor.Visit(InnerProjection);

            return Update(accessExpression, (EntityProjectionExpression)innerProjection);
        }

        public ObjectArrayProjectionExpression Update(Expression accessExpression, EntityProjectionExpression innerProjection)
            => accessExpression != AccessExpression || innerProjection != InnerProjection
                ? new ObjectArrayProjectionExpression(Navigation, accessExpression, innerProjection)
                : this;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Print(ExpressionPrinter expressionPrinter)
            => expressionPrinter.StringBuilder.Append(ToString());

        public override string ToString() => $"{AccessExpression}[\"{Name}\"]";

        public override bool Equals(object obj)
            => obj != null
               && (ReferenceEquals(this, obj)
                   || obj is ObjectArrayProjectionExpression arrayProjectionExpression
                   && Equals(arrayProjectionExpression));

        private bool Equals(ObjectArrayProjectionExpression objectArrayProjectionExpression)
            => AccessExpression.Equals(objectArrayProjectionExpression.AccessExpression)
               && InnerProjection.Equals(objectArrayProjectionExpression.InnerProjection);

        public override int GetHashCode() => HashCode.Combine(AccessExpression, InnerProjection);
    }
}
