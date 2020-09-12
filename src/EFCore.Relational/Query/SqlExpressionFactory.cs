// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <inheritdoc />
    public class SqlExpressionFactory : ISqlExpressionFactory
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly RelationalTypeMapping _boolTypeMapping;

        /// <summary>
        ///     Creates a new instance of the <see cref="SqlExpressionFactory" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this class. </param>
        public SqlExpressionFactory([NotNull] SqlExpressionFactoryDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            _typeMappingSource = dependencies.TypeMappingSource;
            _boolTypeMapping = _typeMappingSource.FindMapping(typeof(bool));
        }

        /// <inheritdoc />
        public virtual SqlExpression ApplyDefaultTypeMapping(SqlExpression sqlExpression)
        {
            return sqlExpression == null
                || sqlExpression.TypeMapping != null
                    ? sqlExpression
                    : sqlExpression is SqlUnaryExpression sqlUnaryExpression
                    && sqlUnaryExpression.OperatorType == ExpressionType.Convert
                    && sqlUnaryExpression.Type == typeof(object)
                        ? sqlUnaryExpression.Operand
                        : ApplyTypeMapping(sqlExpression, _typeMappingSource.FindMapping(sqlExpression.Type));
        }

        /// <inheritdoc />
        public virtual SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, RelationalTypeMapping typeMapping)
        {
#pragma warning disable IDE0046 // Convert to conditional expression
            if (sqlExpression == null
#pragma warning restore IDE0046 // Convert to conditional expression
                || sqlExpression.TypeMapping != null)
            {
                return sqlExpression;
            }

            return sqlExpression switch
            {
                CaseExpression e => ApplyTypeMappingOnCase(e, typeMapping),
                CollateExpression e => ApplyTypeMappingOnCollate(e, typeMapping),
                DistinctExpression e => ApplyTypeMappingOnDistinct(e, typeMapping),
                LikeExpression e => ApplyTypeMappingOnLike(e),
                SqlBinaryExpression e => ApplyTypeMappingOnSqlBinary(e, typeMapping),
                SqlUnaryExpression e => ApplyTypeMappingOnSqlUnary(e, typeMapping),
                SqlConstantExpression e => e.ApplyTypeMapping(typeMapping),
                SqlFragmentExpression e => e,
                SqlFunctionExpression e => e.ApplyTypeMapping(typeMapping),
                SqlParameterExpression e => e.ApplyTypeMapping(typeMapping),
                _ => sqlExpression
            };
        }

        private SqlExpression ApplyTypeMappingOnLike(LikeExpression likeExpression)
        {
            var inferredTypeMapping = (likeExpression.EscapeChar == null
                    ? ExpressionExtensions.InferTypeMapping(
                        likeExpression.Match, likeExpression.Pattern)
                    : ExpressionExtensions.InferTypeMapping(
                        likeExpression.Match, likeExpression.Pattern, likeExpression.EscapeChar))
                ?? _typeMappingSource.FindMapping(likeExpression.Match.Type);

            return new LikeExpression(
                ApplyTypeMapping(likeExpression.Match, inferredTypeMapping),
                ApplyTypeMapping(likeExpression.Pattern, inferredTypeMapping),
                ApplyTypeMapping(likeExpression.EscapeChar, inferredTypeMapping),
                _boolTypeMapping);
        }

        private SqlExpression ApplyTypeMappingOnCase(
            CaseExpression caseExpression,
            RelationalTypeMapping typeMapping)
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

        private SqlExpression ApplyTypeMappingOnCollate(
            CollateExpression collateExpression,
            RelationalTypeMapping typeMapping)
            => collateExpression.Update(ApplyTypeMapping(collateExpression.Operand, typeMapping));

        private SqlExpression ApplyTypeMappingOnDistinct(
            DistinctExpression distinctExpression,
            RelationalTypeMapping typeMapping)
            => distinctExpression.Update(ApplyTypeMapping(distinctExpression.Operand, typeMapping));

        private SqlExpression ApplyTypeMappingOnSqlUnary(
            SqlUnaryExpression sqlUnaryExpression,
            RelationalTypeMapping typeMapping)
        {
            SqlExpression operand;
            Type resultType;
            RelationalTypeMapping resultTypeMapping;
            switch (sqlUnaryExpression.OperatorType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.Not
                    when sqlUnaryExpression.Type == typeof(bool):
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
                        RelationalStrings.UnsupportedOperatorForSqlExpression(
                            sqlUnaryExpression.OperatorType, typeof(SqlUnaryExpression).ShortDisplayName()));
            }

            return new SqlUnaryExpression(sqlUnaryExpression.OperatorType, operand, resultType, resultTypeMapping);
        }

        private SqlExpression ApplyTypeMappingOnSqlBinary(
            SqlBinaryExpression sqlBinaryExpression,
            RelationalTypeMapping typeMapping)
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
                        // We avoid object here since the result does not get typeMapping from outside.
                        ?? (left.Type != typeof(object)
                            ? _typeMappingSource.FindMapping(left.Type)
                            : _typeMappingSource.FindMapping(right.Type));
                    resultType = typeof(bool);
                    resultTypeMapping = _boolTypeMapping;
                    break;
                }

                case ExpressionType.AndAlso:
                case ExpressionType.OrElse:
                {
                    inferredTypeMapping = _boolTypeMapping;
                    resultType = typeof(bool);
                    resultTypeMapping = _boolTypeMapping;
                    break;
                }

                case ExpressionType.Add:
                case ExpressionType.Subtract:
                case ExpressionType.Multiply:
                case ExpressionType.Divide:
                case ExpressionType.Modulo:
                case ExpressionType.And:
                case ExpressionType.Or:
                {
                    inferredTypeMapping = typeMapping ?? ExpressionExtensions.InferTypeMapping(left, right);
                    resultType = inferredTypeMapping?.ClrType ?? left.Type;
                    resultTypeMapping = inferredTypeMapping;
                    break;
                }

                default:
                    throw new InvalidOperationException(
                        RelationalStrings.UnsupportedOperatorForSqlExpression(
                            sqlBinaryExpression.OperatorType, typeof(SqlBinaryExpression).ShortDisplayName()));
            }

            return new SqlBinaryExpression(
                sqlBinaryExpression.OperatorType,
                ApplyTypeMapping(left, inferredTypeMapping),
                ApplyTypeMapping(right, inferredTypeMapping),
                resultType,
                resultTypeMapping);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression MakeBinary(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping typeMapping)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            if (!SqlBinaryExpression.IsValidOperator(operatorType))
            {
                return null;
            }

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

        /// <inheritdoc />
        public virtual SqlBinaryExpression Equal(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Equal, left, right, null);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.NotEqual, left, right, null);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.GreaterThan, left, right, null);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.GreaterThanOrEqual, left, right, null);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression LessThan(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.LessThan, left, right, null);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression LessThanOrEqual(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.LessThanOrEqual, left, right, null);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression AndAlso(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.AndAlso, left, right, null);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression OrElse(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.OrElse, left, right, null);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression Add(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Add, left, right, typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Subtract, left, right, typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression Multiply(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Multiply, left, right, typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression Divide(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Divide, left, right, typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression Modulo(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Modulo, left, right, typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression And(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.And, left, right, typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlBinaryExpression Or(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Or, left, right, typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlFunctionExpression Coalesce(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            var resultType = right.Type;
            var inferredTypeMapping = typeMapping
                ?? ExpressionExtensions.InferTypeMapping(left, right)
                ?? _typeMappingSource.FindMapping(resultType);

            var typeMappedArguments = new List<SqlExpression>
            {
                ApplyTypeMapping(left, inferredTypeMapping), ApplyTypeMapping(right, inferredTypeMapping)
            };

            return new SqlFunctionExpression(
                "COALESCE",
                typeMappedArguments,
                nullable: true,
                // COALESCE is handled separately since it's only nullable if *both* arguments are null
                argumentsPropagateNullability: new[] { false, false },
                resultType,
                inferredTypeMapping);
        }

        /// <inheritdoc />
        public virtual SqlUnaryExpression MakeUnary(
            ExpressionType operatorType,
            SqlExpression operand,
            Type type,
            RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(operatorType, nameof(operand));
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(type, nameof(type));

            return !SqlUnaryExpression.IsValidOperator(operatorType)
                ? null
                : (SqlUnaryExpression)ApplyTypeMapping(new SqlUnaryExpression(operatorType, operand, type, null), typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlUnaryExpression IsNull(SqlExpression operand)
        {
            Check.NotNull(operand, nameof(operand));

            return MakeUnary(ExpressionType.Equal, operand, typeof(bool));
        }

        /// <inheritdoc />
        public virtual SqlUnaryExpression IsNotNull(SqlExpression operand)
        {
            Check.NotNull(operand, nameof(operand));

            return MakeUnary(ExpressionType.NotEqual, operand, typeof(bool));
        }

        /// <inheritdoc />
        public virtual SqlUnaryExpression Convert(SqlExpression operand, Type type, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(type, nameof(type));

            return MakeUnary(ExpressionType.Convert, operand, type.UnwrapNullableType(), typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlUnaryExpression Not(SqlExpression operand)
        {
            Check.NotNull(operand, nameof(operand));

            return MakeUnary(ExpressionType.Not, operand, operand.Type, operand.TypeMapping);
        }

        /// <inheritdoc />
        public virtual SqlUnaryExpression Negate(SqlExpression operand)
        {
            Check.NotNull(operand, nameof(operand));

            return MakeUnary(ExpressionType.Negate, operand, operand.Type, operand.TypeMapping);
        }

        /// <inheritdoc />
        [Obsolete("Use overload which takes IReadOnlyList instead of params")]
        public virtual CaseExpression Case(SqlExpression operand, params CaseWhenClause[] whenClauses)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(whenClauses, nameof(whenClauses));

            return Case(operand, whenClauses, null);
        }

        /// <inheritdoc />
        public virtual CaseExpression Case(SqlExpression operand, IReadOnlyList<CaseWhenClause> whenClauses, SqlExpression elseResult)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(whenClauses, nameof(whenClauses));

            var operandTypeMapping = operand.TypeMapping
                ?? whenClauses.Select(wc => wc.Test.TypeMapping).FirstOrDefault(t => t != null)
                // Since we never look at type of Operand/Test after this place,
                // we need to find actual typeMapping based on non-object type.
                ?? new[] { operand.Type }.Concat(whenClauses.Select(wc => wc.Test.Type))
                    .Where(t => t != typeof(object)).Select(t => _typeMappingSource.FindMapping(t)).FirstOrDefault();

            var resultTypeMapping = elseResult?.TypeMapping
                ?? whenClauses.Select(wc => wc.Result.TypeMapping).FirstOrDefault(t => t != null);

            operand = ApplyTypeMapping(operand, operandTypeMapping);
            elseResult = ApplyTypeMapping(elseResult, resultTypeMapping);

            var typeMappedWhenClauses = new List<CaseWhenClause>();
            foreach (var caseWhenClause in whenClauses)
            {
                typeMappedWhenClauses.Add(
                    new CaseWhenClause(
                        ApplyTypeMapping(caseWhenClause.Test, operandTypeMapping),
                        ApplyTypeMapping(caseWhenClause.Result, resultTypeMapping)));
            }

            return new CaseExpression(operand, typeMappedWhenClauses, elseResult);
        }

        /// <inheritdoc />
        public virtual CaseExpression Case(IReadOnlyList<CaseWhenClause> whenClauses, SqlExpression elseResult)
        {
            Check.NotNull(whenClauses, nameof(whenClauses));

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

        /// <inheritdoc />
        [Obsolete("Use overload that explicitly specifies value for 'argumentsPropagateNullability' argument.")]
        public virtual SqlFunctionExpression Function(
            string name,
            IEnumerable<SqlExpression> arguments,
            Type returnType,
            RelationalTypeMapping typeMapping = null)
            => Function(
                name, arguments, nullable: true, argumentsPropagateNullability: arguments.Select(a => false), returnType, typeMapping);

        /// <inheritdoc />
        [Obsolete("Use overload that explicitly specifies value for 'argumentsPropagateNullability' argument.")]
        public virtual SqlFunctionExpression Function(
            string schema,
            string name,
            IEnumerable<SqlExpression> arguments,
            Type returnType,
            RelationalTypeMapping typeMapping = null)
            => Function(
                schema, name, arguments, nullable: true, argumentsPropagateNullability: arguments.Select(a => false), returnType,
                typeMapping);

        /// <inheritdoc />
        [Obsolete(
            "Use overload that explicitly specifies values for 'instancePropagatesNullability' and 'argumentsPropagateNullability' arguments.")]
        public virtual SqlFunctionExpression Function(
            SqlExpression instance,
            string name,
            IEnumerable<SqlExpression> arguments,
            Type returnType,
            RelationalTypeMapping typeMapping = null)
            => Function(
                instance,
                name,
                arguments,
                nullable: true,
                instancePropagatesNullability: false,
                argumentsPropagateNullability: arguments.Select(a => false),
                returnType,
                typeMapping);

        /// <inheritdoc />
        [Obsolete("Use NiladicFunction method.")]
        public virtual SqlFunctionExpression Function(string name, Type returnType, RelationalTypeMapping typeMapping = null)
            => NiladicFunction(name, nullable: true, returnType, typeMapping);

        /// <inheritdoc />
        [Obsolete("Use NiladicFunction method.")]
        public virtual SqlFunctionExpression Function(string schema, string name, Type returnType, RelationalTypeMapping typeMapping = null)
            => NiladicFunction(schema, name, nullable: true, returnType, typeMapping);

        /// <inheritdoc />
        [Obsolete("Use NiladicFunction method.")]
        public virtual SqlFunctionExpression Function(
            SqlExpression instance,
            string name,
            Type returnType,
            RelationalTypeMapping typeMapping = null)
            => NiladicFunction(instance, name, nullable: true, instancePropagatesNullability: false, returnType, typeMapping);

        /// <inheritdoc />
        public virtual SqlFunctionExpression Function(
            string name,
            IEnumerable<SqlExpression> arguments,
            bool nullable,
            IEnumerable<bool> argumentsPropagateNullability,
            Type returnType,
            RelationalTypeMapping typeMapping = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(argumentsPropagateNullability, nameof(argumentsPropagateNullability));
            Check.NotNull(returnType, nameof(returnType));

            var typeMappedArguments = new List<SqlExpression>();

            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
            }

            return new SqlFunctionExpression(name, typeMappedArguments, nullable, argumentsPropagateNullability, returnType, typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlFunctionExpression Function(
            string schema,
            string name,
            IEnumerable<SqlExpression> arguments,
            bool nullable,
            IEnumerable<bool> argumentsPropagateNullability,
            Type returnType,
            RelationalTypeMapping typeMapping = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(argumentsPropagateNullability, nameof(argumentsPropagateNullability));
            Check.NotNull(returnType, nameof(returnType));

            var typeMappedArguments = new List<SqlExpression>();
            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
            }

            return new SqlFunctionExpression(
                schema, name, typeMappedArguments, nullable, argumentsPropagateNullability, returnType, typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlFunctionExpression Function(
            SqlExpression instance,
            string name,
            IEnumerable<SqlExpression> arguments,
            bool nullable,
            bool instancePropagatesNullability,
            IEnumerable<bool> argumentsPropagateNullability,
            Type returnType,
            RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(instance, nameof(instance));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(argumentsPropagateNullability, nameof(argumentsPropagateNullability));
            Check.NotNull(returnType, nameof(returnType));

            instance = ApplyDefaultTypeMapping(instance);
            var typeMappedArguments = new List<SqlExpression>();
            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
            }

            return new SqlFunctionExpression(
                instance, name, typeMappedArguments, nullable, instancePropagatesNullability, argumentsPropagateNullability, returnType,
                typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlFunctionExpression NiladicFunction(
            string name,
            bool nullable,
            Type returnType,
            RelationalTypeMapping typeMapping = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(returnType, nameof(returnType));

            return new SqlFunctionExpression(name, nullable, returnType, typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlFunctionExpression NiladicFunction(
            string schema,
            string name,
            bool nullable,
            Type returnType,
            RelationalTypeMapping typeMapping = null)
        {
            Check.NotEmpty(schema, nameof(schema));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(returnType, nameof(returnType));

            return new SqlFunctionExpression(schema, name, nullable, returnType, typeMapping);
        }

        /// <inheritdoc />
        public virtual SqlFunctionExpression NiladicFunction(
            SqlExpression instance,
            string name,
            bool nullable,
            bool instancePropagatesNullability,
            Type returnType,
            RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(instance, nameof(instance));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(returnType, nameof(returnType));

            return new SqlFunctionExpression(
                ApplyDefaultTypeMapping(instance), name, nullable, instancePropagatesNullability, returnType, typeMapping);
        }

        /// <inheritdoc />
        public virtual ExistsExpression Exists(SelectExpression subquery, bool negated)
        {
            Check.NotNull(subquery, nameof(subquery));

            return new ExistsExpression(subquery, negated, _boolTypeMapping);
        }

        /// <inheritdoc />
        public virtual InExpression In(SqlExpression item, SqlExpression values, bool negated)
        {
            Check.NotNull(item, nameof(item));
            Check.NotNull(values, nameof(values));

            var typeMapping = item.TypeMapping ?? _typeMappingSource.FindMapping(item.Type);

            item = ApplyTypeMapping(item, typeMapping);
            values = ApplyTypeMapping(values, typeMapping);

            return new InExpression(item, values, negated, _boolTypeMapping);
        }

        /// <inheritdoc />
        public virtual InExpression In(SqlExpression item, SelectExpression subquery, bool negated)
        {
            Check.NotNull(item, nameof(item));
            Check.NotNull(subquery, nameof(subquery));

            var sqlExpression = subquery.Projection.Single().Expression;
            var typeMapping = sqlExpression.TypeMapping;

            item = ApplyTypeMapping(item, typeMapping);
            return new InExpression(item, subquery, negated, _boolTypeMapping);
        }

        /// <inheritdoc />
        public virtual LikeExpression Like(SqlExpression match, SqlExpression pattern, SqlExpression escapeChar = null)
        {
            Check.NotNull(match, nameof(match));
            Check.NotNull(pattern, nameof(pattern));

            return (LikeExpression)ApplyDefaultTypeMapping(new LikeExpression(match, pattern, escapeChar, null));
        }

        /// <inheritdoc />
        public virtual SqlFragmentExpression Fragment(string sql)
        {
            Check.NotNull(sql, nameof(sql));

            return new SqlFragmentExpression(sql);
        }

        /// <inheritdoc />
        public virtual SqlConstantExpression Constant(object value, RelationalTypeMapping typeMapping = null)
            => new SqlConstantExpression(Expression.Constant(value), typeMapping);

        /// <inheritdoc />
        public virtual SelectExpression Select(SqlExpression projection)
            => new SelectExpression(projection);

        /// <inheritdoc />
        public virtual SelectExpression Select(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var selectExpression = new SelectExpression(entityType, this);
            AddConditions(selectExpression, entityType);

            return selectExpression;
        }

        /// <inheritdoc />
        public virtual SelectExpression Select(IEntityType entityType, TableExpressionBase tableExpressionBase)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(tableExpressionBase, nameof(tableExpressionBase));

            var selectExpression = new SelectExpression(entityType, tableExpressionBase);
            AddConditions(selectExpression, entityType);

            return selectExpression;
        }

        /// <inheritdoc />
        [Obsolete("Use overload which takes TableExpressionBase by passing FromSqlExpression directly.")]
        public virtual SelectExpression Select(IEntityType entityType, string sql, Expression sqlArguments)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(sql, nameof(sql));

            var tableExpression = new FromSqlExpression(
                entityType.GetDefaultMappings().SingleOrDefault().Table.Name.Substring(0, 1).ToLower(), sql, sqlArguments);
            var selectExpression = new SelectExpression(entityType, tableExpression);
            AddConditions(selectExpression, entityType);

            return selectExpression;
        }

        private void AddSelfConditions(SelectExpression selectExpression, IEntityType entityType, ITableBase table = null)
        {
            // Add conditions if TPH
            var discriminatorAdded = AddDiscriminatorCondition(selectExpression, entityType);
            if (entityType.FindPrimaryKey() == null)
            {
                return;
            }

            // Add conditions if dependent sharing table with principal
            table ??= entityType.GetViewOrTableMappings().FirstOrDefault()?.Table;
            if (table != null
                && table.IsOptional(entityType)
                && !discriminatorAdded)
            {
                AddOptionalDependentConditions(selectExpression, entityType, table);
            }
        }

        private void AddConditions(SelectExpression selectExpression, IEntityType entityType, ITableBase table = null)
        {
            AddSelfConditions(selectExpression, entityType, table);
            // Add inner join to principal if table sharing
            table ??= entityType.GetViewOrTableMappings().FirstOrDefault()?.Table;
            if (table != null)
            {
                var linkingFks = table.GetRowInternalForeignKeys(entityType);
                var first = true;
                foreach (var foreignKey in linkingFks)
                {
                    if (first)
                    {
                        AddInnerJoin(selectExpression, foreignKey, table);
                        first = false;
                    }
                    else
                    {
                        var dependentSelectExpression = new SelectExpression(entityType, this);
                        AddSelfConditions(dependentSelectExpression, entityType, table);
                        AddInnerJoin(dependentSelectExpression, foreignKey, table);
                        selectExpression.ApplyUnion(dependentSelectExpression, distinct: true);
                    }
                }
            }
        }

        private void AddInnerJoin(SelectExpression selectExpression, IForeignKey foreignKey, ITableBase table)
        {
            var outerEntityProjection = GetMappedEntityProjectionExpression(selectExpression);
            var outerIsPrincipal = foreignKey.PrincipalEntityType.IsAssignableFrom(outerEntityProjection.EntityType);

            var innerSelect = outerIsPrincipal
                ? new SelectExpression(foreignKey.DeclaringEntityType, this)
                : new SelectExpression(foreignKey.PrincipalEntityType, this);

            if (outerIsPrincipal)
            {
                AddSelfConditions(innerSelect, foreignKey.DeclaringEntityType, table);
            }
            else
            {
                AddConditions(innerSelect, foreignKey.PrincipalEntityType, table);
            }

            var innerEntityProjection = GetMappedEntityProjectionExpression(innerSelect);

            var outerKey = (outerIsPrincipal ? foreignKey.PrincipalKey.Properties : foreignKey.Properties)
                .Select(p => outerEntityProjection.BindProperty(p));
            var innerKey = (outerIsPrincipal ? foreignKey.Properties : foreignKey.PrincipalKey.Properties)
                .Select(p => innerEntityProjection.BindProperty(p));

            var joinPredicate = outerKey.Zip(innerKey, Equal).Aggregate(AndAlso);

            selectExpression.AddInnerJoin(innerSelect, joinPredicate);
        }

        private bool AddDiscriminatorCondition(SelectExpression selectExpression, IEntityType entityType)
        {
            var discriminatorProperty = entityType.GetDiscriminatorProperty();
            if (discriminatorProperty == null
                || (entityType.GetRootType().GetIsDiscriminatorMappingComplete()
                    && entityType.GetAllBaseTypesInclusiveAscending()
                        .All(e => (e == entityType || e.IsAbstract()) && !HasSiblings(e))))
            {
                return false;
            }

            var discriminatorColumn = GetMappedEntityProjectionExpression(selectExpression).BindProperty(discriminatorProperty);
            var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();
            var predicate = concreteEntityTypes.Count == 1
                ? (SqlExpression)Equal(discriminatorColumn, Constant(concreteEntityTypes[0].GetDiscriminatorValue()))
                : In(discriminatorColumn, Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()), negated: false);

            selectExpression.ApplyPredicate(predicate);

            return true;

            bool HasSiblings(IEntityType entityType)
            {
                return entityType.BaseType?.GetDirectlyDerivedTypes().Any(i => i != entityType) == true;
            }
        }

        private void AddOptionalDependentConditions(
            SelectExpression selectExpression,
            IEntityType entityType,
            ITableBase table)
        {
            SqlExpression predicate = null;
            var requiredNonPkProperties = entityType.GetProperties().Where(p => !p.IsNullable && !p.IsPrimaryKey()).ToList();
            if (requiredNonPkProperties.Count > 0)
            {
                var entityProjectionExpression = GetMappedEntityProjectionExpression(selectExpression);
                predicate = IsNotNull(requiredNonPkProperties[0], entityProjectionExpression);

                if (requiredNonPkProperties.Count > 1)
                {
                    predicate
                        = requiredNonPkProperties
                            .Skip(1)
                            .Aggregate(
                                predicate, (current, property) =>
                                    AndAlso(
                                        IsNotNull(property, entityProjectionExpression),
                                        current));
                }

                selectExpression.ApplyPredicate(predicate);
            }
            else
            {
                var allNonPkProperties = entityType.GetProperties().Where(p => !p.IsPrimaryKey()).ToList();
                if (allNonPkProperties.Count > 0)
                {
                    var entityProjectionExpression = GetMappedEntityProjectionExpression(selectExpression);
                    predicate = IsNotNull(allNonPkProperties[0], entityProjectionExpression);

                    if (allNonPkProperties.Count > 1)
                    {
                        predicate
                            = allNonPkProperties
                                .Skip(1)
                                .Aggregate(
                                    predicate, (current, property) =>
                                        OrElse(
                                            IsNotNull(property, entityProjectionExpression),
                                            current));
                    }

                    selectExpression.ApplyPredicate(predicate);

                    // If there is no non-nullable property then we also need to add optional dependents which are acting as principal for
                    // other dependents.
                    foreach (var referencingFk in entityType.GetReferencingForeignKeys())
                    {
                        if (referencingFk.PrincipalEntityType.IsAssignableFrom(entityType))
                        {
                            continue;
                        }

                        var otherSelectExpression = new SelectExpression(entityType, this);

                        var sameTable = table.EntityTypeMappings.Any(m => m.EntityType == referencingFk.DeclaringEntityType)
                            && table.IsOptional(referencingFk.DeclaringEntityType);
                        AddInnerJoin(
                            otherSelectExpression, referencingFk,
                            sameTable ? table : null);

                        selectExpression.ApplyUnion(otherSelectExpression, distinct: true);
                    }
                }
            }
        }

        private EntityProjectionExpression GetMappedEntityProjectionExpression(SelectExpression selectExpression)
            => (EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember());

        private SqlExpression IsNotNull(IProperty property, EntityProjectionExpression entityProjection)
            => IsNotNull(entityProjection.BindProperty(property));

        /// <inheritdoc />
        [Obsolete("Use IRelationalTypeMappingSource directly.")]
        public virtual RelationalTypeMapping GetTypeMappingForValue(object value)
            => _typeMappingSource.GetMappingForValue(value);

        /// <inheritdoc />
        [Obsolete("Use IRelationalTypeMappingSource directly.")]
        public virtual RelationalTypeMapping FindMapping(Type type)
            => _typeMappingSource.FindMapping(Check.NotNull(type, nameof(type)));
    }
}
