// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class ColumnExpression : Expression
    {
        private readonly IProperty _property;
        private readonly TableExpressionBase _tableExpression;

        public ColumnExpression(
            [NotNull] string name,
            [NotNull] IProperty property,
            [NotNull] TableExpressionBase tableExpression)
            : this(name, Check.NotNull(property, nameof(property)).ClrType, tableExpression)
        {
            _property = property;
        }

        public ColumnExpression(
            [NotNull] string name,
            [NotNull] Type type,
            [NotNull] TableExpressionBase tableExpression)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(type, nameof(type));
            Check.NotNull(tableExpression, nameof(tableExpression));

            Name = name;
            Type = type;
            _tableExpression = tableExpression;
        }

        public virtual TableExpressionBase Table => _tableExpression;

        public virtual string TableAlias => _tableExpression.Alias;

#pragma warning disable 108
        public virtual IProperty Property => _property;
#pragma warning restore 108

        public virtual string Name { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitColumn(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        protected virtual bool Equals([NotNull] ColumnExpression other)
            => _property.Equals(other._property)
               && _tableExpression.Equals(other._tableExpression);

        public override bool Equals([CanBeNull] object obj)
        {
            if (ReferenceEquals(null, obj))
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

        public override int GetHashCode()
        {
            unchecked
            {
                return (_property.GetHashCode() * 397)
                       ^ _tableExpression.GetHashCode();
            }
        }

        public override string ToString() => _tableExpression.Alias + "." + Name;
    }
}
