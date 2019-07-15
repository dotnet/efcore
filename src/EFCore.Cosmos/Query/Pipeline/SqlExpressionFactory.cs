// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Pipeline;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Pipeline
{
    public class SqlExpressionFactory : ISqlExpressionFactory
    {
        private readonly ITypeMappingSource _typeMappingSource;
        private readonly CoreTypeMapping _boolTypeMapping;

        public SqlExpressionFactory(ITypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
            _boolTypeMapping = typeMappingSource.FindMapping(typeof(bool));
        }

        public SqlExpression ApplyDefaultTypeMapping(SqlExpression sqlExpression)
        {
            if (sqlExpression == null
                || sqlExpression.TypeMapping != null)
            {
                return sqlExpression;
            }

            return ApplyTypeMapping(sqlExpression, _typeMappingSource.FindMapping(sqlExpression.Type));
        }

        public SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, CoreTypeMapping typeMapping)
        {
            if (sqlExpression == null
                || sqlExpression.TypeMapping != null)
            {
                return sqlExpression;
            }

            switch (sqlExpression)
            {
                case SqlConditionalExpression sqlConditionalExpression:
                    return ApplyTypeMappingOnSqlConditional(sqlConditionalExpression, typeMapping);

                case SqlBinaryExpression sqlBinaryExpression:
                    return ApplyTypeMappingOnSqlBinary(sqlBinaryExpression, typeMapping);

                case SqlUnaryExpression sqlUnaryExpression:
                    return ApplyTypeMappingOnSqlUnary(sqlUnaryExpression, typeMapping);

                case SqlConstantExpression sqlConstantExpression:
                    return sqlConstantExpression.ApplyTypeMapping(typeMapping);

                case SqlParameterExpression sqlParameterExpression:
                    return sqlParameterExpression.ApplyTypeMapping(typeMapping);

                default:
                    return sqlExpression;
            }
        }

        private SqlExpression ApplyTypeMappingOnSqlConditional(
            SqlConditionalExpression sqlConditionalExpression, CoreTypeMapping typeMapping)
        {
            return sqlConditionalExpression.Update(
                sqlConditionalExpression.Test,
                ApplyTypeMapping(sqlConditionalExpression.IfTrue, typeMapping),
                ApplyTypeMapping(sqlConditionalExpression.IfFalse, typeMapping));
        }

        private SqlExpression ApplyTypeMappingOnSqlUnary(
            SqlUnaryExpression sqlUnaryExpression, CoreTypeMapping typeMapping)
        {
            SqlExpression operand;
            CoreTypeMapping resultTypeMapping;
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
            SqlBinaryExpression sqlBinaryExpression, CoreTypeMapping typeMapping)
        {
            var left = sqlBinaryExpression.Left;
            var right = sqlBinaryExpression.Right;

            Type resultType;
            CoreTypeMapping resultTypeMapping;
            CoreTypeMapping inferredTypeMapping;
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
                case ExpressionType.LeftShift:
                case ExpressionType.RightShift:
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

        public virtual CoreTypeMapping FindMapping(Type type)
        {
            return _typeMappingSource.FindMapping(type);
        }

        public SqlBinaryExpression MakeBinary(
            ExpressionType operatorType, SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping)
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

        public SqlBinaryExpression Add(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Add, left, right, typeMapping);
        }

        public SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Subtract, left, right, typeMapping);
        }

        public SqlBinaryExpression Multiply(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Multiply, left, right, typeMapping);
        }

        public SqlBinaryExpression Divide(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Divide, left, right, typeMapping);
        }

        public SqlBinaryExpression Modulo(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Modulo, left, right, typeMapping);
        }

        public SqlBinaryExpression And(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.And, left, right, typeMapping);
        }

        public SqlBinaryExpression Or(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Or, left, right, typeMapping);
        }

        public SqlBinaryExpression Coalesce(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
        {
            return MakeBinary(ExpressionType.Coalesce, left, right, typeMapping);
        }


        private SqlUnaryExpression MakeUnary(
            ExpressionType operatorType, SqlExpression operand, Type type, CoreTypeMapping typeMapping = null)
        {
            return (SqlUnaryExpression)ApplyTypeMapping(new SqlUnaryExpression(operatorType, operand, type, null), typeMapping);
        }

        public SqlBinaryExpression IsNull(SqlExpression operand)
        {
            return Equal(operand, Constant(null));
        }

        public SqlBinaryExpression IsNotNull(SqlExpression operand)
        {
            return NotEqual(operand, Constant(null));
        }

        public SqlUnaryExpression Convert(SqlExpression operand, Type type, CoreTypeMapping typeMapping = null)
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

        public SqlFunctionExpression Function(
            string functionName, IEnumerable<SqlExpression> arguments, Type returnType, CoreTypeMapping typeMapping = null)
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

        public SqlConditionalExpression Condition(SqlExpression test, SqlExpression ifTrue, SqlExpression ifFalse)
        {
            var typeMapping = ExpressionExtensions.InferTypeMapping(ifTrue, ifFalse);

            return new SqlConditionalExpression(
                ApplyTypeMapping(test, _boolTypeMapping),
                ApplyTypeMapping(ifTrue, typeMapping),
                ApplyTypeMapping(ifFalse, typeMapping));
        }
        public InExpression In(SqlExpression item, SqlExpression values, bool negated)
        {
            var typeMapping = item.TypeMapping ?? _typeMappingSource.FindMapping(item.Type);

            item = ApplyTypeMapping(item, typeMapping);
            values = ApplyTypeMapping(values, typeMapping);

            return new InExpression(item, negated, values, _boolTypeMapping);
        }

        public SqlConstantExpression Constant(object value, CoreTypeMapping typeMapping = null)
        {
            return new SqlConstantExpression(Expression.Constant(value), typeMapping);
        }

        public SelectExpression Select(IEntityType entityType)
        {
            var selectExpression = new SelectExpression(entityType);
            AddDiscriminator(selectExpression, entityType);

            return selectExpression;
        }

        private void AddDiscriminator(SelectExpression selectExpression, IEntityType entityType)
        {
            var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();

            if (concreteEntityTypes.Count == 1)
            {
                var concreteEntityType = concreteEntityTypes[0];
                if (concreteEntityType.GetDiscriminatorProperty() != null)
                {
                    var discriminatorColumn = ((EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember()))
                        .BindProperty(concreteEntityType.GetDiscriminatorProperty());

                    selectExpression.ApplyPredicate(
                        Equal(discriminatorColumn, Constant(concreteEntityType.GetDiscriminatorValue())));
                }
            }
            else
            {
                var discriminatorColumn = ((EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember()))
                    .BindProperty(concreteEntityTypes[0].GetDiscriminatorProperty());

                selectExpression.ApplyPredicate(
                    In(discriminatorColumn, Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()), negated: false));
            }
        }
    }
}
