// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Expressions.Internal;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SqlTranslatingExpressionVisitor : ThrowingExpressionVisitor
    {
        readonly DocumentDbQueryModelVisitor _queryModelVisitor;
        readonly SelectExpression _selectExpression;
        public SqlTranslatingExpressionVisitor(
            DocumentDbQueryModelVisitor queryModelVisitor, SelectExpression selectExpression)
        {
            _queryModelVisitor = queryModelVisitor;
            _selectExpression = selectExpression;
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            throw new NotImplementedException(typeof(T).DisplayName());
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            var left = Visit(expression.Left);
            var right = Visit(expression.Right);

            if (left != null && right != null)
            {
                return expression.Update(left, expression.Conversion, right);
            }

            return null;
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            return expression;
        }

        protected override Expression VisitExtension(Expression expression)
        {
            switch (expression)
            {
                case NullConditionalExpression nullConditionalExpression:
                    return Visit(nullConditionalExpression.AccessOperation);
            }

            return null;
        }

        protected override Expression VisitQuerySourceReference(QuerySourceReferenceExpression expression)
        {
            return null;
        }

        protected override Expression VisitConditional(ConditionalExpression expression)
        {
            var test = Visit(expression.Test);
            var ifTrue = Visit(expression.IfTrue);
            var ifFalse = Visit(expression.IfFalse);

            if (test != null && ifTrue != null && ifFalse != null)
            {
                return expression.Update(test, ifTrue, ifFalse);
            }

            return null;
        }

        protected override Expression VisitNew(NewExpression expression)
        {
            return null;
        }

        protected override Expression VisitTypeBinary(TypeBinaryExpression expression)
        {
            return null;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return _queryModelVisitor.BindMemberExpression(node, (p, qs, se) => se.BindProperty(p, qs));
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            return _queryModelVisitor.BindMethodCallExpression(expression, (p, qs, se) => se.BindProperty(p, qs));
        }

        protected override Expression VisitParameter(ParameterExpression expression)
        {
            return expression;
        }

        protected override Expression VisitSubQuery(SubQueryExpression expression)
        {
            var subQueryModel = expression.QueryModel;
            var subQueryOutputDataInfo = subQueryModel.GetOutputDataInfo();

            if (!(subQueryOutputDataInfo is StreamedSequenceInfo))
            {
                var subQueryModelVisitor = (DocumentDbQueryModelVisitor)_queryModelVisitor.QueryCompilationContext
                    .CreateQueryModelVisitor(_queryModelVisitor);

                subQueryModelVisitor.VisitQueryModel(subQueryModel);

                if (subQueryModelVisitor.Expression is ShapedQueryExpression)
                {
                    var selectExpression = subQueryModelVisitor.Queries.Single();

                    return selectExpression;
                }
            }

            return null;
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            var newOperand = Visit(expression.Operand);
            if (newOperand != null)
            {
                return expression.Update(newOperand);
            }

            return null;
        }
    }
}
