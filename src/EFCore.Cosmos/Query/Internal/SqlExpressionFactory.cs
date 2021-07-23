// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

#nullable disable

namespace Microsoft.EntityFrameworkCore.Cosmos.Query.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqlExpressionFactory : ISqlExpressionFactory
    {
        private readonly ITypeMappingSource _typeMappingSource;
        private readonly CoreTypeMapping _boolTypeMapping;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public SqlExpressionFactory(ITypeMappingSource typeMappingSource)
        {
            _typeMappingSource = typeMappingSource;
            _boolTypeMapping = typeMappingSource.FindMapping(typeof(bool));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression ApplyDefaultTypeMapping(SqlExpression sqlExpression)
        {
            return sqlExpression == null
                || sqlExpression.TypeMapping != null
                    ? sqlExpression
                    : ApplyTypeMapping(sqlExpression, _typeMappingSource.FindMapping(sqlExpression.Type));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, CoreTypeMapping typeMapping)
        {
            if (sqlExpression == null
                || sqlExpression.TypeMapping != null)
            {
                return sqlExpression;
            }

#pragma warning disable IDE0066 // Convert switch statement to expression
            switch (sqlExpression)
#pragma warning restore IDE0066 // Convert switch statement to expression
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

                case SqlFunctionExpression sqlFunctionExpression:
                    return sqlFunctionExpression.ApplyTypeMapping(typeMapping);

                default:
                    return sqlExpression;
            }
        }

        private SqlExpression ApplyTypeMappingOnSqlConditional(
            SqlConditionalExpression sqlConditionalExpression,
            CoreTypeMapping typeMapping)
        {
            return sqlConditionalExpression.Update(
                sqlConditionalExpression.Test,
                ApplyTypeMapping(sqlConditionalExpression.IfTrue, typeMapping),
                ApplyTypeMapping(sqlConditionalExpression.IfFalse, typeMapping));
        }

        private SqlExpression ApplyTypeMappingOnSqlUnary(
            SqlUnaryExpression sqlUnaryExpression,
            CoreTypeMapping typeMapping)
        {
            SqlExpression operand;
            Type resultType;
            CoreTypeMapping resultTypeMapping;
            switch (sqlUnaryExpression.OperatorType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Not
                    when sqlUnaryExpression.IsLogicalNot():
                {
                    resultTypeMapping = _boolTypeMapping;
                    resultType = typeof(bool);
                    operand = ApplyDefaultTypeMapping(sqlUnaryExpression.Operand);
                    break;
                }

                case ExpressionType.Convert:
                    resultTypeMapping = typeMapping;
                    // Since we are applying convert, resultTypeMapping decides the clrType
                    resultType = resultTypeMapping?.ClrType ?? sqlUnaryExpression.Type;
                    operand = ApplyDefaultTypeMapping(sqlUnaryExpression.Operand);
                    break;

                case ExpressionType.Not:
                case ExpressionType.Negate:
                    resultTypeMapping = typeMapping;
                    // While Not is logical, negate is numeric hence we use clrType from TypeMapping
                    resultType = resultTypeMapping?.ClrType ?? sqlUnaryExpression.Type;
                    operand = ApplyTypeMapping(sqlUnaryExpression.Operand, typeMapping);
                    break;

                default:
                    throw new InvalidOperationException(
                        CosmosStrings.UnsupportedOperatorForSqlExpression(
                            sqlUnaryExpression.OperatorType, typeof(SqlUnaryExpression).ShortDisplayName()));
            }

            return new SqlUnaryExpression(sqlUnaryExpression.OperatorType, operand, resultType, resultTypeMapping);
        }

        private SqlExpression ApplyTypeMappingOnSqlBinary(
            SqlBinaryExpression sqlBinaryExpression,
            CoreTypeMapping typeMapping)
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
                        // We avoid object here since the result does not get typeMapping from outside.
                        ?? (left.Type != typeof(object)
                            ? _typeMappingSource.FindMapping(left.Type)
                            : _typeMappingSource.FindMapping(right.Type));
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
                case ExpressionType.And:
                case ExpressionType.Or:
                {
                    inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                    resultType = inferredTypeMapping?.ClrType ?? left.Type;
                    resultTypeMapping = inferredTypeMapping;
                }
                    break;

                default:
                    throw new InvalidOperationException(
                        CosmosStrings.UnsupportedOperatorForSqlExpression(
                            sqlBinaryExpression.OperatorType, typeof(SqlBinaryExpression).ShortDisplayName()));
            }

            return new SqlBinaryExpression(
                sqlBinaryExpression.OperatorType,
                ApplyTypeMapping(left, inferredTypeMapping),
                ApplyTypeMapping(right, inferredTypeMapping),
                resultType,
                resultTypeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual CoreTypeMapping FindMapping(Type type)
            => _typeMappingSource.FindMapping(type);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression MakeBinary(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            CoreTypeMapping typeMapping)
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Equal(SqlExpression left, SqlExpression right)
            => MakeBinary(ExpressionType.Equal, left, right, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right)
            => MakeBinary(ExpressionType.NotEqual, left, right, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right)
            => MakeBinary(ExpressionType.GreaterThan, left, right, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right)
            => MakeBinary(ExpressionType.GreaterThanOrEqual, left, right, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression LessThan(SqlExpression left, SqlExpression right)
            => MakeBinary(ExpressionType.LessThan, left, right, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression LessThanOrEqual(SqlExpression left, SqlExpression right)
            => MakeBinary(ExpressionType.LessThanOrEqual, left, right, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression AndAlso(SqlExpression left, SqlExpression right)
            => MakeBinary(ExpressionType.AndAlso, left, right, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression OrElse(SqlExpression left, SqlExpression right)
            => MakeBinary(ExpressionType.OrElse, left, right, null);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Add(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
            => MakeBinary(ExpressionType.Add, left, right, typeMapping);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
            => MakeBinary(ExpressionType.Subtract, left, right, typeMapping);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Multiply(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
            => MakeBinary(ExpressionType.Multiply, left, right, typeMapping);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Divide(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
            => MakeBinary(ExpressionType.Divide, left, right, typeMapping);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Modulo(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
            => MakeBinary(ExpressionType.Modulo, left, right, typeMapping);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression And(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
            => MakeBinary(ExpressionType.And, left, right, typeMapping);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression Or(SqlExpression left, SqlExpression right, CoreTypeMapping typeMapping = null)
            => MakeBinary(ExpressionType.Or, left, right, typeMapping);

        private SqlUnaryExpression MakeUnary(
            ExpressionType operatorType,
            SqlExpression operand,
            Type type,
            CoreTypeMapping typeMapping = null)
        {
            return (SqlUnaryExpression)ApplyTypeMapping(new SqlUnaryExpression(operatorType, operand, type, null), typeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression IsNull(SqlExpression operand)
            => Equal(operand, Constant(null));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlBinaryExpression IsNotNull(SqlExpression operand)
            => NotEqual(operand, Constant(null));

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlUnaryExpression Convert(SqlExpression operand, Type type, CoreTypeMapping typeMapping = null)
            => MakeUnary(ExpressionType.Convert, operand, type, typeMapping);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlUnaryExpression Not(SqlExpression operand)
            => MakeUnary(ExpressionType.Not, operand, operand.Type, operand.TypeMapping);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlUnaryExpression Negate(SqlExpression operand)
            => MakeUnary(ExpressionType.Negate, operand, operand.Type, operand.TypeMapping);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlFunctionExpression Function(
            string functionName,
            IEnumerable<SqlExpression> arguments,
            Type returnType,
            CoreTypeMapping typeMapping = null)
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

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlConditionalExpression Condition(SqlExpression test, SqlExpression ifTrue, SqlExpression ifFalse)
        {
            var typeMapping = ExpressionExtensions.InferTypeMapping(ifTrue, ifFalse);

            return new SqlConditionalExpression(
                ApplyTypeMapping(test, _boolTypeMapping),
                ApplyTypeMapping(ifTrue, typeMapping),
                ApplyTypeMapping(ifFalse, typeMapping));
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual InExpression In(SqlExpression item, SqlExpression values, bool negated)
        {
            var typeMapping = item.TypeMapping ?? _typeMappingSource.FindMapping(item.Type);

            item = ApplyTypeMapping(item, typeMapping);
            values = ApplyTypeMapping(values, typeMapping);

            return new InExpression(item, negated, values, _boolTypeMapping);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SqlConstantExpression Constant(object value, CoreTypeMapping typeMapping = null)
            => new(Expression.Constant(value), typeMapping);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual SelectExpression Select(IEntityType entityType)
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
                var discriminatorProperty = concreteEntityType.FindDiscriminatorProperty();
                if (discriminatorProperty != null)
                {
                    var discriminatorColumn = ((EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember()))
                        .BindProperty(discriminatorProperty, clientEval: false);

                    selectExpression.ApplyPredicate(
                        Equal((SqlExpression)discriminatorColumn, Constant(concreteEntityType.GetDiscriminatorValue())));
                }
            }
            else
            {
                var discriminatorColumn = ((EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember()))
                    .BindProperty(concreteEntityTypes[0].FindDiscriminatorProperty(), clientEval: false);

                selectExpression.ApplyPredicate(
                    In(
                        (SqlExpression)discriminatorColumn, Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()),
                        negated: false));
            }
        }
    }
}
