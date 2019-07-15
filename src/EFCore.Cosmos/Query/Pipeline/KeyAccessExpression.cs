// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class KeyAccessExpression : SqlExpression, IAccessExpression
    {
        public KeyAccessExpression(IProperty property, Expression accessExpression)
            : base(property.ClrType, property.GetTypeMapping())
        {
            Name = property.GetCosmosPropertyName();
            Property = property;
            AccessExpression = accessExpression;
        }

        public virtual string Name { get; }
        public new virtual IProperty Property { get; }
        public virtual Expression AccessExpression { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var outerExpression = visitor.Visit(AccessExpression);

            return Update(outerExpression);
        }

        public KeyAccessExpression Update(Expression outerExpression)
            => outerExpression != AccessExpression
                ? new KeyAccessExpression(Property, outerExpression)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
            => expressionPrinter.StringBuilder.Append(ToString());

        public override string ToString() => $"{AccessExpression}[\"{Name}\"]";

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is KeyAccessExpression keyAccessExpression
                    && Equals(keyAccessExpression));

        private bool Equals(KeyAccessExpression keyAccessExpression)
            => base.Equals(keyAccessExpression)
            && string.Equals(Name, keyAccessExpression.Name)
            && AccessExpression.Equals(keyAccessExpression.AccessExpression);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name, AccessExpression);
    }
}
