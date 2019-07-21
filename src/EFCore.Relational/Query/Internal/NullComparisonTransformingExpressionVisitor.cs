// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    public class NullComparisonTransformingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            if (extensionExpression is SqlBinaryExpression sqlBinary
                && (sqlBinary.OperatorType == ExpressionType.Equal
                    || sqlBinary.OperatorType == ExpressionType.NotEqual))
            {
                var isLeftNull = sqlBinary.Left is SqlConstantExpression leftConstant && leftConstant.Value == null;
                var isRightNull = sqlBinary.Right is SqlConstantExpression rightConstant && rightConstant.Value == null;

                if (isLeftNull || isRightNull)
                {
                    var nonNull = isLeftNull ? sqlBinary.Right : sqlBinary.Left;

                    return new SqlUnaryExpression(
                        sqlBinary.OperatorType,
                        nonNull,
                        sqlBinary.Type,
                        sqlBinary.TypeMapping);
                }
            }

            return base.VisitExtension(extensionExpression);
        }
    }
}
