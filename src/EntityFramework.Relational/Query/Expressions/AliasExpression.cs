// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Query.Sql;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Query.Expressions
{
    public class AliasExpression : Expression
    {
        private readonly Expression _expression;

        private string _alias;

        private Expression _sourceExpression;

        public AliasExpression([NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            _expression = expression;
        }

        // TODO: Revisit the design here, "alias" should really be required.
        public AliasExpression([CanBeNull] string alias, [NotNull] Expression expression)
        {
            Check.NotNull(expression, nameof(expression));

            _alias = alias;
            _expression = expression;
        }

        public virtual string Alias
        {
            get { return _alias; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _alias = value;
            }
        }

        public virtual Expression Expression => _expression;

        // TODO: Revisit why we need this. Try and remove
        public virtual bool Projected { get; set; } = false;

        public override ExpressionType NodeType => ExpressionType.Extension;
        public override Type Type => _expression.Type;

        public virtual Expression SourceExpression
        {
            get { return _sourceExpression; }
            [param: NotNull]
            set
            {
                Check.NotNull(value, nameof(value));

                _sourceExpression = value;
            }
        }

        public virtual MemberInfo SourceMember { get; [param: CanBeNull] set; }

        protected override Expression Accept(ExpressionVisitor visitor)
        {
            Check.NotNull(visitor, nameof(visitor));

            var specificVisitor = visitor as ISqlExpressionVisitor;

            return specificVisitor != null
                ? specificVisitor.VisitAlias(this)
                : base.Accept(visitor);
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var newInnerExpression = visitor.Visit(_expression);

            return newInnerExpression != _expression
                ? new AliasExpression(Alias, newInnerExpression)
                : this;
        }

        public override string ToString()
            => this.TryGetColumnExpression()?.ToString() ?? Expression.NodeType + " " + Alias;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return (obj.GetType() == GetType())
                   && Equals((AliasExpression)obj);
        }

        private bool Equals(AliasExpression other)
            => Equals(_expression, other._expression)
               && string.Equals(_alias, other._alias);

        public override int GetHashCode()
        {
            unchecked
            {
                // ReSharper disable once NonReadonlyMemberInGetHashCode
                return (_expression.GetHashCode() * 397) ^ (_alias?.GetHashCode() ?? 0);
            }
        }
    }
}
