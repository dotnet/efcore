// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public class SqlExpressionFactory : ISqlExpressionFactory
    {
        private readonly IRelationalTypeMappingSource _typeMappingSource;
        private readonly RelationalTypeMapping _boolTypeMapping;

        public SqlExpressionFactory([NotNull] SqlExpressionFactoryDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            _typeMappingSource = dependencies.TypeMappingSource;
            _boolTypeMapping = _typeMappingSource.FindMapping(typeof(bool));
        }

        public virtual SqlExpression ApplyDefaultTypeMapping(SqlExpression sqlExpression)
        {
            if (sqlExpression == null
                || sqlExpression.TypeMapping != null)
            {
                return sqlExpression;
            }

            if (sqlExpression is SqlUnaryExpression sqlUnaryExpression
                && sqlUnaryExpression.OperatorType == ExpressionType.Convert
                && sqlUnaryExpression.Type == typeof(object))
            {
                return sqlUnaryExpression.Operand;
            }

            return ApplyTypeMapping(sqlExpression, _typeMappingSource.FindMapping(sqlExpression.Type));
        }

        public virtual SqlExpression ApplyTypeMapping(SqlExpression sqlExpression, RelationalTypeMapping typeMapping)
        {
            if (sqlExpression == null
                || sqlExpression.TypeMapping != null)
            {
                return sqlExpression;
            }

            return sqlExpression switch
            {
                CaseExpression e => ApplyTypeMappingOnCase(e, typeMapping),
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
            Type resultType;
            RelationalTypeMapping resultTypeMapping;
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
                    throw new InvalidOperationException(CoreStrings.UnsupportedUnary);
            }

            return new SqlUnaryExpression(sqlUnaryExpression.OperatorType, operand, resultType, resultTypeMapping);
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
                    throw new InvalidOperationException(CoreStrings.IncorrectOperatorType);
            }

            return new SqlBinaryExpression(
                sqlBinaryExpression.OperatorType,
                ApplyTypeMapping(left, inferredTypeMapping),
                ApplyTypeMapping(right, inferredTypeMapping),
                resultType,
                resultTypeMapping);
        }

        public virtual RelationalTypeMapping GetTypeMappingForValue(object value)
            => _typeMappingSource.GetMappingForValue(value);

        public virtual RelationalTypeMapping FindMapping(Type type)
        {
            Check.NotNull(type, nameof(type));

            return _typeMappingSource.FindMapping(type);
        }

        public virtual SqlBinaryExpression MakeBinary(
            ExpressionType operatorType, SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping)
        {
            Check.NotNull(operatorType, nameof(operatorType));
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

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

        public virtual SqlBinaryExpression Equal(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Equal, left, right, null);
        }

        public virtual SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.NotEqual, left, right, null);
        }

        public virtual SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.GreaterThan, left, right, null);
        }

        public virtual SqlBinaryExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.GreaterThanOrEqual, left, right, null);
        }

        public virtual SqlBinaryExpression LessThan(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.LessThan, left, right, null);
        }

        public virtual SqlBinaryExpression LessThanOrEqual(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.LessThanOrEqual, left, right, null);
        }

        public virtual SqlBinaryExpression AndAlso(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.AndAlso, left, right, null);
        }

        public virtual SqlBinaryExpression OrElse(SqlExpression left, SqlExpression right)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.OrElse, left, right, null);
        }

        public virtual SqlBinaryExpression Add(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Add, left, right, typeMapping);
        }

        public virtual SqlBinaryExpression Subtract(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Subtract, left, right, typeMapping);
        }

        public virtual SqlBinaryExpression Multiply(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Multiply, left, right, typeMapping);
        }

        public virtual SqlBinaryExpression Divide(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Divide, left, right, typeMapping);
        }

        public virtual SqlBinaryExpression Modulo(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Modulo, left, right, typeMapping);
        }

        public virtual SqlBinaryExpression And(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.And, left, right, typeMapping);
        }

        public virtual SqlBinaryExpression Or(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            return MakeBinary(ExpressionType.Or, left, right, typeMapping);
        }

        public virtual SqlFunctionExpression Coalesce(SqlExpression left, SqlExpression right, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(left, nameof(left));
            Check.NotNull(right, nameof(right));

            var resultType = right.Type;
            var inferredTypeMapping = typeMapping
                ?? ExpressionExtensions.InferTypeMapping(left, right)
                ?? _typeMappingSource.FindMapping(resultType);

            var typeMappedArguments = new List<SqlExpression>()
            {
                ApplyTypeMapping(left, inferredTypeMapping),
                ApplyTypeMapping(right, inferredTypeMapping)
            };

            return SqlFunctionExpression.Create(
                "COALESCE",
                typeMappedArguments,
                nullable: true,
                // COALESCE is handled separately since it's only nullable if *both* arguments are null
                argumentsPropagateNullability: new[] { false, false },
                resultType,
                inferredTypeMapping);
        }

        public virtual SqlUnaryExpression MakeUnary(
            ExpressionType operatorType, SqlExpression operand, Type type, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(operatorType, nameof(operand));
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(type, nameof(type));

            return (SqlUnaryExpression)ApplyTypeMapping(new SqlUnaryExpression(operatorType, operand, type, null), typeMapping);
        }

        public virtual SqlUnaryExpression IsNull(SqlExpression operand)
        {
            Check.NotNull(operand, nameof(operand));

            return MakeUnary(ExpressionType.Equal, operand, typeof(bool));
        }

        public virtual SqlUnaryExpression IsNotNull(SqlExpression operand)
        {
            Check.NotNull(operand, nameof(operand));

            return MakeUnary(ExpressionType.NotEqual, operand, typeof(bool));
        }

        public virtual SqlUnaryExpression Convert(SqlExpression operand, Type type, RelationalTypeMapping typeMapping = null)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(type, nameof(type));

            return MakeUnary(ExpressionType.Convert, operand, type, typeMapping);
        }

        public virtual SqlUnaryExpression Not(SqlExpression operand)
        {
            Check.NotNull(operand, nameof(operand));

            return MakeUnary(ExpressionType.Not, operand, operand.Type, operand.TypeMapping);
        }

        public virtual SqlUnaryExpression Negate(SqlExpression operand)
        {
            Check.NotNull(operand, nameof(operand));

            return MakeUnary(ExpressionType.Negate, operand, operand.Type, operand.TypeMapping);
        }

        public virtual CaseExpression Case(
            [NotNull] SqlExpression operand, [CanBeNull] SqlExpression elseResult, [NotNull] params CaseWhenClause[] whenClauses)
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

            var typeMappedWhenClauses = new List<CaseWhenClause>();
            foreach (var caseWhenClause in whenClauses)
            {
                typeMappedWhenClauses.Add(
                    new CaseWhenClause(
                        ApplyTypeMapping(caseWhenClause.Test, operandTypeMapping),
                        ApplyTypeMapping(caseWhenClause.Result, resultTypeMapping)));
            }

            elseResult = ApplyTypeMapping(elseResult, resultTypeMapping);

            return new CaseExpression(operand, typeMappedWhenClauses, elseResult);
        }

        public virtual CaseExpression Case(SqlExpression operand, params CaseWhenClause[] whenClauses)
        {
            Check.NotNull(operand, nameof(operand));
            Check.NotNull(whenClauses, nameof(whenClauses));

            return Case(operand, null, whenClauses);
        }

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

        [Obsolete("Use overload that explicitly specifies value for 'argumentsPropagateNullability' argument.")]
        public virtual SqlFunctionExpression Function(
            string name,
            IEnumerable<SqlExpression> arguments,
            Type returnType,
            RelationalTypeMapping typeMapping = null)
            => Function(name, arguments, nullable: true, argumentsPropagateNullability: arguments.Select(a => false), returnType, typeMapping);

        [Obsolete("Use overload that explicitly specifies value for 'argumentsPropagateNullability' argument.")]
        public virtual SqlFunctionExpression Function(
            string schema,
            string name,
            IEnumerable<SqlExpression> arguments,
            Type returnType,
            RelationalTypeMapping typeMapping = null)
            => Function(schema, name, arguments, nullable: true, argumentsPropagateNullability: arguments.Select(a => false), returnType, typeMapping);

        [Obsolete("Use overload that explicitly specifies values for 'instancePropagatesNullability' and 'argumentsPropagateNullability' arguments.")]
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

        public virtual SqlFunctionExpression Function(string name, Type returnType, RelationalTypeMapping typeMapping = null)
            => Function(name, nullable: true, returnType, typeMapping);

        public virtual SqlFunctionExpression Function(string schema, string name, Type returnType, RelationalTypeMapping typeMapping = null)
            => Function(schema, name, nullable: true, returnType, typeMapping);

        [Obsolete("Use overload that explicitly specifies value for 'instancePropagatesNullability' argument.")]
        public virtual SqlFunctionExpression Function(SqlExpression instance, string name, Type returnType, RelationalTypeMapping typeMapping = null)
            => Function(instance, name, nullable: true, instancePropagatesNullability: false, returnType, typeMapping);

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
            Check.NotNull(returnType, nameof(returnType));

            var typeMappedArguments = new List<SqlExpression>();

            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
            }

            return SqlFunctionExpression.Create(
                name,
                typeMappedArguments,
                nullable,
                argumentsPropagateNullability,
                returnType,
                typeMapping);
        }

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
            Check.NotNull(returnType, nameof(returnType));

            var typeMappedArguments = new List<SqlExpression>();
            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
            }

            return SqlFunctionExpression.Create(
                schema,
                name,
                typeMappedArguments,
                nullable,
                argumentsPropagateNullability,
                returnType,
                typeMapping);
        }

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
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(arguments, nameof(arguments));
            Check.NotNull(returnType, nameof(returnType));

            instance = ApplyDefaultTypeMapping(instance);
            var typeMappedArguments = new List<SqlExpression>();
            foreach (var argument in arguments)
            {
                typeMappedArguments.Add(ApplyDefaultTypeMapping(argument));
            }

            return SqlFunctionExpression.Create(
                instance,
                name,
                typeMappedArguments,
                nullable,
                instancePropagatesNullability,
                argumentsPropagateNullability,
                returnType,
                typeMapping);
        }

        public virtual SqlFunctionExpression Function(string name, bool nullable, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(returnType, nameof(returnType));

            return SqlFunctionExpression.CreateNiladic(name, nullable, returnType, typeMapping);
        }

        public virtual SqlFunctionExpression Function(string schema, string name, bool nullable, Type returnType, RelationalTypeMapping typeMapping = null)
        {
            Check.NotEmpty(schema, nameof(schema));
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(returnType, nameof(returnType));

            return SqlFunctionExpression.CreateNiladic(schema, name, nullable, returnType, typeMapping);
        }

        public virtual SqlFunctionExpression Function(
            SqlExpression instance,
            string name,
            bool nullable,
            bool instancePropagatesNullability,
            Type returnType,
            RelationalTypeMapping typeMapping = null)
        {
            Check.NotEmpty(name, nameof(name));
            Check.NotNull(returnType, nameof(returnType));

            return SqlFunctionExpression.CreateNiladic(
                ApplyDefaultTypeMapping(instance),
                name,
                nullable,
                instancePropagatesNullability,
                returnType,
                typeMapping);
        }

        public virtual ExistsExpression Exists(SelectExpression subquery, bool negated)
        {
            Check.NotNull(subquery, nameof(subquery));

            return new ExistsExpression(subquery, negated, _boolTypeMapping);
        }

        public virtual InExpression In(SqlExpression item, SqlExpression values, bool negated)
        {
            Check.NotNull(item, nameof(item));
            Check.NotNull(values, nameof(values));

            var typeMapping = item.TypeMapping ?? _typeMappingSource.FindMapping(item.Type);

            item = ApplyTypeMapping(item, typeMapping);
            values = ApplyTypeMapping(values, typeMapping);

            return new InExpression(item, negated, values, _boolTypeMapping);
        }

        public virtual InExpression In(SqlExpression item, SelectExpression subquery, bool negated)
        {
            Check.NotNull(item, nameof(item));
            Check.NotNull(subquery, nameof(subquery));

            var sqlExpression = subquery.Projection.Single().Expression;
            var typeMapping = sqlExpression.TypeMapping;

            if (typeMapping == null)
            {
                throw new InvalidOperationException(RelationalStrings.NoTypeMappingFoundForSubquery(subquery.Print(), sqlExpression.Type));
            }

            item = ApplyTypeMapping(item, typeMapping);
            return new InExpression(item, negated, subquery, _boolTypeMapping);
        }

        public virtual LikeExpression Like(SqlExpression match, SqlExpression pattern, SqlExpression escapeChar = null)
        {
            Check.NotNull(match, nameof(match));
            Check.NotNull(pattern, nameof(pattern));

            return (LikeExpression)ApplyDefaultTypeMapping(new LikeExpression(match, pattern, escapeChar, null));
        }

        public virtual SqlFragmentExpression Fragment(string sql)
        {
            Check.NotNull(sql, nameof(sql));

            return new SqlFragmentExpression(sql);
        }

        public virtual SqlConstantExpression Constant(object value, RelationalTypeMapping typeMapping = null)
            => new SqlConstantExpression(Expression.Constant(value), typeMapping);

        public virtual SelectExpression Select(SqlExpression projection) => new SelectExpression(projection);

        public virtual SelectExpression Select(IEntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            var selectExpression = new SelectExpression(entityType);
            AddConditions(selectExpression, entityType);

            return selectExpression;
        }

        public virtual SelectExpression Select(IEntityType entityType, TableExpressionBase tableExpressionBase)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(tableExpressionBase, nameof(tableExpressionBase));

            var selectExpression = new SelectExpression(entityType, tableExpressionBase);
            AddConditions(selectExpression, entityType);

            return selectExpression;
        }

        public virtual SelectExpression Select(IEntityType entityType, string sql, Expression sqlArguments)
        {
            Check.NotNull(entityType, nameof(entityType));
            Check.NotNull(sql, nameof(sql));

            var tableExpression = new FromSqlExpression(sql, sqlArguments,
                (entityType.GetViewOrTableMappings().SingleOrDefault()?.Table.Name
                ?? entityType.ShortName()).Substring(0, 1).ToLower());
            var selectExpression = new SelectExpression(entityType, tableExpression);
            AddConditions(selectExpression, entityType);

            return selectExpression;
        }

        private void AddConditions(
            SelectExpression selectExpression,
            IEntityType entityType,
            ITableBase table = null,
            bool skipJoins = false)
        {
            if (entityType.FindPrimaryKey() == null)
            {
                AddDiscriminatorCondition(selectExpression, entityType);
            }
            else
            {
                var tableMappings = entityType.GetViewOrTableMappings();
                if (!tableMappings.Any())
                {
                    return;
                }

                table ??= tableMappings.Single().Table;
                var discriminatorAdded = AddDiscriminatorCondition(selectExpression, entityType);

                var linkingFks = table.GetInternalForeignKeys(entityType);
                if (linkingFks.Any())
                {
                    if (!discriminatorAdded)
                    {
                        AddOptionalDependentConditions(selectExpression, entityType, table);
                    }

                    if (!skipJoins)
                    {
                        var first = true;

                        foreach (var foreignKey in linkingFks)
                        {
                            if (!(entityType.FindOwnership() == foreignKey
                                && foreignKey.PrincipalEntityType.BaseType == null))
                            {
                                var otherSelectExpression = first
                                    ? selectExpression
                                    : new SelectExpression(entityType);

                                AddInnerJoin(otherSelectExpression, foreignKey, table, skipInnerJoins: false);

                                if (first)
                                {
                                    first = false;
                                }
                                else
                                {
                                    selectExpression.ApplyUnion(otherSelectExpression, distinct: true);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void AddInnerJoin(
            SelectExpression selectExpression, IForeignKey foreignKey, ITableBase table, bool skipInnerJoins)
        {
            var joinPredicate = GenerateJoinPredicate(selectExpression, foreignKey, table, skipInnerJoins, out var innerSelect);

            selectExpression.AddInnerJoin(innerSelect, joinPredicate, null);
        }

        private SqlExpression GenerateJoinPredicate(
            SelectExpression selectExpression,
            IForeignKey foreignKey,
            ITableBase table,
            bool skipInnerJoins,
            out SelectExpression innerSelect)
        {
            var outerEntityProjection = GetMappedEntityProjectionExpression(selectExpression);
            var outerIsPrincipal = foreignKey.PrincipalEntityType.IsAssignableFrom(outerEntityProjection.EntityType);

            innerSelect = outerIsPrincipal
                ? new SelectExpression(foreignKey.DeclaringEntityType)
                : new SelectExpression(foreignKey.PrincipalEntityType);
            AddConditions(
                innerSelect,
                outerIsPrincipal ? foreignKey.DeclaringEntityType : foreignKey.PrincipalEntityType,
                table,
                skipInnerJoins);

            var innerEntityProjection = GetMappedEntityProjectionExpression(innerSelect);

            var outerKey = (outerIsPrincipal ? foreignKey.PrincipalKey.Properties : foreignKey.Properties)
                .Select(p => outerEntityProjection.BindProperty(p));
            var innerKey = (outerIsPrincipal ? foreignKey.Properties : foreignKey.PrincipalKey.Properties)
                .Select(p => innerEntityProjection.BindProperty(p));

            return outerKey.Zip<SqlExpression, SqlExpression, SqlExpression>(innerKey, Equal)
                .Aggregate(AndAlso);
        }

        private bool AddDiscriminatorCondition(SelectExpression selectExpression, IEntityType entityType)
        {
            if (entityType.GetIsDiscriminatorMappingComplete()
                && entityType.GetAllBaseTypesInclusiveAscending()
                    .All(e => (e == entityType || e.IsAbstract()) && !HasSiblings(e)))
            {
                return false;
            }

            SqlExpression predicate;
            var concreteEntityTypes = entityType.GetConcreteDerivedTypesInclusive().ToList();
            if (concreteEntityTypes.Count == 1)
            {
                var concreteEntityType = concreteEntityTypes[0];
                if (concreteEntityType.BaseType == null)
                {
                    return false;
                }

                var discriminatorColumn = GetMappedEntityProjectionExpression(selectExpression)
                    .BindProperty(concreteEntityType.GetDiscriminatorProperty());

                predicate = Equal(discriminatorColumn, Constant(concreteEntityType.GetDiscriminatorValue()));
            }
            else
            {
                var discriminatorColumn = GetMappedEntityProjectionExpression(selectExpression)
                    .BindProperty(concreteEntityTypes[0].GetDiscriminatorProperty());

                predicate = In(
                    discriminatorColumn, Constant(concreteEntityTypes.Select(et => et.GetDiscriminatorValue()).ToList()), negated: false);
            }

            selectExpression.ApplyPredicate(predicate);

            return true;

            bool HasSiblings(IEntityType entityType)
            {
                return entityType.BaseType?.GetDirectlyDerivedTypes().Any(i => i != entityType) == true;
            }
        }

        private void AddOptionalDependentConditions(
            SelectExpression selectExpression, IEntityType entityType, ITableBase table)
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

                    foreach (var referencingFk in entityType.GetReferencingForeignKeys())
                    {
                        var otherSelectExpression = new SelectExpression(entityType);

                        var sameTable = table.GetInternalForeignKeys(referencingFk.DeclaringEntityType).Any();
                        AddInnerJoin(
                            otherSelectExpression, referencingFk,
                            sameTable ? table : null,
                            skipInnerJoins: sameTable);
                        selectExpression.ApplyUnion(otherSelectExpression, distinct: true);
                    }
                }
            }
        }

        private EntityProjectionExpression GetMappedEntityProjectionExpression(SelectExpression selectExpression)
            => (EntityProjectionExpression)selectExpression.GetMappedProjection(new ProjectionMember());

        private SqlExpression IsNotNull(IProperty property, EntityProjectionExpression entityProjection)
            => IsNotNull(entityProjection.BindProperty(property));
    }
}
