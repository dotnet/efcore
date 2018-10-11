// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore.Cosmos.Query.Expressions.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.ExpressionVisitors.Internal
{
    public class CosmosSqlGenerator : ExpressionVisitor
    {
        private readonly StringBuilder _sqlBuilder = new StringBuilder();
        private IReadOnlyDictionary<string, object> _parameterValues;
        private List<SqlParameter> _sqlParameters;

        private readonly IDictionary<ExpressionType, string> _operatorMap = new Dictionary<ExpressionType, string>
        {
            // Arithmetic
            { ExpressionType.Add, " + " },
            { ExpressionType.Subtract, " - " },
            { ExpressionType.Multiply, " * " },
            { ExpressionType.Divide, " / " },
            { ExpressionType.Modulo, " % " },

            // Bitwise >>> (zero-fill right shift) not available in C#
            { ExpressionType.Or, " | " },
            { ExpressionType.And, " & " },
            { ExpressionType.ExclusiveOr, " ^ " },
            { ExpressionType.LeftShift, " << " },
            { ExpressionType.RightShift, " >> " },

            // Logical
            { ExpressionType.AndAlso, " AND " },
            { ExpressionType.OrElse, " OR " },

            // Comparison
            { ExpressionType.Equal, " = " },
            { ExpressionType.NotEqual, " != " },
            { ExpressionType.GreaterThan, " > " },
            { ExpressionType.GreaterThanOrEqual, " >= " },
            { ExpressionType.LessThan, " < " },
            { ExpressionType.LessThanOrEqual, " <= " },

            // Unary
            { ExpressionType.UnaryPlus, "+" },
            { ExpressionType.Negate, "-" },
            { ExpressionType.Not, "~" },

            // Others
            { ExpressionType.Coalesce, " ?? " },
        };

        public CosmosSqlQuery GenerateSqlQuerySpec(
            SelectExpression selectExpression,
            IReadOnlyDictionary<string, object> parameterValues)
        {
            _sqlBuilder.Clear();
            _parameterValues = parameterValues;
            _sqlParameters = new List<SqlParameter>();

            Visit(selectExpression);

            return new CosmosSqlQuery(_sqlBuilder.ToString(), _sqlParameters);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (_operatorMap.ContainsKey(binaryExpression.NodeType))
            {
                _sqlBuilder.Append("(");
                Visit(binaryExpression.Left);
                var op = _operatorMap[binaryExpression.NodeType];

                if (binaryExpression.NodeType == ExpressionType.Add
                    && binaryExpression.Left.Type == typeof(string))
                {
                    op = " || ";
                }

                _sqlBuilder.Append(op);

                Visit(binaryExpression.Right);
                _sqlBuilder.Append(")");

                return binaryExpression;
            }

            return base.VisitBinary(binaryExpression);
        }

        protected override Expression VisitConditional(ConditionalExpression conditionalExpression)
        {
            _sqlBuilder.Append("(");
            Visit(conditionalExpression.Test);
            _sqlBuilder.Append(" ? ");
            Visit(conditionalExpression.IfTrue);
            _sqlBuilder.Append(" : ");
            Visit(conditionalExpression.IfFalse);
            _sqlBuilder.Append(")");

            return conditionalExpression;
        }

        protected override Expression VisitConstant(ConstantExpression constantExpression)
        {
            var value = constantExpression.Value;

            if (value is null)
            {
                _sqlBuilder.Append("null");
            }
            else if (value.GetType().IsNumeric() || value is DateTime || value is DateTimeOffset)
            {
                var jsonValue = JToken.FromObject(value);
                _sqlBuilder.Append(jsonValue.ToString(Formatting.None));
            }
            else if (value is bool boolValue)
            {
                _sqlBuilder.Append(boolValue ? "true" : "false");
            }
            else
            {
                _sqlBuilder.Append("\"").Append(value).Append("\"");
            }

            return constantExpression;
        }

        protected override Expression VisitExtension(Expression extensionExpression)
        {
            switch (extensionExpression)
            {
                case SelectExpression selectExpression:

                    _sqlBuilder.Append("SELECT ");
                    Visit(selectExpression.Projection);
                    _sqlBuilder.AppendLine();

                    _sqlBuilder.Append("FROM root ");
                    Visit(selectExpression.FromExpression);
                    _sqlBuilder.AppendLine();

                    _sqlBuilder.Append("WHERE ");
                    Visit(selectExpression.FilterExpression);

                    return extensionExpression;

                case RootReferenceExpression rootReferenceExpression:
                    _sqlBuilder.Append(rootReferenceExpression);
                    return extensionExpression;

                case KeyAccessExpression keyAccessExpression:
                    _sqlBuilder.Append(keyAccessExpression);
                    return extensionExpression;

                case EntityProjectionExpression entityProjectionExpression:
                    _sqlBuilder.Append(entityProjectionExpression);
                    return extensionExpression;
            }

            return base.VisitExtension(extensionExpression);
        }

        protected override Expression VisitParameter(ParameterExpression parameterExpression)
        {
            var parameterName = $"@{parameterExpression.Name}";

            if (_sqlParameters.All(sp => sp.Name != parameterName))
            {
                _sqlParameters.Add(new SqlParameter(parameterName, _parameterValues[parameterExpression.Name]));
            }

            _sqlBuilder.Append(parameterName);

            return parameterExpression;
        }

        protected override Expression VisitUnary(UnaryExpression unaryExpression)
        {
            if (_operatorMap.ContainsKey(unaryExpression.NodeType))
            {
                var op = _operatorMap[unaryExpression.NodeType];

                if (unaryExpression.NodeType == ExpressionType.Not
                    && unaryExpression.Operand.Type == typeof(bool))
                {
                    op = "NOT";
                }

                _sqlBuilder.Append(op);

                _sqlBuilder.Append("(");
                Visit(unaryExpression.Operand);
                _sqlBuilder.Append(")");

                return unaryExpression;
            }

            return base.VisitUnary(unaryExpression);
        }
    }
}
