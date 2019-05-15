// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class SqlExpressionFactory : ISqlExpressionFactory
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly RelationalTypeMapping _boolTypeMapping;

        public SqlExpressionFactory(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
            _boolTypeMapping = typeMappingSource.FindMapping(typeof(bool));
        }

        #region TypeMapping
        public SqlExpression ApplyDefaultTypeMapping(SqlExpression sqlExpression)
        {
            if (sqlExpression == null
                || sqlExpression.TypeMapping != null)
            {
                return sqlExpression;
            }

            return ApplyTypeMapping(sqlExpression, _typeMappingSource.FindMapping(sqlExpression.Type));
        }

        public SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, RelationalTypeMapping typeMapping)
        {
            if (sqlExpression == null
                || sqlExpression.TypeMapping != null)
            {
                return sqlExpression;
            }

            switch (sqlExpression)
            {
                case CaseExpression caseExpression:
                    return ApplyTypeMappingOnCase(caseExpression, typeMapping);

                case LikeExpression likeExpression:
                    return ApplyTypeMappingOnLike(likeExpression, typeMapping);

                case SqlBinaryExpression sqlBinaryExpression:
                    return ApplyTypeMappingOnSqlBinary(sqlBinaryExpression, typeMapping);

                case SqlUnaryExpression sqlUnaryExpression:
                    return ApplyTypeMappingOnSqlUnary(sqlUnaryExpression, typeMapping);

                case SqlConstantExpression sqlConstantExpression:
                    return sqlConstantExpression.ApplyTypeMapping(typeMapping);

                case SqlFragmentExpression sqlFragmentExpression:
                    return sqlFragmentExpression;

                case SqlFunctionExpression sqlFunctionExpression:
                    return sqlFunctionExpression.ApplyTypeMapping(typeMapping);

                case SqlParameterExpression sqlParameterExpression:
                    return sqlParameterExpression.ApplyTypeMapping(typeMapping);

                default:
                    return sqlExpression;
            }
        }

        private SqlExpression ApplyTypeMappingOnLike(LikeExpression likeExpression, RelationalTypeMapping typeMapping)
        {
            var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(
                likeExpression.Match, likeExpression.Pattern, likeExpression.EscapeChar)
                ?? _typeMappingSource.FindMapping(likeExpression.Match.Type);

            return new LikeExpression(
                ApplyTypeMapping(likeExpression.Match, inferredTypeMapping),
                ApplyTypeMapping(likeExpression.Pattern, inferredTypeMapping),
                ApplyTypeMapping(likeExpression.EscapeChar, inferredTypeMapping),
                _boolTypeMapping);
        }

        private SqlExpression ApplyTypeMappingOnCase(
            CaseExpression caseExpression, RelationalTypeMapping typeMapping)
        {
            var whenClauses = new List<CaseWhenClause>();
            foreach (var caseWhenClause in caseExpression.WhenClauses)
            {
                whenClauses.Add(
                    new CaseWhenClause(
                        caseWhenClause.Test,
                        ApplyTypeMapping(caseWhenClause.Result, typeMapping)));
            }

            var elseResult = ApplyTypeMapping(caseExpression.ElseResult, typeMapping);

            return caseExpression.Update(caseExpression.Operand, whenClauses, elseResult);
        }

        private SqlExpression ApplyTypeMappingOnSqlUnary(
            SqlUnaryExpression sqlUnaryExpression, RelationalTypeMapping typeMapping)
        {
            SqlExpression operand;
            RelationalTypeMapping resultTypeMapping;
            switch (sqlUnaryExpression.OperatorType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Not:
                    resultTypeMapping = _boolTypeMapping;
                    operand = ApplyDefaultTypeMapping(sqlUnaryExpression.Operand);
                    break;

                case ExpressionType.Convert:
                    resultTypeMapping = typeMapping;
                    operand = ApplyDefaultTypeMapping(sqlUnaryExpression.Operand);
                    break;

                case ExpressionType.Negate:
                    resultTypeMapping = typeMapping;
                    operand = ApplyTypeMapping(sqlUnaryExpression.Operand, typeMapping);
                    break;

                default:
                    throw new InvalidOperationException();
            }

            return new SqlUnaryExpression(
                sqlUnaryExpression.OperatorType,
                operand,
                sqlUnaryExpression.Type,
                resultTypeMapping);
        }

        private SqlExpression ApplyTypeMappingOnSqlBinary(
            SqlBinaryExpression sqlBinaryExpression, RelationalTypeMapping typeMapping)
        {
            var left = sqlBinaryExpression.Left;
            var right = sqlBinaryExpression.Right;

            Type resultType;
            RelationalTypeMapping resultTypeMapping;
            RelationalTypeMapping inferredTypeMapping;
            switch (sqlBinaryExpression.OperatorType)
            {
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    {
                        inferredTypeMapping = ExpressionExtensions.InferTypeMapping(left, right)
                            ?? _typeMappingSource.FindMapping(left.Type);
                        resultType = typeof(bool);
                        resultTypeMapping = _boolTypeMapping;
                    }
                    break;

                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    {
                        inferredTypeMapping = _boolTypeMapping;
                        resultType = typeof(bool);
                        resultTypeMapping = _boolTypeMapping;
                    }
                    break;

                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.Coalesce:
                case ExpressionType.And:
                case ExpressionType.Or:
                    {
                        inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                        resultType = left.Type;
                        resultTypeMapping = inferredTypeMapping;
                    }
                    break;

                default:
                    throw new InvalidOperationException("Incorrect operatorType for SqlBinaryExpression");
            }

            return new SqlBinaryExpression(
                sqlBinaryExpression.OperatorType,
                ApplyTypeMapping(left, inferredTypeMapping),
                ApplyTypeMapping(right, inferredTypeMapping),
                resultType,
                resultTypeMapping);
        }
        #endregion

        #region Binary
        public SqlBinaryExpression MakeBinary(
            ExpressionType operatorType, SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping)
        {
            var returnType = left.Type;
            switch (operatorType)
            {
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    returnType = typeof(bool);
                    break;
            }

            return (SqlBinaryExpression)ApplyTypeMapping(
                new SqlBinaryExpression(operatorType, left, right, returnType, null), typeMapping);
        }

        public SqlBinaryExpression Equal(SqlExpression left, SqlExpression right)
        {
            return MakeBinary(ExpressionType.Equal, left, right, null);
        }

        public SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right)
        {
            return MakeBinary(ExpressionType.NotEqual, left, right, null);
        }

        public SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right)
        {
            return MakeBinary(ExpressionType.GreaterThan, left, right, null);
        }

        public SqlBinaryExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right)
        {
            return MakeBinary(ExpressionType.GreaterThanOrEqual, left, right, null);
        }

        public SqlBinaryExpression LessThan(SqlExpression left, SqlExpression right)
        {
            return MakeBinary(ExpressionType.LessThan, left, right, null);
        }

        public SqlBinaryExpression LessThanOrEqual(SqlExpression left, SqlExpression right)
        {
            return MakeBinary(ExpressionType.LessThanOrEqual, left, right, null);
        }

        public SqlBinaryExpression AndAlso(SqlExpression left, SqlExpression right)
        {
            return MakeBinary(ExpressionType.AndAlso, left, right, null);
        }

        public SqlBinaryExpression OrElse(SqlExpression left, SqlExpression right)
        {
            return MakeBinary(ExpressionType.OrElse, left, right, null);
        }

        public SqlBinaryExpression Add(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Add, left, right, typeMapping);
        }

        public SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Subtract, left, right, typeMapping);
        }

        public SqlBinaryExpression Multiply(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Multiply, left, right, typeMapping);
        }

        public SqlBinaryExpression Divide(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Divide, left, right, typeMapping);
        }

        public SqlBinaryExpression Modulo(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Modulo, left, right, typeMapping);
        }

        public SqlBinaryExpression And(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.And, left, right, typeMapping);
        }

        public SqlBinaryExpression Or(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Or, left, right, typeMapping);
        }

        public SqlBinaryExpression Coalesce(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Coalesce, left, right, typeMapping);
        }

        #endregion

        #region Unary
        private SqlUnaryExpression MakeUnary(
            ExpressionType operatorType, SqlExpression operand, Type type, RelationalTypeMapping typeMapping = null)
        {
            return (SqlUnaryExpression)ApplyTypeMapping(new SqlUnaryExpression(operatorType, operand, type, null), typeMapping);
        }

        public SqlUnaryExpression IsNull(SqlExpression operand)
        {
            return MakeUnary(ExpressionType.Equal, operand, typeof(bool));
        }

        public SqlUnaryExpression IsNotNull(SqlExpression operand)
        {
            return MakeUnary(ExpressionType.NotEqual, operand, typeof(bool));
        }

        public SqlUnaryExpression Convert(SqlExpression operand, Type type, RelationalTypeMapping typeMapping = null)
        {
            return MakeUnary(ExpressionType.Convert, operand, type, typeMapping);
        }
        public SqlUnaryExpression Not(SqlExpression operand)
        {
            return MakeUnary(ExpressionType.Not, operand, typeof(bool));
        }

        public SqlUnaryExpression Negate(SqlExpression operand)
        {
            return MakeUnary(ExpressionType.Negate, operand, operand.Type, operand.TypeMapping);
        }
        #endregion

        #region Case block
        public CaseExpression Case(SqlExpression operand, params CaseWhenClause[] whenClauses)
        {
            var operandTypeMapping = operand.TypeMapping
                ?? whenClauses.Select(wc => wc.Test.TypeMapping).FirstOrDefault(t => t != null)
                ?? _typeMappingSource.FindMapping(operand.Type);
            var resultTypeMapping = whenClauses.Select(wc => wc.Result.TypeMapping).FirstOrDefault(t => t != null);

            operand = ApplyTypeMapping(operand, operandTypeMapping);

            var typeMappedWhenClauses = new List<CaseWhenClause>();
            foreach (var caseWhenClause in whenClauses)
            {
                typeMappedWhenClauses.Add(
                    new CaseWhenClause(
                        ApplyTypeMapping(caseWhenClause.Test, operandTypeMapping),
                        ApplyTypeMapping(caseWhenClause.Result, resultTypeMapping)));
            }


            return new CaseExpression(operand, typeMappedWhenClauses);

        }

        public CaseExpression Case(IReadOnlyList<CaseWhenClause> whenClauses, SqlExpression elseResult)
        {
            var resultTypeMapping = elseResult?.TypeMapping
                ?? whenClauses.Select(wc => wc.Result.TypeMapping).FirstOrDefault(t => t != null);

            var typeMappedWhenClauses = new List<CaseWhenClause>();
            foreach (var caseWhenClause in whenClauses)
            {
                typeMappedWhenClauses.Add(
                    new CaseWhenClause(
                        ApplyTypeMapping(caseWhenClause.Test, _boolTypeMapping),
                        ApplyTypeMapping(caseWhenClause.Result, resultTypeMapping)));
            }

            elseResult = ApplyTypeMapping(elseResult, resultTypeMapping);

            return new CaseExpression(typeMappedWhenClauses, elseResult);
        }
        #endregion

        #region Functions
        public SqlFunctionExpression Function(
            string functionName, IEnumerable<SqlExpression> arguments, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            var typeMappedArguments = new List<SqlExpression>();

            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
            }

            return new SqlFunctionExpression(
                functionName,
                typeMappedArguments,
                returnType,
                typeMapping);
        }

        public SqlFunctionExpression Function(
            string schema, string functionName, IEnumerable<SqlExpression> arguments, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            var typeMappedArguments = new List<SqlExpression>();

            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
            }

            return new SqlFunctionExpression(
                schema,
                functionName,
                typeMappedArguments,
                returnType,
                typeMapping);
        }

        public SqlFunctionExpression Function(
            SqlExpression instance, string functionName, IEnumerable<SqlExpression> arguments, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            instance = ApplyDefaultTypeMapping(instance);
            var typeMappedArguments = new List<SqlExpression>();
            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
            }

            return new SqlFunctionExpression(
                instance,
                functionName,
                typeMappedArguments,
                returnType,
                typeMapping);
        }

        public SqlFunctionExpression Function(
            string functionName, bool niladic, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            return new SqlFunctionExpression(functionName, niladic, returnType, typeMapping);
        }

        public SqlFunctionExpression Function(
            string schema, string functionName, bool niladic, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            return new SqlFunctionExpression(schema, functionName, niladic, returnType, typeMapping);
        }

        public SqlFunctionExpression Function(
            SqlExpression instance, string functionName, bool niladic, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            instance = ApplyDefaultTypeMapping(instance);
            return new SqlFunctionExpression(instance, functionName, niladic, returnType, typeMapping);
        }


        #endregion

        #region Other Sql specific constructs
        public ExistsExpression Exists(SelectExpression subquery, bool negated)
        {
            return new ExistsExpression(subquery, negated, _boolTypeMapping);
        }

        public InExpression In(SqlExpression item, SqlExpression values, bool negated)
        {
            var typeMapping = item.TypeMapping ?? _typeMappingSource.FindMapping(item.Type);

            item = ApplyTypeMapping(item, typeMapping);
            values = ApplyTypeMapping(values, typeMapping);

            return new InExpression(item, negated, values, _boolTypeMapping);
        }

        public InExpression In(SqlExpression item, SelectExpression subquery, bool negated)
        {
            var typeMapping = subquery.Projection.Single().Expression.TypeMapping;

            if (typeMapping == null)
            {
                throw new InvalidOperationException();
            }

            item = ApplyTypeMapping(item, typeMapping);
            return new InExpression(item, negated, subquery, _boolTypeMapping);
        }

        public LikeExpression Like(SqlExpression match, SqlExpression pattern, SqlExpression escapeChar = null)
        {
            return (LikeExpression)ApplyDefaultTypeMapping(new LikeExpression(match, pattern, escapeChar, null));
        }

        public SqlFragmentExpression Fragment(string sql)
        {
            return new SqlFragmentExpression(sql);
        }

        public SqlConstantExpression Constant(object value, RelationalTypeMapping typeMapping = null)
        {
            return new SqlConstantExpression(Expression.Constant(value), typeMapping);
        }

        #endregion
    }
}
