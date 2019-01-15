// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Relational.Query.Pipeline.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Relational.Query.Pipeline
{
    public class TypeMappingApplyingExpressionVisitor : ITypeMappingApplyingExpressionVisitor
    {
        private readonly RelationalTypeMapping _boolTypeMapping;
        private readonly IRelationalTypeMappingSource _typeMappingSource;

        public TypeMappingApplyingExpressionVisitor(IRelationalTypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
            _boolTypeMapping = typeMappingSource.FindMapping(typeof(bool));
        }

        public virtual SqlExpression ApplyTypeMapping(
            SqlExpression expression, RelationalTypeMapping typeMapping)
        {
            if (expression == null)
            {
                return null;
            }

            if (expression.TypeMapping != null)
            // ColumnExpression, SqlNullExpression, SqlNotExpression should be captured here.
            {
                return expression;
            }

            switch (expression)
            {
                case CaseExpression caseExpression:
                    return ApplyTypeMappingOnCase(caseExpression, typeMapping);

                case LikeExpression likeExpression:
                    return ApplyTypeMappingOnLike(likeExpression, typeMapping);

                case SqlBinaryExpression sqlBinaryExpression:
                    return ApplyTypeMappingOnSqlBinary(sqlBinaryExpression, typeMapping);

                case SqlCastExpression sqlCastExpression:
                    return ApplyTypeMappingOnSqlCast(sqlCastExpression, typeMapping);

                case SqlConstantExpression sqlConstantExpression:
                    return ApplyTypeMappingOnSqlConstant(sqlConstantExpression, typeMapping);

                case SqlFragmentExpression sqlFragmentExpression:
                    return sqlFragmentExpression;

                case SqlFunctionExpression sqlFunctionExpression:
                    return ApplyTypeMappingOnSqlFunction(sqlFunctionExpression, typeMapping);

                case SqlParameterExpression sqlParameterExpression:
                    return ApplyTypeMappingOnSqlParameter(sqlParameterExpression, typeMapping);

                default:
                    return ApplyTypeMappingOnExtension(expression, typeMapping);

            }
        }

        protected virtual SqlExpression ApplyTypeMappingOnSqlCast(
            SqlCastExpression sqlCastExpression, RelationalTypeMapping typeMapping)
        {
            if (typeMapping == null)
            {
                return sqlCastExpression;
            }

            var operand = ApplyTypeMapping(
                sqlCastExpression.Operand,
                _typeMappingSource.FindMapping(sqlCastExpression.Operand.Type));

            return new SqlCastExpression(
                operand,
                sqlCastExpression.Type,
                typeMapping);
        }

        protected virtual SqlExpression ApplyTypeMappingOnCase(
            CaseExpression caseExpression, RelationalTypeMapping typeMapping)
        {
            var inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(caseExpression.ElseResult);

            if (inferredTypeMapping == null)
            {
                foreach (var caseWhenClause in caseExpression.WhenClauses)
                {
                    inferredTypeMapping = ExpressionExtensions.InferTypeMapping(caseWhenClause.Result);

                    if (inferredTypeMapping != null)
                    {
                        break;
                    }
                }
            }

            if (inferredTypeMapping == null)
            {
                throw new InvalidOperationException("TypeMapping should not be null.");
            }

            var whenClauses = new List<CaseWhenClause>();

            foreach (var caseWhenClause in caseExpression.WhenClauses)
            {
                whenClauses.Add(
                    new CaseWhenClause(
                        ApplyTypeMapping(caseWhenClause.Test, _boolTypeMapping),
                        ApplyTypeMapping(caseWhenClause.Result, inferredTypeMapping)));
            }

            var elseResult = ApplyTypeMapping(caseExpression.ElseResult, inferredTypeMapping);

            return new CaseExpression(
                whenClauses,
                elseResult);
        }

        protected virtual SqlExpression ApplyTypeMappingOnSqlBinary(
            SqlBinaryExpression sqlBinaryExpression, RelationalTypeMapping typeMapping)
        {
            var left = sqlBinaryExpression.Left;
            var right = sqlBinaryExpression.Right;

            switch (sqlBinaryExpression.OperatorType)
            {
                case ExpressionType.Equal:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.NotEqual:
                    {
                        if (sqlBinaryExpression.Type != typeof(bool))
                        {
                            throw new InvalidCastException("Comparison operation should be of type bool.");
                        }

                        var inferredTypeMapping = InferTypeMappingForBinary(left, right);

                        left = ApplyTypeMapping(left, inferredTypeMapping);
                        right = ApplyTypeMapping(right, inferredTypeMapping);

                        return new SqlBinaryExpression(
                            sqlBinaryExpression.OperatorType,
                            left,
                            right,
                            typeof(bool),
                            _boolTypeMapping);
                    }

                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                    {
                        left = ApplyTypeMapping(left, _boolTypeMapping);
                        right = ApplyTypeMapping(right, _boolTypeMapping);

                        return new SqlBinaryExpression(
                            sqlBinaryExpression.OperatorType,
                            left,
                            right,
                            typeof(bool),
                            _boolTypeMapping);
                    }

                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.Coalesce:
                    {
                        var inferredTypeMapping = typeMapping ?? InferTypeMappingForBinary(left, right);

                        left = ApplyTypeMapping(left, inferredTypeMapping);
                        right = ApplyTypeMapping(right, inferredTypeMapping);

                        return new SqlBinaryExpression(
                            sqlBinaryExpression.OperatorType,
                            left,
                            right,
                            sqlBinaryExpression.Type,
                            inferredTypeMapping);
                    }

                case ExpressionType.And:
                case ExpressionType.Or:
                    return null;
            }

            return null;
        }

        private RelationalTypeMapping InferTypeMappingForBinary(SqlExpression left, SqlExpression right)
        {
            var typeMapping = ExpressionExtensions.InferTypeMapping(left, right);

            if (typeMapping == null)
            {
                if (left is SqlCastExpression)
                {
                    typeMapping = _typeMappingSource.FindMapping(left.Type);
                }
                else if (right is SqlCastExpression)
                {
                    typeMapping = _typeMappingSource.FindMapping(right.Type);
                }
                else
                {
                    throw new InvalidOperationException("TypeMapping should not be null.");
                }
            }

            return typeMapping;
        }

        protected virtual SqlExpression ApplyTypeMappingOnSqlFunction(
            SqlFunctionExpression sqlFunctionExpression, RelationalTypeMapping typeMapping)
        {
            return sqlFunctionExpression;
        }

        protected virtual SqlExpression ApplyTypeMappingOnLike(
            LikeExpression likeExpression, RelationalTypeMapping typeMapping)
        {
            var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(likeExpression.Match, likeExpression.Pattern);

            if (inferredTypeMapping == null)
            {
                throw new InvalidOperationException("TypeMapping should not be null.");
            }

            var match = ApplyTypeMapping(likeExpression.Match, inferredTypeMapping);
            var pattern = ApplyTypeMapping(likeExpression.Pattern, inferredTypeMapping);
            var escapeChar = ApplyTypeMapping(likeExpression.EscapeChar, inferredTypeMapping);

            return new LikeExpression(
                match,
                pattern,
                escapeChar,
                _boolTypeMapping);
        }

        protected virtual SqlExpression ApplyTypeMappingOnSqlParameter(
            SqlParameterExpression sqlParameterExpression, RelationalTypeMapping typeMapping)
        {
            if (typeMapping == null)
            {
                throw new InvalidOperationException("TypeMapping should not be null.");
            }

            return sqlParameterExpression.ApplyTypeMapping(typeMapping);
        }

        protected virtual SqlExpression ApplyTypeMappingOnSqlConstant(
            SqlConstantExpression sqlConstantExpression, RelationalTypeMapping typeMapping)
        {
            if (typeMapping == null)
            {
                throw new InvalidOperationException("TypeMapping should not be null.");
            }

            return sqlConstantExpression.ApplyTypeMapping(typeMapping);
        }

        protected virtual SqlExpression ApplyTypeMappingOnExtension(
            SqlExpression expression, RelationalTypeMapping typeMapping)
        {
            return expression;
        }
    }
}
