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
        /// <summary>
        ///     Gets the relational database type for a given object, throwing if no mapping is found.
        /// </summary>
        /// <param name="value"> The object to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        [Obsolete("Use IRelationalTypeMappingSource directly.")]
        RelationalTypeMapping GetTypeMappingForValue([CanBeNull] object value);

        /// <summary>
        ///     Finds the type mapping for a given <see cref="Type" />.
        /// </summary>
        /// <param name="type"> The CLR type. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        [Obsolete("Use IRelationalTypeMappingSource directly.")]
        RelationalTypeMapping FindMapping([NotNull] Type type);

        /// <summary>
        ///     Applies type mapping to the given <see cref="SqlExpression" />.
        /// </summary>
        /// <param name="sqlExpression"> A SQL expression to apply type mapping. </param>
        /// <param name="typeMapping"> A type mapping to apply. </param>
        /// <returns> A SQL expression with given type mapping applied. </returns>
        SqlExpression ApplyTypeMapping([CanBeNull] SqlExpression sqlExpression, [CanBeNull] RelationalTypeMapping typeMapping);

        /// <summary>
        ///     Applies default type mapping to given <see cref="SqlExpression" />.
        /// </summary>
        /// <param name="sqlExpression"> A SQL Expression to apply default type mapping. </param>
        /// <returns> A SQL expression with default type mapping applied. </returns>
        SqlExpression ApplyDefaultTypeMapping([CanBeNull] SqlExpression sqlExpression);

        /// <summary>
        ///     Creates a new <see cref="SqlUnaryExpression" /> with the given arguments.
        /// </summary>
        /// <param name="operatorType"> An <see cref="ExpressionType" /> reprenting SQL unary operator. </param>
        /// <param name="operand"> A <see cref="SqlExpression" /> to apply unary operator on. </param>
        /// <param name="type"> The type of the created expression. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> A <see cref="SqlUnaryExpression" /> with the given arguments. </returns>
        SqlUnaryExpression MakeUnary(
            ExpressionType operatorType,
            [NotNull] SqlExpression operand,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlBinaryExpression" /> with the given arguments.
        /// </summary>
        /// <param name="operatorType"> An <see cref="ExpressionType" /> reprenting SQL unary operator. </param>
        /// <param name="left"> The left operand of binary operation. </param>
        /// <param name="right"> The right operand of binary operation. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> A <see cref="SqlBinaryExpression" /> with the given arguments. </returns>
        SqlBinaryExpression MakeBinary(
            ExpressionType operatorType,
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [CanBeNull] RelationalTypeMapping typeMapping);

        // Comparison
        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents an equality comparison.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL equality comparison. </returns>
        SqlBinaryExpression Equal([NotNull] SqlExpression left, [NotNull] SqlExpression right);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents an inequality comparison.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL inequality comparison. </returns>
        SqlBinaryExpression NotEqual([NotNull] SqlExpression left, [NotNull] SqlExpression right);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a greater than comparison.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL greater than comparison. </returns>
        SqlBinaryExpression GreaterThan([NotNull] SqlExpression left, [NotNull] SqlExpression right);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a greater than or equal comparison.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL greater than or equal comparison. </returns>
        SqlBinaryExpression GreaterThanOrEqual([NotNull] SqlExpression left, [NotNull] SqlExpression right);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a less than comparison.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL less than comparison. </returns>
        SqlBinaryExpression LessThan([NotNull] SqlExpression left, [NotNull] SqlExpression right);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a less than or equal comparison.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL less than or equal comparison. </returns>
        SqlBinaryExpression LessThanOrEqual([NotNull] SqlExpression left, [NotNull] SqlExpression right);

        // Logical
        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a logical AND operation.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL AND operation. </returns>
        SqlBinaryExpression AndAlso([NotNull] SqlExpression left, [NotNull] SqlExpression right);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a logical OR operation.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL OR operation. </returns>
        SqlBinaryExpression OrElse([NotNull] SqlExpression left, [NotNull] SqlExpression right);

        // Arithmetic
        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents an addition.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL addition. </returns>
        SqlBinaryExpression Add(
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a subtraction.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL subtraction. </returns>
        SqlBinaryExpression Subtract(
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a multiplication.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL multiplication. </returns>
        SqlBinaryExpression Multiply(
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a division.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL division. </returns>
        SqlBinaryExpression Divide(
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a modulo operation.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL modulo operation. </returns>
        SqlBinaryExpression Modulo(
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        // Bitwise
        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a bitwise AND operation.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL bitwise AND operation. </returns>
        SqlBinaryExpression And(
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a bitwise OR operation.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL bitwise OR operation. </returns>
        SqlBinaryExpression Or(
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        // Other
        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a bitwise OR operation.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL bitwise OR operation. </returns>
        SqlFunctionExpression Coalesce(
            [NotNull] SqlExpression left,
            [NotNull] SqlExpression right,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlUnaryExpression" /> which represent equality to null.
        /// </summary>
        /// <param name="operand"> A <see cref="SqlExpression" /> to compare to null. </param>
        /// <returns> An expression representing IS NULL construct in a SQL tree. </returns>
        SqlUnaryExpression IsNull([NotNull] SqlExpression operand);

        /// <summary>
        ///     Creates a new <see cref="SqlUnaryExpression" /> which represent inequality to null.
        /// </summary>
        /// <param name="operand"> A <see cref="SqlExpression" /> to compare to non null. </param>
        /// <returns> An expression representing IS NOT NULL construct in a SQL tree. </returns>
        SqlUnaryExpression IsNotNull([NotNull] SqlExpression operand);

        /// <summary>
        ///     Creates a new <see cref="SqlUnaryExpression" /> which represent casting a SQL expression to different type.
        /// </summary>
        /// <param name="operand"> A <see cref="SqlExpression" /> to cast. </param>
        /// <param name="type"> The return type of the expression after cast. </param>
        /// <param name="typeMapping"> A relational type mapping to use for conversion. </param>
        /// <returns> An expression representing cast operation in a SQL tree. </returns>
        SqlUnaryExpression Convert(
            [NotNull] SqlExpression operand,
            [NotNull] Type type,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlUnaryExpression" /> which represent a NOT operation in a SQL tree.
        /// </summary>
        /// <param name="operand"> A <see cref="SqlExpression" /> to apply NOT on. </param>
        /// <returns> An expression representing a NOT operation in a SQL tree. </returns>
        SqlUnaryExpression Not([NotNull] SqlExpression operand);

        /// <summary>
        ///     Creates a new <see cref="SqlUnaryExpression" /> which represent a negation operation in a SQL tree.
        /// </summary>
        /// <param name="operand"> A <see cref="SqlExpression" /> to apply NOT on. </param>
        /// <returns> An expression representing a negation operation in a SQL tree. </returns>
        SqlUnaryExpression Negate([NotNull] SqlExpression operand);

        /// <summary>
        ///     Creates a new <see cref="CaseExpression" /> which represent a CASE statement in a SQL tree.
        /// </summary>
        /// <param name="operand"> An expression to compare with <see cref="CaseWhenClause.Test" /> in <paramref name="whenClauses" />. </param>
        /// <param name="whenClauses"> A list of <see cref="CaseWhenClause" /> to compare and get result from. </param>
        /// <returns> An expression representing a CASE statement in a SQL tree. </returns>
        [Obsolete("Use overload which takes IReadOnlyList instead of params")]
        CaseExpression Case([NotNull] SqlExpression operand, [NotNull] params CaseWhenClause[] whenClauses);

        /// <summary>
        ///     Creates a new <see cref="CaseExpression" /> which represent a CASE statement in a SQL tree.
        /// </summary>
        /// <param name="operand"> An expression to compare with <see cref="CaseWhenClause.Test" /> in <paramref name="whenClauses" />. </param>
        /// <param name="whenClauses"> A list of <see cref="CaseWhenClause" /> to compare and get result from. </param>
        /// <param name="elseResult"> A value to return if no <paramref name="whenClauses" /> matches, if any. </param>
        /// <returns> An expression representing a CASE statement in a SQL tree. </returns>
        CaseExpression Case(
            [NotNull] SqlExpression operand,
            [NotNull] IReadOnlyList<CaseWhenClause> whenClauses,
            [CanBeNull] SqlExpression elseResult);

        /// <summary>
        ///     Creates a new <see cref="CaseExpression" /> which represent a CASE statement in a SQL tree.
        /// </summary>
        /// <param name="whenClauses"> A list of <see cref="CaseWhenClause" /> to evaluate condition and get result from. </param>
        /// <param name="elseResult"> A value to return if no <paramref name="whenClauses" /> matches, if any. </param>
        /// <returns> An expression representing a CASE statement in a SQL tree. </returns>
        CaseExpression Case([NotNull] IReadOnlyList<CaseWhenClause> whenClauses, [CanBeNull] SqlExpression elseResult);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a function call in a SQL tree.
        /// </summary>
        /// <param name="name"> The name of the function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        [Obsolete("Use overload that explicitly specifies value for 'argumentsPropagateNullability' argument.")]
        SqlFunctionExpression Function(
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a function call in a SQL tree.
        /// </summary>
        /// <param name="schema"> The schema in which the function is defined. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        [Obsolete("Use overload that explicitly specifies value for 'argumentsPropagateNullability' argument.")]
        SqlFunctionExpression Function(
            [CanBeNull] string schema,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a function call in a SQL tree.
        /// </summary>
        /// <param name="instance"> An expression on which the function is applied. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        [Obsolete(
            "Use overload that explicitly specifies value for 'instancePropagatesNullability' and 'argumentsPropagateNullability' arguments.")]
        SqlFunctionExpression Function(
            [NotNull] SqlExpression instance,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a function call in a SQL tree.
        /// </summary>
        /// <param name="name"> The name of the function. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        [Obsolete("Use NiladicFunction method.")]
        SqlFunctionExpression Function(
            [NotNull] string name,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a function call in a SQL tree.
        /// </summary>
        /// <param name="schema"> The schema in which the function is defined. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        [Obsolete("Use NiladicFunction method.")]
        SqlFunctionExpression Function(
            [NotNull] string schema,
            [NotNull] string name,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a function call in a SQL tree.
        /// </summary>
        /// <param name="instance"> An expression on which the function is applied. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        [Obsolete("Use NiladicFunction method.")]
        SqlFunctionExpression Function(
            [NotNull] SqlExpression instance,
            [NotNull] string name,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a function call in a SQL tree.
        /// </summary>
        /// <param name="name"> The name of the function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        /// <param name="nullable"> A bool value indicating whether this function can return null. </param>
        /// <param name="argumentsPropagateNullability"> A list of bool values indicating whether individual arguments propagate null to result. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        SqlFunctionExpression Function(
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullable,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a function call in a SQL tree.
        /// </summary>
        /// <param name="schema"> The schema in which the function is defined. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        /// <param name="nullable"> A bool value indicating whether this function can return null. </param>
        /// <param name="argumentsPropagateNullability"> A list of bool values indicating whether individual arguments propagate null to result. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        SqlFunctionExpression Function(
            [NotNull] string schema,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullable,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a function call in a SQL tree.
        /// </summary>
        /// <param name="instance"> An expression on which the function is applied. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="arguments"> The arguments of the function. </param>
        /// <param name="nullable"> A bool value indicating whether this function can return null. </param>
        /// <param name="instancePropagatesNullability"> A value indicating if instance propagates null to result. </param>
        /// <param name="argumentsPropagateNullability"> A list of bool values indicating whether individual arguments propagate null to result. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        SqlFunctionExpression Function(
            [NotNull] SqlExpression instance,
            [NotNull] string name,
            [NotNull] IEnumerable<SqlExpression> arguments,
            bool nullable,
            bool instancePropagatesNullability,
            [NotNull] IEnumerable<bool> argumentsPropagateNullability,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a niladic function call in a SQL tree.
        /// </summary>
        /// <param name="name"> The name of the function. </param>
        /// <param name="nullable"> A bool value indicating whether this function can return null. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        SqlFunctionExpression NiladicFunction(
            [NotNull] string name,
            bool nullable,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a niladic function call in a SQL tree.
        /// </summary>
        /// <param name="schema"> The schema in which the function is defined. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="nullable"> A bool value indicating whether this function can return null. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        SqlFunctionExpression NiladicFunction(
            [NotNull] string schema,
            [NotNull] string name,
            bool nullable,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a niladic function call in a SQL tree.
        /// </summary>
        /// <param name="instance"> An expression on which the function is applied. </param>
        /// <param name="name"> The name of the function. </param>
        /// <param name="nullable"> A bool value indicating whether this function can return null. </param>
        /// <param name="instancePropagatesNullability"> A value indicating if instance propagates null to result. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        SqlFunctionExpression NiladicFunction(
            [NotNull] SqlExpression instance,
            [NotNull] string name,
            bool nullable,
            bool instancePropagatesNullability,
            [NotNull] Type returnType,
            [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="ExistsExpression" /> which represents an EXISTS operation in a SQL tree.
        /// </summary>
        /// <param name="subquery"> A subquery to check existence of. </param>
        /// <param name="negated"> A value indicating if the existence check is negated. </param>
        /// <returns> An expression representing an EXISTS operation in a SQL tree. </returns>
        ExistsExpression Exists([NotNull] SelectExpression subquery, bool negated);

        /// <summary>
        ///     Creates a new <see cref="InExpression" /> which represents an IN operation in a SQL tree.
        /// </summary>
        /// <param name="item"> An item to look into values. </param>
        /// <param name="values"> A list of values in which item is searched. </param>
        /// <param name="negated"> A value indicating if the item should be present in the values or absent. </param>
        /// <returns> An expression representing an IN operation in a SQL tree. </returns>
        InExpression In([NotNull] SqlExpression item, [NotNull] SqlExpression values, bool negated);

        /// <summary>
        ///     Creates a new <see cref="InExpression" /> which represents an IN operation in a SQL tree.
        /// </summary>
        /// <param name="item"> An item to look into values. </param>
        /// <param name="subquery"> A subquery in which item is searched. </param>
        /// <param name="negated"> A value indicating if the item should be present in the values or absent. </param>
        /// <returns> An expression representing an IN operation in a SQL tree. </returns>
        InExpression In([NotNull] SqlExpression item, [NotNull] SelectExpression subquery, bool negated);

        /// <summary>
        ///     Creates a new <see cref="InExpression" /> which represents a LIKE in a SQL tree.
        /// </summary>
        /// <param name="match"> An expression on which LIKE is applied. </param>
        /// <param name="pattern"> A pattern to search. </param>
        /// <param name="escapeChar"> An optional escape character to use in LIKE. </param>
        /// <returns> An expression representing a LIKE in a SQL tree. </returns>
        LikeExpression Like([NotNull] SqlExpression match, [NotNull] SqlExpression pattern, [CanBeNull] SqlExpression escapeChar = null);

        /// <summary>
        ///     Creates a new <see cref="SqlConstantExpression" /> which represents a constant in a SQL tree.
        /// </summary>
        /// <param name="value"> A value. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a LIKE in a SQL tree. </returns>
        SqlConstantExpression Constant([NotNull] object value, [CanBeNull] RelationalTypeMapping typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFragmentExpression" /> which represents a SQL token.
        /// </summary>
        /// <param name="sql"> A string token to print in SQL tree. </param>
        /// <returns> An expression representing a SQL token. </returns>
        SqlFragmentExpression Fragment([NotNull] string sql);

        /// <summary>
        ///     Creates a new <see cref="SelectExpression" /> which represents a SELECT in a SQL tree projecting a <see cref="SqlExpression" />
        ///     or 1 from no table and without any composition.
        /// </summary>
        /// <param name="projection"> A <see cref="SqlExpression" /> to project. </param>
        /// <returns> An expression representing a SELECT in a SQL tree. </returns>
        SelectExpression Select([CanBeNull] SqlExpression projection);

        /// <summary>
        ///     Creates a new <see cref="SelectExpression" /> which represents a SELECT in a SQL tree projecting an entity type from
        ///     a table source created using default mapping in the model.
        /// </summary>
        /// <param name="entityType"> An entity type to project. </param>
        /// <returns> An expression representing a SELECT in a SQL tree. </returns>
        SelectExpression Select([NotNull] IEntityType entityType);

        /// <summary>
        ///     Creates a new <see cref="SelectExpression" /> which represents a SELECT in a SQL tree projecting an entity type from
        ///     a table source.
        /// </summary>
        /// <param name="entityType"> An entity type to project. </param>
        /// <param name="tableExpressionBase"> A table source to project from. </param>
        /// <returns> An expression representing a SELECT in a SQL tree. </returns>
        SelectExpression Select([NotNull] IEntityType entityType, [NotNull] TableExpressionBase tableExpressionBase);

        /// <summary>
        ///     Creates a new <see cref="SelectExpression" /> which represents a SELECT in a SQL tree projecting an entity type from
        ///     a table source created using a custom SQL.
        /// </summary>
        /// <param name="entityType"> An entity type to project. </param>
        /// <param name="sql"> A custom SQL for the table source. </param>
        /// <param name="sqlArguments"> An expression representing parameters passed to the custom SQL. </param>
        /// <returns> An expression representing a SELECT in a SQL tree. </returns>
        [Obsolete("Use overload which takes TableExpressionBase by passing FromSqlExpression directly.")]
        SelectExpression Select([NotNull] IEntityType entityType, [NotNull] string sql, [NotNull] Expression sqlArguments);
    }
}
