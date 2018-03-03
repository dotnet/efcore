// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class ColumnExpression : QueryExpressionBase
    {
        private readonly IProperty _property;
        private readonly CollectionExpression _collectionExpression;

        public ColumnExpression(
            string name,
            IProperty property,
            CollectionExpression collectionOriginExpression)
        {
            Name = name;
            _property = property;
            _collectionExpression = collectionOriginExpression;
        }

        public CollectionExpression Collection => _collectionExpression;

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
        public IProperty Property => _property;
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword

        public string Name { get; }

        public override Type Type => Property.ClrType;

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            return this;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType()
                   && Equals((ColumnExpression)obj);
        }

        private bool Equals([NotNull] ColumnExpression other)
            // Compare on names only because multiple properties can map to same column in inheritance scenario
            => string.Equals(Name, other.Name)
               && Type == other.Type
               && Equals(Collection, other.Collection);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Type.GetHashCode();
                hashCode = (hashCode * 397) ^ Collection.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();

                return hashCode;
            }
        }

        public override string ToString() => $"{Collection.Alias}.{Name}";
    }
}
