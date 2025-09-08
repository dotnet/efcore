// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.XuGu.Query.ExpressionVisitors.Internal;

namespace Microsoft.EntityFrameworkCore.XuGu.Query.Expressions.Internal
{
    public enum XGBinaryExpressionOperatorType
    {
        /// <summary>
        /// TODO
        /// </summary>
        IntegerDivision,

        /// <summary>
        /// Use to force an equals expression, that will not be optimized by EF Core.
        /// Can be used, to force a `value = TRUE` expression.
        /// </summary>
        NonOptimizedEqual,
    }

    public class XGBinaryExpression : SqlExpression
    {
        private static ConstructorInfo _quotingConstructor;

        public XGBinaryExpression(
            XGBinaryExpressionOperatorType operatorType,
            SqlExpression left,
            SqlExpression right,
            Type type,
            RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            OperatorType = operatorType;

            Left = left;
            Right = right;
        }

        public virtual XGBinaryExpressionOperatorType OperatorType { get; }
        public virtual SqlExpression Left { get; }
        public virtual SqlExpression Right { get; }

        protected override Expression Accept(ExpressionVisitor visitor)
            => visitor is XGQuerySqlGenerator xgQuerySqlGenerator // TODO: Move to VisitExtensions
                ? xgQuerySqlGenerator.VisitXGBinaryExpression(this)
                : base.Accept(visitor);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var left = (SqlExpression)visitor.Visit(Left);
            var right = (SqlExpression)visitor.Visit(Right);

            return Update(left, right);
        }

        public override Expression Quote()
            => New(
                _quotingConstructor ??= typeof(XGBinaryExpression).GetConstructor(
                    [typeof(XGBinaryExpressionOperatorType), typeof(SqlExpression), typeof(SqlExpression), typeof(Type), typeof(RelationalTypeMapping)])!,
                Constant(OperatorType),
                Left.Quote(),
                Right.Quote(),
                Constant(Type),
                RelationalExpressionQuotingUtilities.QuoteTypeMapping(TypeMapping));

        public virtual XGBinaryExpression Update(SqlExpression left, SqlExpression right)
            => left != Left || right != Right
                ? new XGBinaryExpression(OperatorType, left, right, Type, TypeMapping)
                : this;

        protected override void Print(ExpressionPrinter expressionPrinter)
        {
            var requiresBrackets = RequiresBrackets(Left);

            if (requiresBrackets)
            {
                expressionPrinter.Append("(");
            }

            expressionPrinter.Visit(Left);

            if (requiresBrackets)
            {
                expressionPrinter.Append(")");
            }

            switch (OperatorType)
            {
                case XGBinaryExpressionOperatorType.IntegerDivision:
                    expressionPrinter.Append(" DIV ");
                    break;
                case XGBinaryExpressionOperatorType.NonOptimizedEqual:
                    expressionPrinter.Append(" = ");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            requiresBrackets = RequiresBrackets(Right);

            if (requiresBrackets)
            {
                expressionPrinter.Append("(");
            }

            expressionPrinter.Visit(Right);

            if (requiresBrackets)
            {
                expressionPrinter.Append(")");
            }
        }

        private bool RequiresBrackets(SqlExpression expression)
        {
            return expression is SqlBinaryExpression sqlBinary
                && sqlBinary.OperatorType != ExpressionType.Coalesce
                || expression is LikeExpression;
        }

        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is XGBinaryExpression sqlBinaryExpression
                    && Equals(sqlBinaryExpression));

        private bool Equals(XGBinaryExpression sqlBinaryExpression)
            => base.Equals(sqlBinaryExpression)
            && OperatorType == sqlBinaryExpression.OperatorType
            && Left.Equals(sqlBinaryExpression.Left)
            && Right.Equals(sqlBinaryExpression.Right);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), OperatorType, Left, Right);
    }
}
