// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Query.Sql;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Expressions
{
    public class ColumnExpression : ExtensionExpression
    {
        private readonly IProperty _property;
        private readonly TableExpressionBase _tableExpression;

        public ColumnExpression(
            [NotNull] string name,
            [NotNull] IProperty property,
            [NotNull] TableExpressionBase tableExpression)
            : base(Check.NotNull(property, "property").PropertyType)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(tableExpression, nameof(tableExpression));

            Name = name;
            _property = property;
            _tableExpression = tableExpression;
        }

        public virtual TableExpressionBase Table
        {
            get { return _tableExpression; }
        }

        public virtual string TableAlias
        {
            get { return _tableExpression.Alias; }
        }

#pragma warning disable 108
        public virtual IProperty Property
#pragma warning restore 108
        {
            get { return _property; }
        }

        public virtual string Name { get; }

        public virtual string Alias { get; [param: CanBeNull] set; }

        public override Expression Accept([NotNull] ExpressionTreeVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitColumnExpression(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionTreeVisitor visitor)
        {
            return this;
        }

        protected bool Equals(ColumnExpression other)
        {
            return _property.Equals(other._property)
                   && _tableExpression.Equals(other._tableExpression);
        }

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

        public override string ToString()
        {
            // TODO: Get provider-specific name
            // Issue #871 
            var s = _tableExpression.Alias + "." + Name;

            if (Alias != null)
            {
                s += " " + Alias;
            }

            return s;
        }
    }
}
