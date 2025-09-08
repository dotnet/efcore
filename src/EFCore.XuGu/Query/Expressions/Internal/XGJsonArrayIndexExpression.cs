// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal
{
    /// <summary>
    /// Represents a MySQL JSON array index (i.e. x[y]).
    /// </summary>
    public class XGJsonArrayIndexExpression : SqlExpression, IEquatable<XGJsonArrayIndexExpression>
    {
        private static ConstructorInfo _quotingConstructor;

        [NotNull]
        public virtual SqlExpression Expression { get; }

        public XGJsonArrayIndexExpression(
            [NotNull] SqlExpression expression,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Expression = expression;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SqlExpression)visitor.Visit(Expression));

        /// <inheritdoc />
        public override Expression Quote()
            => New(
                _quotingConstructor ??= typeof(XGInlinedParameterExpression).GetConstructor(
                    [typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping)])!,
                Expression.Quote(),
                Constant(Type),
                RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

        public virtual XGJsonArrayIndexExpression Update(
            [NotNull] SqlExpression expression)
            => expression == Expression
                ? this
                : new XGJsonArrayIndexExpression(expression, Type, TypeMapping);

        public override bool Equals(object obj)
            => Equals(obj as XGJsonArrayIndexExpression);

        public virtual bool Equals(XGJsonArrayIndexExpression other)
            => ReferenceEquals(this, other) ||
               other != null &&
               base.Equals(other) &&
               Equals(Expression, other.Expression);

        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Expression);

        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("[");
            expressionPrinter.Visit(Expression);
            expressionPrinter.Append("]");
        }

        public override string ToString()
            => $"[{Expression}]";

        public virtual SqlExpression ApplyTypeMapping(RelationalTypeMapping typeMapping)
            => new XGJsonArrayIndexExpression(Expression, Type, typeMapping);
    }
}
