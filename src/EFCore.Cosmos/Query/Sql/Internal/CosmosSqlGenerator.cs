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
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Sql.Internal
{
    public class CosmosSqlGenerator : ExpressionVisitor, ISqlGenerator
    {
        private readonly SelectExpression _selectExpression;
        private readonly ITypeMappingSource _typeMappingSource;
        private CoreTypeMapping _typeMapping;

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
            { ExpressionType.Coalesce, " ?? " }
        };

        public CosmosSqlGenerator(SelectExpression selectExpression, ITypeMappingSource typeMappingSource)
        {
            _selectExpression = selectExpression;
            _typeMappingSource = typeMappingSource;
        }

        public CosmosSqlQuery GenerateSqlQuery(
            IReadOnlyDictionary<string, object> parameterValues)
        {
            _sqlBuilder.Clear();
            _parameterValues = parameterValues;
            _sqlParameters = new List<SqlParameter>();

            Visit(_selectExpression);

            return new CosmosSqlQuery(_sqlBuilder.ToString(), _sqlParameters);
        }

        protected override Expression VisitBinary(BinaryExpression binaryExpression)
        {
            if (!_operatorMap.TryGetValue(binaryExpression.NodeType, out var op))
            {
                return base.VisitBinary(binaryExpression);
            }

            var parentTypeMapping = _typeMapping;

            if (binaryExpression.IsComparisonOperation()
                || binaryExpression.NodeType == ExpressionType.Add
                || binaryExpression.NodeType == ExpressionType.Coalesce)
            {
                _typeMapping
                    = FindTypeMapping(binaryExpression.Left)
                      ?? FindTypeMapping(binaryExpression.Right)
                      ?? parentTypeMapping;
            }

            _sqlBuilder.Append("(");
            Visit(binaryExpression.Left);

            if (binaryExpression.NodeType == ExpressionType.Add
                && binaryExpression.Left.Type == typeof(string))
            {
                op = " || ";
            }

            _sqlBuilder.Append(op);

            Visit(binaryExpression.Right);
            _sqlBuilder.Append(")");

            _typeMapping = parentTypeMapping;
            return binaryExpression;
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
            var jToken = GenerateJToken(constantExpression.Value, constantExpression.Type, _typeMapping);

            _sqlBuilder.Append(jToken == null ? "null" : jToken.ToString(Formatting.None));

            return constantExpression;
        }

        private JToken GenerateJToken(object value, Type type, CoreTypeMapping typeMapping)
        {
            var mappingClrType = typeMapping?.ClrType.UnwrapNullableType();
            if (mappingClrType != null
                && (value == null
                    || mappingClrType.IsInstanceOfType(value)
                    || (value.GetType().IsInteger()
                        && (mappingClrType.IsInteger()
                            || mappingClrType.IsEnum))))
            {
                if (value?.GetType().IsInteger() == true
                    && mappingClrType.IsEnum)
                {
                    value = Enum.ToObject(mappingClrType, value);
                }
            }
            else
            {
                var mappingType = (value?.GetType() ?? type).UnwrapNullableType();
                typeMapping = _typeMappingSource.FindMapping(mappingType);

                if (typeMapping == null)
                {
                    throw new InvalidOperationException($"Unsupported parameter type {mappingType.ShortDisplayName()}");
                }
            }

            var converter = typeMapping.Converter;
            if (converter != null)
            {
                value = converter.ConvertToProvider(value);
            }

            if (value == null)
            {
                return null;
            }

            return (value as JToken) ?? JToken.FromObject(value, CosmosClientWrapper.Serializer);
        }

        private CoreTypeMapping FindTypeMapping(Expression expression)
            => FindProperty(expression)?.FindMapping();

        private static IProperty FindProperty(Expression expression)
        {
            switch (expression)
            {
                case KeyAccessExpression keyAccessExpression:
                    return keyAccessExpression.PropertyBase as IProperty;
                case UnaryExpression unaryExpression:
                    return FindProperty(unaryExpression.Operand);
            }

            return null;
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
                var jToken = GenerateJToken(_parameterValues[parameterExpression.Name], parameterExpression.Type, _typeMapping);
                _sqlParameters.Add(new SqlParameter(parameterName, jToken));
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
