// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.SqlExpressions
{
    public class SqlUnaryExpression : SqlExpression
    {
        private static readonly ISet<ExpressionType> _allowedOperators = new HashSet<ExpressionType>
        {
            ExpressionType.Equal,
            ExpressionType.NotEqual,
            ExpressionType.Convert,
            ExpressionType.Not,
            ExpressionType.Negate,
        };
        private static ExpressionType VerifyOperator(ExpressionType operatorType)
            => _allowedOperators.Contains(operatorType)
                ? operatorType
                : throw new InvalidOperationException("Unsupported Unary operator type specified.");

        public SqlUnaryExpression(
            ExpressionType operatorType,
            SqlExpression operand,
            Type type,
            RelationalTypeMapping typeMapping)
            : base(type, typeMapping)
        {
            Check.NotNull(operand, nameof(operand));
            OperatorType = VerifyOperator(operatorType);
            Operand = operand;
        }

        public virtual ExpressionType OperatorType { get; }
        public virtual SqlExpression Operand { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
            => Update((SqlExpression)visitor.Visit(Operand));

        public virtual SqlUnaryExpression Update(SqlExpression operand)
            => operand != Operand
                ? new SqlUnaryExpression(OperatorType, operand, Type, TypeMapping)
                : this;

        public override void Print(ExpressionPrinter expressionPrinter)
        {
            if (OperatorType == ExpressionType.Convert)
            {
                expressionPrinter.Append("CAST(");
                expressionPrinter.Visit(Operand);
                expressionPrinter.Append(")");
                expressionPrinter.Append(" AS ");
                expressionPrinter.Append(TypeMapping.StoreType);
                expressionPrinter.Append(")");
            }
            else
            {
                expressionPrinter.Append(OperatorType);
                expressionPrinter.Append("(");
                expressionPrinter.Visit(Operand);
                expressionPrinter.Append(")");
            }
        }
        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlUnaryExpression sqlUnaryExpression
                    && Equals(sqlUnaryExpression));

        private bool Equals(SqlUnaryExpression sqlUnaryExpression)
            => base.Equals(sqlUnaryExpression)
            && OperatorType == sqlUnaryExpression.OperatorType
            && Operand.Equals(sqlUnaryExpression.Operand);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), OperatorType, Operand);
    }
}
