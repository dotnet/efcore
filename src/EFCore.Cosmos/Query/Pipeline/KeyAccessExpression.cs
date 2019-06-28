// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class KeyAccessExpression : SqlExpression
    {
        private readonly IProperty _property;
        private readonly Expression _outerExpression;

        public KeyAccessExpression(IProperty property, Expression outerExpression)
            : base(property.ClrType, property.GetTypeMapping())
        {
            Name = property.GetCosmosPropertyName();
            _property = property;
            _outerExpression = outerExpression;
        }

        public string Name { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var outerExpression = visitor.Visit(_outerExpression);

            return Update(outerExpression);
        }

        public KeyAccessExpression Update(Expression outerExpression)
            => outerExpression != _outerExpression
                ? new KeyAccessExpression(_property, outerExpression)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
            => expressionPrinter.StringBuilder.Append(ToString());

        public override string ToString() => $"{_outerExpression}[\"{Name}\"]";

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is KeyAccessExpression keyAccessExpression
                    && Equals(keyAccessExpression));

        private bool Equals(KeyAccessExpression keyAccessExpression)
            => base.Equals(keyAccessExpression)
            && string.Equals(Name, keyAccessExpression.Name)
            && _outerExpression.Equals(keyAccessExpression._outerExpression);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), Name, _outerExpression);
    }
}
