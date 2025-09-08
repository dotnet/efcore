// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal
{
    public class XGComplexFunctionArgumentExpression : SqlExpression
    {
        private static ConstructorInfo _quotingConstructor;

        public XGComplexFunctionArgumentExpression(
            IEnumerable<SqlExpression> argumentParts,
            string delimiter,
            Type type,
            RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Delimiter = delimiter;
            ArgumentParts = argumentParts.ToList().AsReadOnly();
        }

        /// <summary>
        ///     The arguments parts.
        /// </summary>
        public virtual IReadOnlyList<SqlExpression> ArgumentParts { get; }

        public virtual string Delimiter { get; }

        /// <summary>
        ///     Dispatches to the specific visit method for this node type.
        /// </summary>
        protected override Expression Accept(ExpressionVisitor visitor) =>
            visitor is XGQuerySqlGenerator xgQuerySqlGenerator // TODO: Move to VisitExtensions
                ? xgQuerySqlGenerator.VisitXGComplexFunctionArgumentExpression(this)
                : base.Accept(visitor);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var argumentParts = new SqlExpression[ArgumentParts.Count];

            for (var i = 0; i < argumentParts.Length; i++)
            {
                argumentParts[i] = (SqlExpression) visitor.Visit(ArgumentParts[i]);
            }

            return Update(argumentParts, Delimiter);
        }

        /// <inheritdoc />
        public override Expression Quote()
            => New(
                _quotingConstructor ??= typeof(XGColumnAliasReferenceExpression).GetConstructor(
                    [typeof(IReadOnlyList<SqlExpression>), typeof(string), typeof(Type), typeof(RelationalTypeMapping)])!,
                NewArrayInit(typeof(SqlExpression), ArgumentParts.Select(p => p.Quote())),
                Constant(Delimiter),
                Constant(Type),
                RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

        public virtual XGComplexFunctionArgumentExpression Update(IReadOnlyList<SqlExpression> argumentParts, string delimiter)
            => !argumentParts.SequenceEqual(ArgumentParts)
                ? new XGComplexFunctionArgumentExpression(argumentParts, delimiter, Type, TypeMapping)
                : this;

        protected override void Print(ExpressionPrinter expressionPrinter)
            => expressionPrinter.Append(ToString());

        public override bool Equals(object obj)
            => obj != null &&
               (ReferenceEquals(this, obj) ||
                obj is XGComplexFunctionArgumentExpression complexExpression && Equals(complexExpression));

        private bool Equals(XGComplexFunctionArgumentExpression other)
            => base.Equals(other) &&
               Delimiter.Equals(other.Delimiter, StringComparison.OrdinalIgnoreCase) &&
               ArgumentParts.SequenceEqual(other.ArgumentParts);

        /// <summary>
        ///     Returns a hash code for this object.
        /// </summary>
        /// <returns>
        ///     A hash code for this object.
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();

            hashCode.Add(base.GetHashCode());

            foreach (var argumentPart in ArgumentParts)
            {
                hashCode.Add(argumentPart);
            }

            hashCode.Add(Delimiter);
            return hashCode.ToHashCode();
        }

        /// <summary>
        ///     Creates a <see cref="string" /> representation of the Expression.
        /// </summary>
        /// <returns>A <see cref="string" /> representation of the Expression.</returns>
        public override string ToString()
            => string.Join(Delimiter, ArgumentParts);
    }
}
