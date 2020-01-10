// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A factory for creating <see cref="SqlExpression" /> instances.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public interface ISqlExpressionFactory
    {
        SqlExpression ApplyTypeMapping([CanBeNull] SqlExpression sqlExpression, [CanBeNull] RelationalTypeMapping typeMapping);
        SqlExpression ApplyDefaultTypeMapping([CanBeNull] SqlExpression sqlExpression);
        RelationalTypeMapping GetTypeMappingForValue([CanBeNull] object value);
        RelationalTypeMapping FindMapping([NotNull] Type type);

        SqlUnaryExpression MakeUnary(
            ExpressionType operatorType,
            [NotNull] SqlExpression operand,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlBinaryExpression MakeBinary(
            ExpressionType operatorType,
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [CanBeNull] RelationalTypeMapping typeMapping);

        // Comparison
        SqlBinaryExpression Equal([NotNull] SqlExpression left, [NotNull] SqlExpression right);
        SqlBinaryExpression NotEqual([NotNull] SqlExpression left, [NotNull] SqlExpression right);
        SqlBinaryExpression GreaterThan([NotNull] SqlExpression left, [NotNull] SqlExpression right);
        SqlBinaryExpression GreaterThanOrEqual([NotNull] SqlExpression left, [NotNull] SqlExpression right);
        SqlBinaryExpression LessThan([NotNull] SqlExpression left, [NotNull] SqlExpression right);

        SqlBinaryExpression LessThanOrEqual([NotNull] SqlExpression left, [NotNull] SqlExpression right);

        // Logical
        SqlBinaryExpression AndAlso([NotNull] SqlExpression left, [NotNull] SqlExpression right);

        SqlBinaryExpression OrElse([NotNull] SqlExpression left, [NotNull] SqlExpression right);

        // Arithmetic
        SqlBinaryExpression Add(
            [NotNull] SqlExpression left, [NotNull] SqlExpression right, [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlBinaryExpression Subtract(
            [NotNull] SqlExpression left, [NotNull] SqlExpression right, [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlBinaryExpression Multiply(
            [NotNull] SqlExpression left, [NotNull] SqlExpression right, [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlBinaryExpression Divide(
            [NotNull] SqlExpression left, [NotNull] SqlExpression right, [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlBinaryExpression Modulo(
            [NotNull] SqlExpression left, [NotNull] SqlExpression right, [CanBeNull] RelationalTypeMapping typeMapping = null);

        // Bitwise
        SqlBinaryExpression And(
            [NotNull] SqlExpression left, [NotNull] SqlExpression right, [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlBinaryExpression Or(
            [NotNull] SqlExpression left, [NotNull] SqlExpression right, [CanBeNull] RelationalTypeMapping typeMapping = null);

        // Other
        SqlFunctionExpression Coalesce(
            [NotNull] SqlExpression left, [NotNull] SqlExpression right, [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlUnaryExpression IsNull([NotNull] SqlExpression operand);
        SqlUnaryExpression IsNotNull([NotNull] SqlExpression operand);

        SqlUnaryExpression Convert(
            [NotNull] SqlExpression operand, [NotNull] Type type, [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlUnaryExpression Not([NotNull] SqlExpression operand);
        SqlUnaryExpression Negate([NotNull] SqlExpression operand);

        CaseExpression Case([NotNull] SqlExpression operand, [NotNull] params CaseWhenClause[] whenClauses);
        CaseExpression Case([NotNull] IReadOnlyList<CaseWhenClause> whenClauses, [CanBeNull] SqlExpression elseResult);

        SqlFunctionExpression Function(
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlFunctionExpression Function(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlFunctionExpression Function(
            [CanBeNull] SqlExpression instance,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlFunctionExpression Function(
            [NotNull] string name,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlFunctionExpression Function(
            [NotNull] string schema,
            [NotNull] string name,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlFunctionExpression Function(
            [CanBeNull] SqlExpression instance,
            [NotNull] string name,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlFunctionExpression Function(
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullResultAllowed,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlFunctionExpression Function(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullResultAllowed,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlFunctionExpression Function(
            [CanBeNull] SqlExpression instance,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullResultAllowed,
            bool instancePropagatesNullability,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlFunctionExpression Function(
            [NotNull] string name,
            bool nullResultAllowed,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlFunctionExpression Function(
            [NotNull] string schema,
            [NotNull] string name,
            bool nullResultAllowed,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        SqlFunctionExpression Function(
            [CanBeNull] SqlExpression instance,
            [NotNull] string name,
            bool nullResultAllowed,
            bool instancePropagatesNullability,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        ExistsExpression Exists([NotNull] SelectExpression subquery, bool negated);
        InExpression In([NotNull] SqlExpression item, [NotNull] SqlExpression values, bool negated);
        InExpression In([NotNull] SqlExpression item, [NotNull] SelectExpression subquery, bool negated);
        LikeExpression Like([NotNull] SqlExpression match, [NotNull] SqlExpression pattern, [CanBeNull] SqlExpression escapeChar = null);
        SqlConstantExpression Constant([NotNull] object value, [CanBeNull] RelationalTypeMapping typeMapping = null);
        SqlFragmentExpression Fragment([NotNull] string sql);

        SelectExpression Select([CanBeNull] SqlExpression projection);
        SelectExpression Select([NotNull] IEntityType entityType);
        SelectExpression Select([NotNull] IEntityType entityType, [NotNull] string sql, [NotNull] Expression sqlArguments);
    }
}
