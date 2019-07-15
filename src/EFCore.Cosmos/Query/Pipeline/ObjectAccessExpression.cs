// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class ObjectAccessExpression : Expression, IPrintable, IAccessExpression
    {
        public ObjectAccessExpression(INavigation navigation, Expression accessExpression)
        {
            Name = navigation.GetTargetType().GetCosmosContainingPropertyName();
            if (Name == null)
            {
                throw new InvalidOperationException(
                    $"Navigation '{navigation.DeclaringEntityType.DisplayName()}.{navigation.Name}' doesn't point to a nested entity.");
            }

            Navigation = navigation;
            AccessExpression = accessExpression;
        }

        public override Type Type => Navigation.ClrType;
        public virtual string Name { get; }
        public virtual INavigation Navigation { get; }
        public virtual Expression AccessExpression { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var outerExpression = visitor.Visit(AccessExpression);

            return Update(outerExpression);
        }

        public ObjectAccessExpression Update(Expression outerExpression)
            => outerExpression != AccessExpression
                ? new ObjectAccessExpression(Navigation, outerExpression)
                : this;

        public virtual void Print(ExpressionPrinter expressionPrinter) => expressionPrinter.StringBuilder.Append(ToString());

        public override string ToString() => $"{AccessExpression}[\"{Name}\"]";

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ObjectAccessExpression objectAccessExpression
                    && Equals(objectAccessExpression));

        private bool Equals(ObjectAccessExpression objectAccessExpression)
            => Navigation == objectAccessExpression.Navigation
               && AccessExpression.Equals(objectAccessExpression.AccessExpression);

        public override int GetHashCode() => HashCode.Combine(Navigation, AccessExpression);
    }
}
