// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#nullable disable

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class QuerySqlGenerator : SqlExpressionVisitor
    {
        private readonly StringBuilder _sqlBuilder = new();
        private IReadOnlyDictionary<string, object> _parameterValues;
        private List<SqlParameter> _sqlParameters;
        private bool _useValueProjection;
        private ParameterNameGenerator _parameterNameGenerator;

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
            { ExpressionType.Not, "~" }
        };

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual CosmosSqlQuery GetSqlQuery(
            SelectExpression selectExpression,
            IReadOnlyDictionary<string, object> parameterValues)
        {
            _sqlBuilder.Clear();
            _parameterValues = parameterValues;
            _sqlParameters = new List<SqlParameter>();
            _parameterNameGenerator = new ParameterNameGenerator();

            Visit(selectExpression);
            return new CosmosSqlQuery(_sqlBuilder.ToString(), _sqlParameters);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitEntityProjection(EntityProjectionExpression entityProjectionExpression)
        {
            Check.NotNull(entityProjectionExpression, nameof(entityProjectionExpression));

            Visit(entityProjectionExpression.AccessExpression);

            return entityProjectionExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitObjectArrayProjection(ObjectArrayProjectionExpression objectArrayProjectionExpression)
        {
            Check.NotNull(objectArrayProjectionExpression, nameof(objectArrayProjectionExpression));

            _sqlBuilder.Append(objectArrayProjectionExpression);

            return objectArrayProjectionExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitKeyAccess(KeyAccessExpression keyAccessExpression)
        {
            Check.NotNull(keyAccessExpression, nameof(keyAccessExpression));

            _sqlBuilder.Append(keyAccessExpression);

            return keyAccessExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitObjectAccess(ObjectAccessExpression objectAccessExpression)
        {
            Check.NotNull(objectAccessExpression, nameof(objectAccessExpression));

            _sqlBuilder.Append(objectAccessExpression);

            return objectAccessExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitProjection(ProjectionExpression projectionExpression)
        {
            Check.NotNull(projectionExpression, nameof(projectionExpression));

            if (_useValueProjection)
            {
                _sqlBuilder.Append('"').Append(projectionExpression.Alias).Append("\" : ");
            }

            Visit(projectionExpression.Expression);

            if (!_useValueProjection
                && !string.IsNullOrEmpty(projectionExpression.Alias)
                && projectionExpression.Alias != projectionExpression.Name)
            {
                _sqlBuilder.Append(" AS " + projectionExpression.Alias);
            }

            return projectionExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitRootReference(RootReferenceExpression rootReferenceExpression)
        {
            Check.NotNull(rootReferenceExpression, nameof(rootReferenceExpression));

            _sqlBuilder.Append(rootReferenceExpression);

            return rootReferenceExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSelect(SelectExpression selectExpression)
        {
            Check.NotNull(selectExpression, nameof(selectExpression));

            _sqlBuilder.Append("SELECT ");

            if (selectExpression.IsDistinct)
            {
                _sqlBuilder.Append("DISTINCT ");
            }

            if (selectExpression.Projection.Count > 0)
            {
                if (selectExpression.Projection.Any(p => !string.IsNullOrEmpty(p.Alias) && p.Alias != p.Name)
                    && !selectExpression.Projection.Any(p => p.Expression is SqlFunctionExpression)) // Aggregates are not allowed
                {
                    _useValueProjection = true;
                    _sqlBuilder.Append("VALUE {");
                    GenerateList(selectExpression.Projection, e => Visit(e));
                    _sqlBuilder.Append('}');
                    _useValueProjection = false;
                }
                else
                {
                    GenerateList(selectExpression.Projection, e => Visit(e));
                }
            }
            else
            {
                _sqlBuilder.Append('1');
            }

            _sqlBuilder.AppendLine();

            if (selectExpression.FromExpression is FromSqlExpression)
            {
                _sqlBuilder.Append("FROM ");
            }
            else
            {
                _sqlBuilder.Append("FROM root ");
            }
            Visit(selectExpression.FromExpression);
            _sqlBuilder.AppendLine();

            if (selectExpression.Predicate != null)
            {
                _sqlBuilder.Append("WHERE ");
                Visit(selectExpression.Predicate);
            }

            if (selectExpression.Orderings.Any())
            {
                _sqlBuilder.AppendLine().Append("ORDER BY ");

                GenerateList(selectExpression.Orderings, e => Visit(e));
            }

            if (selectExpression.Offset != null
                || selectExpression.Limit != null)
            {
                _sqlBuilder.AppendLine().Append("OFFSET ");

                if (selectExpression.Offset != null)
                {
                    Visit(selectExpression.Offset);
                }
                else
                {
                    _sqlBuilder.Append('0');
                }

                _sqlBuilder.Append(" LIMIT ");

                if (selectExpression.Limit != null)
                {
                    Visit(selectExpression.Limit);
                }
                else
                {
                    // TODO: See Issue#18923
                    throw new InvalidOperationException(CosmosStrings.OffsetRequiresLimit);
                }
            }

            return selectExpression;
        }

        /// <inheritdoc />
        protected override Expression VisitFromSql(FromSqlExpression fromSqlExpression)
        {
            Check.NotNull(fromSqlExpression, nameof(fromSqlExpression));

            string[] substitutions = null;
            var sql = fromSqlExpression.Sql;

            switch (fromSqlExpression.Arguments)
            {
                case ConstantExpression constantExpression:
                    var existingValues = constantExpression.GetConstantValue<object[]>();
                    substitutions = new string[existingValues.Length];
                    for (int i = 0; i < existingValues.Length; i++)
                    {
                        var value = existingValues[i];

                        if (value is DbParameter dbParameter)
                        {
                            if (string.IsNullOrEmpty(dbParameter.ParameterName))
                            {
                                string parameterName = _parameterNameGenerator.GenerateNext();
                                _sqlParameters.Add(new SqlParameter(parameterName, dbParameter.Value));
                                substitutions[i] = parameterName;
                            }
                            else
                            {
                                _sqlParameters.Add(new SqlParameter(dbParameter.ParameterName, dbParameter.Value));
                            }
                        }
                        else
                        {
                            string parameterName = _parameterNameGenerator.GenerateNext();
                            _sqlParameters.Add(new SqlParameter(parameterName, value));
                            substitutions[i] = parameterName;
                        }
                    }
                    break;
                case ParameterExpression parameterExpression:
                    if (_parameterValues.ContainsKey(parameterExpression.Name))
                    {
                        var parameterValues = (object[])_parameterValues[parameterExpression.Name];
                        substitutions = new string[parameterValues.Length];
                        for (int i = 0; i < parameterValues.Length; i++)
                        {
                            var value = parameterValues[i];

                            if (value is DbParameter dbParameter)
                            {
                                if (string.IsNullOrEmpty(dbParameter.ParameterName))
                                {
                                    string parameterName = _parameterNameGenerator.GenerateNext();
                                    _sqlParameters.Add(new SqlParameter(parameterName, dbParameter.Value));
                                    substitutions[i] = parameterName;
                                }
                                else
                                {
                                    _sqlParameters.Add(new SqlParameter(dbParameter.ParameterName, dbParameter.Value));
                                }
                            }
                            else
                            {
                                string parameterName = _parameterNameGenerator.GenerateNext();
                                _sqlParameters.Add(new SqlParameter(parameterName, value));
                                substitutions[i] = parameterName;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            _sqlBuilder.AppendLine("(");

            if (substitutions != null)
            {
                sql = string.Format(sql, substitutions);
            }

            _sqlBuilder.Append(sql);

            _sqlBuilder.Append(")");

            _sqlBuilder.Append($" {fromSqlExpression.Alias}");

            return fromSqlExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitOrdering(OrderingExpression orderingExpression)
        {
            Check.NotNull(orderingExpression, nameof(orderingExpression));

            Visit(orderingExpression.Expression);

            if (!orderingExpression.IsAscending)
            {
                _sqlBuilder.Append(" DESC");
            }

            return orderingExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlBinary(SqlBinaryExpression sqlBinaryExpression)
        {
            Check.NotNull(sqlBinaryExpression, nameof(sqlBinaryExpression));

            var op = _operatorMap[sqlBinaryExpression.OperatorType];
            _sqlBuilder.Append('(');
            Visit(sqlBinaryExpression.Left);

            if (sqlBinaryExpression.OperatorType == ExpressionType.Add
                && sqlBinaryExpression.Left.Type == typeof(string))
            {
                op = " || ";
            }

            _sqlBuilder.Append(op);

            Visit(sqlBinaryExpression.Right);
            _sqlBuilder.Append(')');

            return sqlBinaryExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlUnary(SqlUnaryExpression sqlUnaryExpression)
        {
            Check.NotNull(sqlUnaryExpression, nameof(sqlUnaryExpression));

            var op = _operatorMap[sqlUnaryExpression.OperatorType];

            if (sqlUnaryExpression.OperatorType == ExpressionType.Not
                && sqlUnaryExpression.Operand.Type == typeof(bool))
            {
                op = "NOT";
            }

            _sqlBuilder.Append(op);

            _sqlBuilder.Append('(');
            Visit(sqlUnaryExpression.Operand);
            _sqlBuilder.Append(')');

            return sqlUnaryExpression;
        }

        private void GenerateList<T>(
            IReadOnlyList<T> items,
            Action<T> generationAction,
            Action<StringBuilder> joinAction = null)
        {
            joinAction ??= (isb => isb.Append(", "));

            for (var i = 0; i < items.Count; i++)
            {
                if (i > 0)
                {
                    joinAction(_sqlBuilder);
                }

                generationAction(items[i]);
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlConstant(SqlConstantExpression sqlConstantExpression)
        {
            Check.NotNull(sqlConstantExpression, nameof(sqlConstantExpression));

            var jToken = GenerateJToken(sqlConstantExpression.Value, sqlConstantExpression.TypeMapping);

            _sqlBuilder.Append(jToken == null ? "null" : jToken.ToString(Formatting.None));

            return sqlConstantExpression;
        }

        private JToken GenerateJToken(object value, CoreTypeMapping typeMapping)
        {
            if (value?.GetType().IsInteger() == true)
            {
                var unwrappedType = typeMapping.ClrType.UnwrapNullableType();
                value = unwrappedType.IsEnum
                    ? Enum.ToObject(unwrappedType, value)
                    : unwrappedType == typeof(char)
                        ? Convert.ChangeType(value, unwrappedType)
                        : value;
            }

            var converter = typeMapping.Converter;
            if (converter != null)
            {
                value = converter.ConvertToProvider(value);
            }

            return value == null
                ? null
                : (value as JToken) ?? JToken.FromObject(value, CosmosClientWrapper.Serializer);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlConditional(SqlConditionalExpression sqlConditionalExpression)
        {
            Check.NotNull(sqlConditionalExpression, nameof(sqlConditionalExpression));

            _sqlBuilder.Append('(');
            Visit(sqlConditionalExpression.Test);
            _sqlBuilder.Append(" ? ");
            Visit(sqlConditionalExpression.IfTrue);
            _sqlBuilder.Append(" : ");
            Visit(sqlConditionalExpression.IfFalse);
            _sqlBuilder.Append(')');

            return sqlConditionalExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlParameter(SqlParameterExpression sqlParameterExpression)
        {
            Check.NotNull(sqlParameterExpression, nameof(sqlParameterExpression));

            var parameterName = $"@{sqlParameterExpression.Name}";

            if (_sqlParameters.All(sp => sp.Name != parameterName))
            {
                var jToken = GenerateJToken(_parameterValues[sqlParameterExpression.Name], sqlParameterExpression.TypeMapping);
                _sqlParameters.Add(new SqlParameter(parameterName, jToken));
            }

            _sqlBuilder.Append(parameterName);

            return sqlParameterExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitIn(InExpression inExpression)
        {
            Check.NotNull(inExpression, nameof(inExpression));

            Visit(inExpression.Item);
            _sqlBuilder.Append(inExpression.IsNegated ? " NOT IN " : " IN ");
            _sqlBuilder.Append('(');
            var valuesConstant = (SqlConstantExpression)inExpression.Values;
            var valuesList = ((IEnumerable<object>)valuesConstant.Value)
                .Select(v => new SqlConstantExpression(Expression.Constant(v), valuesConstant.TypeMapping)).ToList();
            GenerateList(valuesList, e => Visit(e));
            _sqlBuilder.Append(')');

            return inExpression;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected override Expression VisitSqlFunction(SqlFunctionExpression sqlFunctionExpression)
        {
            Check.NotNull(sqlFunctionExpression, nameof(sqlFunctionExpression));

            _sqlBuilder.Append(sqlFunctionExpression.Name);
            _sqlBuilder.Append('(');
            GenerateList(sqlFunctionExpression.Arguments, e => Visit(e));
            _sqlBuilder.Append(')');

            return sqlFunctionExpression;
        }
    }
}
