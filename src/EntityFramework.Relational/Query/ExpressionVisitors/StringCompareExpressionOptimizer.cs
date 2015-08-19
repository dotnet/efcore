// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq.Parsing;
using Microsoft.Data.Entity.Query.Expressions;
using Microsoft.Data.Entity.Query.Methods;

namespace Microsoft.Data.Entity.Query.ExpressionVisitors
{
    public class StringCompareExpressionOptimizer : RelinqExpressionVisitor
    {
        private static readonly Dictionary<ExpressionType, ExpressionType> _operatorMap = new Dictionary<ExpressionType, ExpressionType>
        {
            {  ExpressionType.LessThan, ExpressionType.GreaterThan },
            {  ExpressionType.LessThanOrEqual, ExpressionType.GreaterThanOrEqual },
            {  ExpressionType.GreaterThan, ExpressionType.LessThan },
            {  ExpressionType.GreaterThanOrEqual, ExpressionType.LessThanOrEqual },
            {  ExpressionType.Equal, ExpressionType.Equal },
            {  ExpressionType.NotEqual, ExpressionType.NotEqual },
        };

        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (!_operatorMap.ContainsKey(node.NodeType))
            {
                return base.VisitBinary(node);
            }

            var leftSqlFunction = node.Left as SqlFunctionExpression;
            var rightConstant = node.Right as ConstantExpression;
            if (leftSqlFunction != null 
                && leftSqlFunction.FunctionName == StringCompareTranslator.StringCompareMethodName
                && leftSqlFunction.Type == typeof(int)
                && rightConstant != null 
                && rightConstant.Type == typeof(int) 
                && (int)rightConstant.Value == 0)
            {
                var arguments = leftSqlFunction.Arguments.ToList();
                return new SqlFunctionExpression(
                    StringCompareTranslator.StringCompareMethodName,
                        new[] { arguments[0], arguments[1], Expression.Constant(node.NodeType) },
                        typeof(bool));
            }

            var leftConstant = node.Left as ConstantExpression;
            var rightSqlFunction = node.Right as SqlFunctionExpression;
            if (rightSqlFunction != null
                && rightSqlFunction.FunctionName == StringCompareTranslator.StringCompareMethodName
                && rightSqlFunction.Type == typeof(int)
                && leftConstant != null
                && leftConstant.Type == typeof(int)
                && (int)leftConstant.Value == 0)
            {
                var arguments = rightSqlFunction.Arguments.ToList();
                return new SqlFunctionExpression(
                    StringCompareTranslator.StringCompareMethodName,
                        new[] { arguments[0], arguments[1], Expression.Constant(_operatorMap[node.NodeType]) },
                        typeof(bool));
            }

            return base.VisitBinary(node);
        }
    }
}
