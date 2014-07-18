// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.AzureTableStorage.Query.Expressions;
using Microsoft.Data.Entity.AzureTableStorage.Utilities;
using Microsoft.WindowsAzure.Storage.Table;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.AzureTableStorage.Query
{
    public class TableQueryGenerator : ThrowingExpressionTreeVisitor
    {
        private StringBuilder _whereStringBuilder;
        private int? _take;

        public virtual TableQuery GenerateTableQuery([NotNull] SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, "selectExpression");

            _whereStringBuilder = new StringBuilder();
            _take = null;

            selectExpression.Accept(this);

            var tableQuery = new TableQuery();
            tableQuery.Where(_whereStringBuilder.ToString());
            tableQuery.Take(_take);

            return tableQuery;
        }

        internal Expression VisitSelectExpression(SelectExpression expression)
        {
            if (expression.Take != null)
            {
                _take = expression.Take.Limit;
            }
            VisitExpression(expression.Predicate);
            return expression;
        }

        internal Expression VisitPropertyExpression(PropertyExpression expression)
        {
            _whereStringBuilder.Append(expression.PropertyName);
            return expression;
        }

        internal Expression VisitQueryableConstant(QueryableConstantExpression expression)
        {
            _whereStringBuilder.Append(expression.QueryString);
            return expression;
        }

        protected override Expression VisitBinaryExpression(BinaryExpression expression)
        {
            if (IsFilterCombination(expression))
            {
                Decorate("(", () => VisitExpression(expression.Left), ")");
            }
            else
            {
                VisitExpression(expression.Left);
            }

            Decorate(" ", () => GenerateOperator(expression.NodeType), " ");

            if (IsFilterCombination(expression))
            {
                Decorate("(", () => VisitExpression(expression.Right), ")");
            }
            else
            {
                VisitExpression(expression.Right);
            }

            return expression;
        }

        private void GenerateOperator(ExpressionType nodeType)
        {
            switch (nodeType)
            {
                case ExpressionType.Equal:
                    _whereStringBuilder.Append(QueryComparisons.Equal);
                    break;
                case ExpressionType.GreaterThan:
                    _whereStringBuilder.Append(QueryComparisons.GreaterThan);
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    _whereStringBuilder.Append(QueryComparisons.GreaterThanOrEqual);
                    break;
                case ExpressionType.LessThan:
                    _whereStringBuilder.Append(QueryComparisons.LessThan);
                    break;
                case ExpressionType.LessThanOrEqual:
                    _whereStringBuilder.Append(QueryComparisons.LessThanOrEqual);
                    break;
                case ExpressionType.NotEqual:
                    _whereStringBuilder.Append(QueryComparisons.NotEqual);
                    break;
                case ExpressionType.AndAlso:
                    _whereStringBuilder.Append(TableOperators.And);
                    break;
                case ExpressionType.OrElse:
                    _whereStringBuilder.Append(TableOperators.Or);
                    break;
                case ExpressionType.Not:
                    _whereStringBuilder.Append(TableOperators.Not);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("nodeType", "Cannot match node type");
            }
        }

        private static bool IsFilterCombination(BinaryExpression expression)
        {
            return expression.NodeType == ExpressionType.AndAlso
                   || expression.NodeType == ExpressionType.OrElse
                   || expression.NodeType == ExpressionType.Not;
        }

        private void Decorate(string pre, Action func, string post)
        {
            _whereStringBuilder.Append(pre);
            func();
            _whereStringBuilder.Append(post);
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            throw new NotImplementedException(visitMethod);
        }
    }
}
