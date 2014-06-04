// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Text;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Query.Expressions;
using Microsoft.Data.Entity.Relational.Utilities;
using Remotion.Linq.Parsing;

namespace Microsoft.Data.Entity.Relational.Query.Sql
{
    public class SqlGeneratingExpressionTreeVisitor : ThrowingExpressionTreeVisitor, ISqlExpressionVisitor
    {
        private readonly StringBuilder _sql;
        private readonly IParameterFactory _parameterFactory;

        private ExpressionType? _currentOperator;

        public SqlGeneratingExpressionTreeVisitor(
            [NotNull] StringBuilder sql, [NotNull] IParameterFactory parameterFactory)
        {
            Check.NotNull(sql, "sql");
            Check.NotNull(parameterFactory, "parameterFactory");

            _sql = sql;
            _parameterFactory = parameterFactory;
        }

        protected override Expression VisitBinaryExpression([NotNull] BinaryExpression expression)
        {
            Check.NotNull(expression, "expression");

            _currentOperator = expression.NodeType;

            _sql.Append("(");

            VisitExpression(expression.Left);

            string op;

            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    op = " = ";
                    break;
                case ExpressionType.NotEqual:
                    op = " <> ";
                    break;
                case ExpressionType.GreaterThan:
                    op = " > ";
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    op = " >= ";
                    break;
                case ExpressionType.LessThan:
                    op = " < ";
                    break;
                case ExpressionType.LessThanOrEqual:
                    op = " <= ";
                    break;
                case ExpressionType.AndAlso:
                    op = " AND ";
                    break;
                case ExpressionType.OrElse:
                    op = " OR ";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _sql.Append(op);

            VisitExpression(expression.Right);

            _sql.Append(")");

            _currentOperator = null;

            return expression;
        }

        public virtual Expression VisitPropertyAccessExpression(PropertyAccessExpression expression)
        {
            _sql.Append(expression.Property.StorageName);

            return expression;
        }

        public virtual Expression VisitIsNullExpression(IsNullExpression expression)
        {
            _sql.Append("(");

            VisitExpression(expression.Operand);

            _sql.Append(" IS NULL)");

            return expression;
        }

        public virtual Expression VisitIsNotNullExpression(IsNotNullExpression expression)
        {
            _sql.Append("(");

            VisitExpression(expression.Operand);

            _sql.Append(" IS NOT NULL)");

            return expression;
        }

        protected override Expression VisitConstantExpression(ConstantExpression expression)
        {
            var maybeBool = expression.Value as bool?;

            if (maybeBool != null
                && (_currentOperator == null
                    || _currentOperator == ExpressionType.AndAlso
                    || _currentOperator == ExpressionType.OrElse))
            {
                _sql.Append(maybeBool.Value ? "(1=1)" : "(1=0)");
            }
            else
            {
                _sql.Append(_parameterFactory.CreateParameter(expression.Value));
            }

            return expression;
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            return new NotImplementedException(visitMethod);
        }
    }
}
