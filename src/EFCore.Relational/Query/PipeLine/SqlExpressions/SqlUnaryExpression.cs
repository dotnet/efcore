// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions
{
    public class SqlUnaryExpression : SqlExpression
    {
        #region Fields & Constructors
        private static ISet<ExpressionType> _allowedOperators = new HashSet<ExpressionType>
        {
            ExpressionType.Equal,
            ExpressionType.NotEqual,
            ExpressionType.Convert,
            ExpressionType.Not,
            ExpressionType.Negate,
        };

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

        private static ExpressionType VerifyOperator(ExpressionType operatorType)
        {
            return _allowedOperators.Contains(operatorType)
                ? operatorType
                : throw new InvalidOperationException("Unsupported Unary operator type specified.");

        }
        #endregion

        #region Public Properties

        public ExpressionType OperatorType { get; }
        public SqlExpression Operand { get; }
        #endregion

        #region Expression-based methods
        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var operand = (SqlExpression)visitor.Visit(Operand);

            return Update(operand);
        }

        public SqlUnaryExpression Update(SqlExpression operand)
        {
            return operand != Operand
                ? new SqlUnaryExpression(OperatorType, operand, Type, TypeMapping)
                : this;
        }
        #endregion

        #region Equality & HashCode
        public override bool Equals(object obj)
            => obj != null
            && (ReferenceEquals(this, obj)
                || obj is SqlUnaryExpression sqlUnaryExpression
                    && Equals(sqlUnaryExpression));

        private bool Equals(SqlUnaryExpression sqlUnaryExpression)
            => base.Equals(sqlUnaryExpression)
            && OperatorType == sqlUnaryExpression.OperatorType
            && Operand.Equals(sqlUnaryExpression.Operand);

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ OperatorType.GetHashCode();
                hashCode = (hashCode * 397) ^ Operand.GetHashCode();

                return hashCode;
            }
        }
        #endregion
    }
}
