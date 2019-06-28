// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class ObjectAccessExpression : SqlExpression
    {
        private readonly Expression _outerExpression;

        public ObjectAccessExpression(INavigation navigation, Expression outerExpression)
            : base(navigation.ClrType, null)
        {
            Name = navigation.GetTargetType().GetCosmosContainingPropertyName();
            if (Name == null)
            {
                throw new InvalidOperationException(
                    $"Navigation '{navigation.DeclaringEntityType.DisplayName()}.{navigation.Name}' doesn't point to a nested entity.");
            }

            Navigation = navigation;
            _outerExpression = outerExpression;
        }

        public string Name { get; }

        public INavigation Navigation { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var outerExpression = visitor.Visit(_outerExpression);

            return Update(outerExpression);
        }

        public ObjectAccessExpression Update(Expression outerExpression)
            => outerExpression != _outerExpression
                ? new ObjectAccessExpression(Navigation, outerExpression)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter) => expressionPrinter.StringBuilder.Append(ToString());

        public override string ToString() => $"{_outerExpression}[\"{Name}\"]";

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is ObjectAccessExpression objectAccessExpression
                    && Equals(objectAccessExpression));

        private bool Equals(ObjectAccessExpression objectAccessExpression)
            => base.Equals(objectAccessExpression)
            && string.Equals(Name, objectAccessExpression.Name)
            && _outerExpression.Equals(objectAccessExpression._outerExpression);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name, _outerExpression);
    }
}
