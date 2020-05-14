// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class CaseSimplifyingExpressionVisitor : ExpressionVisitor
    {
        private readonly ISqlExpressionFactory _sqlExpressionFactory;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public CaseSimplifyingExpressionVisitor([NotNull] ISqlExpressionFactory sqlExpressionFactory)
        {
            _sqlExpressionFactory = sqlExpressionFactory;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitExtension(Expression extensionExpression)
        {
            Check.NotNull(extensionExpression, nameof(extensionExpression));

            if (extensionExpression is ShapedQueryExpression shapedQueryExpression)
            {
                return shapedQueryExpression.Update(Visit(shapedQueryExpression.QueryExpression), shapedQueryExpression.ShaperExpression);
            }

            // Only applies to 'CASE WHEN condition...' not 'CASE operand WHEN...'
            if (extensionExpression is CaseExpression caseExpression
                && caseExpression.Operand == null
                && caseExpression.ElseResult is CaseExpression nestedCaseExpression
                && nestedCaseExpression.Operand == null)
            {
                return VisitExtension(_sqlExpressionFactory.Case(
                    caseExpression.WhenClauses.Union(nestedCaseExpression.WhenClauses).ToList(),
                    nestedCaseExpression.ElseResult));
            }

            if (extensionExpression is SqlBinaryExpression sqlBinaryExpression)
            {
                var sqlConstantComponent = sqlBinaryExpression.Left as SqlConstantExpression ?? sqlBinaryExpression.Right as SqlConstantExpression;
                var caseComponent = sqlBinaryExpression.Left as CaseExpression ?? sqlBinaryExpression.Right as CaseExpression;

                // generic CASE statement comparison optimization:
                // (CASE
                //  WHEN condition1 THEN result1
                //  WHEN condition2 THEN result2
                //  WHEN ...
                //  WHEN conditionN THEN resultN) == result1 -> condition1
                if (sqlBinaryExpression.OperatorType == ExpressionType.Equal
                    && sqlConstantComponent != null
                    && sqlConstantComponent.Value != null
                    && caseComponent != null
                    && caseComponent.Operand == null)
                {
                    var matchingCaseBlock = caseComponent.WhenClauses.FirstOrDefault(wc => sqlConstantComponent.Equals(wc.Result));
                    if (matchingCaseBlock != null)
                    {
                        return Visit(matchingCaseBlock.Test);
                    }
                }

                // CompareTo specific optimizations
                if (sqlConstantComponent != null
                    && IsCompareTo(caseComponent)
                    && sqlConstantComponent.Value is int intValue
                    && (intValue > -2 && intValue < 2)
                    && (sqlBinaryExpression.OperatorType == ExpressionType.NotEqual
                        || sqlBinaryExpression.OperatorType == ExpressionType.GreaterThan
                        || sqlBinaryExpression.OperatorType == ExpressionType.GreaterThanOrEqual
                        || sqlBinaryExpression.OperatorType == ExpressionType.LessThan
                        || sqlBinaryExpression.OperatorType == ExpressionType.LessThanOrEqual))
                {
                    return OptimizeCompareTo(
                        sqlBinaryExpression,
                        intValue,
                        caseComponent);
                }
            }

            return base.VisitExtension(extensionExpression);
        }

        private bool IsCompareTo(CaseExpression caseExpression)
        {
            if (caseExpression != null
                && caseExpression.Operand == null
                && caseExpression.ElseResult == null
                && caseExpression.WhenClauses.Count == 3
                && caseExpression.WhenClauses.All(c => c.Test is SqlBinaryExpression
                    && c.Result is SqlConstantExpression constant
                    && constant.Value is int))
            {
                var whenClauses = caseExpression.WhenClauses.Select(c => new
                {
                    test = (SqlBinaryExpression)c.Test,
                    resultValue = (int)((SqlConstantExpression)c.Result).Value
                }).ToList();

                if (whenClauses[0].test.Left.Equals(whenClauses[1].test.Left)
                    && whenClauses[1].test.Left.Equals(whenClauses[2].test.Left)
                    && whenClauses[0].test.Right.Equals(whenClauses[1].test.Right)
                    && whenClauses[1].test.Right.Equals(whenClauses[2].test.Right)
                    && whenClauses[0].test.OperatorType == ExpressionType.Equal
                    && whenClauses[1].test.OperatorType == ExpressionType.GreaterThan
                    && whenClauses[2].test.OperatorType == ExpressionType.LessThan
                    && whenClauses[0].resultValue == 0
                    && whenClauses[1].resultValue == 1
                    && whenClauses[2].resultValue == -1)
                {
                    return true;
                }
            }

            return false;
        }

        private SqlExpression OptimizeCompareTo(
            SqlBinaryExpression sqlBinaryExpression,
            int intValue,
            CaseExpression caseExpression)
        {
            var testLeft = ((SqlBinaryExpression)caseExpression.WhenClauses[0].Test).Left;
            var testRight = ((SqlBinaryExpression)caseExpression.WhenClauses[0].Test).Right;
            var operatorType = sqlBinaryExpression.Right is SqlConstantExpression
                ? sqlBinaryExpression.OperatorType
                : sqlBinaryExpression.OperatorType switch
                {
                    ExpressionType.GreaterThan => ExpressionType.LessThan,
                    ExpressionType.GreaterThanOrEqual => ExpressionType.LessThanOrEqual,
                    ExpressionType.LessThan => ExpressionType.GreaterThan,
                    ExpressionType.LessThanOrEqual => ExpressionType.GreaterThanOrEqual,
                    _ => sqlBinaryExpression.OperatorType
                };

            switch (operatorType)
            {
                // CompareTo(a, b) != 0 -> a != b
                // CompareTo(a, b) != 1 -> a <= b
                // CompareTo(a, b) != -1 -> a >= b
                case ExpressionType.NotEqual:
                    return (SqlExpression)Visit(intValue switch
                    {
                        0 => _sqlExpressionFactory.NotEqual(testLeft, testRight),
                        1 => _sqlExpressionFactory.LessThanOrEqual(testLeft, testRight),
                        _ => _sqlExpressionFactory.GreaterThanOrEqual(testLeft, testRight),
                    });

                // CompareTo(a, b) > 0 -> a > b
                // CompareTo(a, b) > 1 -> false
                // CompareTo(a, b) > -1 -> a >= b
                case ExpressionType.GreaterThan:
                    return (SqlExpression)Visit(intValue switch
                    {
                        0 => _sqlExpressionFactory.GreaterThan(testLeft, testRight),
                        1 => _sqlExpressionFactory.Constant(false, sqlBinaryExpression.TypeMapping),
                        _ => _sqlExpressionFactory.GreaterThanOrEqual(testLeft, testRight),
                    });

                // CompareTo(a, b) >= 0 -> a >= b
                // CompareTo(a, b) >= 1 -> a > b
                // CompareTo(a, b) >= -1 -> true
                case ExpressionType.GreaterThanOrEqual:
                    return (SqlExpression)Visit(intValue switch
                    {
                        0 => _sqlExpressionFactory.GreaterThanOrEqual(testLeft, testRight),
                        1 => _sqlExpressionFactory.GreaterThan(testLeft, testRight),
                        _ => _sqlExpressionFactory.Constant(true, sqlBinaryExpression.TypeMapping),
                    });

                // CompareTo(a, b) < 0 -> a < b
                // CompareTo(a, b) < 1 -> a <= b
                // CompareTo(a, b) < -1 -> false
                case ExpressionType.LessThan:
                    return (SqlExpression)Visit(intValue switch
                    {
                        0 => _sqlExpressionFactory.LessThan(testLeft, testRight),
                        1 => _sqlExpressionFactory.LessThanOrEqual(testLeft, testRight),
                        _ => _sqlExpressionFactory.Constant(false, sqlBinaryExpression.TypeMapping),
                    });

                // operatorType == ExpressionType.LessThanOrEqual
                // CompareTo(a, b) <= 0 -> a <= b
                // CompareTo(a, b) <= 1 -> true
                // CompareTo(a, b) <= -1 -> a < b
                default:
                    return (SqlExpression)Visit(intValue switch
                    {
                        0 => _sqlExpressionFactory.LessThanOrEqual(testLeft, testRight),
                        1 => _sqlExpressionFactory.Constant(true, sqlBinaryExpression.TypeMapping),
                        _ => _sqlExpressionFactory.LessThan(testLeft, testRight),
                    });
            };
        }
    }
}
