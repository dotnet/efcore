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
    /// Allows to reference an alias from within the same SELECT statement, e.g. in a HAVING clause.
    /// </summary>
    public class XGColumnAliasReferenceExpression : SqlExpression, IEquatable<XGColumnAliasReferenceExpression>
    {
        private static ConstructorInfo _quotingConstructor;

        [NotNull]
        public virtual string Alias { get; }

        [NotNull]
        public virtual SqlExpression Expression { get; }

        public XGColumnAliasReferenceExpression(
            [NotNull] string alias,
            [NotNull] SqlExpression expression,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Alias = alias;
            Expression = expression;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => this;

        public override Expression Quote()
            => New(
                _quotingConstructor ??= typeof(XGColumnAliasReferenceExpression).GetConstructor(
                    [typeof(string), typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping)])!,
                Constant(Alias),
                Expression,
                Constant(Type),
                RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

        public virtual XGColumnAliasReferenceExpression Update(
            [NotNull] string alias,
            [NotNull] SqlExpression expression)
            => alias == Alias &&
               expression.Equals(Expression)
                ? this
                : new XGColumnAliasReferenceExpression(alias, expression, Type, TypeMapping);

        public override bool Equals(object obj)
            => Equals(obj as XGColumnAliasReferenceExpression);

        public virtual bool Equals(XGColumnAliasReferenceExpression other)
            => ReferenceEquals(this, other) ||
               other != null &&
               base.Equals(other) &&
               Equals(Expression, other.Expression);

        public override int GetHashCode()
            => HashCode.Combine(base.GetHashCode(), Expression);

        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            expressionPrinter.Append("`");
            expressionPrinter.Append(Alias);
            expressionPrinter.Append("`");
        }

        public override string ToString()
            => $"`{Alias}`";

        public virtual SqlExpression ApplyTypeMapping(RelationalTypeMapping typeMapping)
            => new XGColumnAliasReferenceExpression(Alias, Expression, Type, typeMapping);
    }
}
