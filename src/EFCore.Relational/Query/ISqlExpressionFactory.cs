// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
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
        RelationalTypeMapping GetTypeMappingForValue(object? value);

        /// <summary>
        ///     Finds the type mapping for a given <see cref="Type" />.
        /// </summary>
        /// <param name="type"> The CLR type. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        [Obsolete("Use IRelationalTypeMappingSource directly.")]
        RelationalTypeMapping? FindMapping(Type type);

        /// <summary>
        ///     Applies type mapping to the given <see cref="SqlExpression" />.
        /// </summary>
        /// <param name="sqlExpression"> A SQL expression to apply type mapping. </param>
        /// <param name="typeMapping"> A type mapping to apply. </param>
        /// <returns> A SQL expression with given type mapping applied. </returns>
        [return: NotNullIfNotNull("sqlExpression")]
        SqlExpression? ApplyTypeMapping(SqlExpression? sqlExpression, RelationalTypeMapping? typeMapping);

        /// <summary>
        ///     Applies default type mapping to given <see cref="SqlExpression" />.
        /// </summary>
        /// <param name="sqlExpression"> A SQL Expression to apply default type mapping. </param>
        /// <returns> A SQL expression with default type mapping applied. </returns>
        [return: NotNullIfNotNull("sqlExpression")]
        SqlExpression? ApplyDefaultTypeMapping(SqlExpression? sqlExpression);

        /// <summary>
        ///     Creates a new <see cref="SqlUnaryExpression" /> with the given arguments.
        /// </summary>
        /// <param name="operatorType"> An <see cref="ExpressionType" /> reprenting SQL unary operator. </param>
        /// <param name="operand"> A <see cref="SqlExpression" /> to apply unary operator on. </param>
        /// <param name="type"> The type of the created expression. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> A <see cref="SqlUnaryExpression" /> with the given arguments. </returns>
        SqlUnaryExpression? MakeUnary(
            ExpressionType operatorType,
            SqlExpression operand,
            Type type,
            RelationalTypeMapping? typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlBinaryExpression" /> with the given arguments.
        /// </summary>
        /// <param name="operatorType"> An <see cref="ExpressionType" /> reprenting SQL unary operator. </param>
        /// <param name="left"> The left operand of binary operation. </param>
        /// <param name="right"> The right operand of binary operation. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> A <see cref="SqlBinaryExpression" /> with the given arguments. </returns>
        SqlBinaryExpression? MakeBinary(
            ExpressionType operatorType,
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping? typeMapping);

        // Comparison
        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents an equality comparison.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL equality comparison. </returns>
        SqlBinaryExpression Equal(SqlExpression left, SqlExpression right);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents an inequality comparison.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL inequality comparison. </returns>
        SqlBinaryExpression NotEqual(SqlExpression left, SqlExpression right);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a greater than comparison.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL greater than comparison. </returns>
        SqlBinaryExpression GreaterThan(SqlExpression left, SqlExpression right);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a greater than or equal comparison.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL greater than or equal comparison. </returns>
        SqlBinaryExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a less than comparison.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL less than comparison. </returns>
        SqlBinaryExpression LessThan(SqlExpression left, SqlExpression right);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a less than or equal comparison.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL less than or equal comparison. </returns>
        SqlBinaryExpression LessThanOrEqual(SqlExpression left, SqlExpression right);

        // Logical
        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a logical AND operation.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL AND operation. </returns>
        SqlBinaryExpression AndAlso(SqlExpression left, SqlExpression right);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a logical OR operation.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <returns> An expression representing a SQL OR operation. </returns>
        SqlBinaryExpression OrElse(SqlExpression left, SqlExpression right);

        // Arithmetic
        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents an addition.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL addition. </returns>
        SqlBinaryExpression Add(
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping? typeMapping = null);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a subtraction.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL subtraction. </returns>
        SqlBinaryExpression Subtract(
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping? typeMapping = null);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a multiplication.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL multiplication. </returns>
        SqlBinaryExpression Multiply(
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping? typeMapping = null);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a division.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL division. </returns>
        SqlBinaryExpression Divide(
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping? typeMapping = null);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a modulo operation.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL modulo operation. </returns>
        SqlBinaryExpression Modulo(
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping? typeMapping = null);

        // Bitwise
        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a bitwise AND operation.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL bitwise AND operation. </returns>
        SqlBinaryExpression And(
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping? typeMapping = null);

        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a bitwise OR operation.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL bitwise OR operation. </returns>
        SqlBinaryExpression Or(
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping? typeMapping = null);

        // Other
        /// <summary>
        ///     Creates a <see cref="SqlBinaryExpression" /> which represents a bitwise OR operation.
        /// </summary>
        /// <param name="left"> The left operand. </param>
        /// <param name="right"> The right operand. </param>
        /// <param name="typeMapping"> A type mapping to be assigned to the created expression. </param>
        /// <returns> An expression representing a SQL bitwise OR operation. </returns>
        SqlFunctionExpression Coalesce(
            SqlExpression left,
            SqlExpression right,
            RelationalTypeMapping? typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlUnaryExpression" /> which represent equality to null.
        /// </summary>
        /// <param name="operand"> A <see cref="SqlExpression" /> to compare to null. </param>
        /// <returns> An expression representing IS NULL construct in a SQL tree. </returns>
        SqlUnaryExpression IsNull(SqlExpression operand);

        /// <summary>
        ///     Creates a new <see cref="SqlUnaryExpression" /> which represent inequality to null.
        /// </summary>
        /// <param name="operand"> A <see cref="SqlExpression" /> to compare to non null. </param>
        /// <returns> An expression representing IS NOT NULL construct in a SQL tree. </returns>
        SqlUnaryExpression IsNotNull(SqlExpression operand);

        /// <summary>
        ///     Creates a new <see cref="SqlUnaryExpression" /> which represent casting a SQL expression to different type.
        /// </summary>
        /// <param name="operand"> A <see cref="SqlExpression" /> to cast. </param>
        /// <param name="type"> The return type of the expression after cast. </param>
        /// <param name="typeMapping"> A relational type mapping to use for conversion. </param>
        /// <returns> An expression representing cast operation in a SQL tree. </returns>
        SqlUnaryExpression Convert(
            SqlExpression operand,
            Type type,
            RelationalTypeMapping? typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlUnaryExpression" /> which represent a NOT operation in a SQL tree.
        /// </summary>
        /// <param name="operand"> A <see cref="SqlExpression" /> to apply NOT on. </param>
        /// <returns> An expression representing a NOT operation in a SQL tree. </returns>
        SqlUnaryExpression Not(SqlExpression operand);

        /// <summary>
        ///     Creates a new <see cref="SqlUnaryExpression" /> which represent a negation operation in a SQL tree.
        /// </summary>
        /// <param name="operand"> A <see cref="SqlExpression" /> to apply NOT on. </param>
        /// <returns> An expression representing a negation operation in a SQL tree. </returns>
        SqlUnaryExpression Negate(SqlExpression operand);

        /// <summary>
        ///     Creates a new <see cref="CaseExpression" /> which represent a CASE statement in a SQL tree.
        /// </summary>
        /// <param name="operand"> An expression to compare with <see cref="CaseWhenClause.Test" /> in <paramref name="whenClauses" />. </param>
        /// <param name="whenClauses"> A list of <see cref="CaseWhenClause" /> to compare and get result from. </param>
        /// <returns> An expression representing a CASE statement in a SQL tree. </returns>
        [Obsolete("Use overload which takes IReadOnlyList instead of params")]
        CaseExpression Case(SqlExpression operand, params CaseWhenClause[] whenClauses);

        /// <summary>
        ///     Creates a new <see cref="CaseExpression" /> which represent a CASE statement in a SQL tree.
        /// </summary>
        /// <param name="operand"> An expression to compare with <see cref="CaseWhenClause.Test" /> in <paramref name="whenClauses" />. </param>
        /// <param name="whenClauses"> A list of <see cref="CaseWhenClause" /> to compare and get result from. </param>
        /// <param name="elseResult"> A value to return if no <paramref name="whenClauses" /> matches, if any. </param>
        /// <returns> An expression representing a CASE statement in a SQL tree. </returns>
        CaseExpression Case(
            SqlExpression operand,
            IReadOnlyList<CaseWhenClause> whenClauses,
            SqlExpression? elseResult);

        /// <summary>
        ///     Creates a new <see cref="CaseExpression" /> which represent a CASE statement in a SQL tree.
        /// </summary>
        /// <param name="whenClauses"> A list of <see cref="CaseWhenClause" /> to evaluate condition and get result from. </param>
        /// <param name="elseResult"> A value to return if no <paramref name="whenClauses" /> matches, if any. </param>
        /// <returns> An expression representing a CASE statement in a SQL tree. </returns>
        CaseExpression Case(IReadOnlyList<CaseWhenClause> whenClauses, SqlExpression? elseResult);

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
            string name,
            IEnumerable<SqlExpression> arguments,
            Type returnType,
            RelationalTypeMapping? typeMapping = null);

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
            string? schema,
            string name,
            IEnumerable<SqlExpression> arguments,
            Type returnType,
            RelationalTypeMapping? typeMapping = null);

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
            SqlExpression instance,
            string name,
            IEnumerable<SqlExpression> arguments,
            Type returnType,
            RelationalTypeMapping? typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a function call in a SQL tree.
        /// </summary>
        /// <param name="name"> The name of the function. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        [Obsolete("Use NiladicFunction method.")]
        SqlFunctionExpression Function(
            string name,
            Type returnType,
            RelationalTypeMapping? typeMapping = null);

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
            string schema,
            string name,
            Type returnType,
            RelationalTypeMapping? typeMapping = null);

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
            SqlExpression instance,
            string name,
            Type returnType,
            RelationalTypeMapping? typeMapping = null);

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
            string name,
            IEnumerable<SqlExpression> arguments,
            bool nullable,
            IEnumerable<bool> argumentsPropagateNullability,
            Type returnType,
            RelationalTypeMapping? typeMapping = null);

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
            string? schema,
            string name,
            IEnumerable<SqlExpression> arguments,
            bool nullable,
            IEnumerable<bool> argumentsPropagateNullability,
            Type returnType,
            RelationalTypeMapping? typeMapping = null);

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
            SqlExpression instance,
            string name,
            IEnumerable<SqlExpression> arguments,
            bool nullable,
            bool instancePropagatesNullability,
            IEnumerable<bool> argumentsPropagateNullability,
            Type returnType,
            RelationalTypeMapping? typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFunctionExpression" /> which represents a niladic function call in a SQL tree.
        /// </summary>
        /// <param name="name"> The name of the function. </param>
        /// <param name="nullable"> A bool value indicating whether this function can return null. </param>
        /// <param name="returnType"> The <see cref="Type" /> of the expression. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a function call in a SQL tree. </returns>
        SqlFunctionExpression NiladicFunction(
            string name,
            bool nullable,
            Type returnType,
            RelationalTypeMapping? typeMapping = null);

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
            string schema,
            string name,
            bool nullable,
            Type returnType,
            RelationalTypeMapping? typeMapping = null);

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
            SqlExpression instance,
            string name,
            bool nullable,
            bool instancePropagatesNullability,
            Type returnType,
            RelationalTypeMapping? typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="ExistsExpression" /> which represents an EXISTS operation in a SQL tree.
        /// </summary>
        /// <param name="subquery"> A subquery to check existence of. </param>
        /// <param name="negated"> A value indicating if the existence check is negated. </param>
        /// <returns> An expression representing an EXISTS operation in a SQL tree. </returns>
        ExistsExpression Exists(SelectExpression subquery, bool negated);

        /// <summary>
        ///     Creates a new <see cref="InExpression" /> which represents an IN operation in a SQL tree.
        /// </summary>
        /// <param name="item"> An item to look into values. </param>
        /// <param name="values"> A list of values in which item is searched. </param>
        /// <param name="negated"> A value indicating if the item should be present in the values or absent. </param>
        /// <returns> An expression representing an IN operation in a SQL tree. </returns>
        InExpression In(SqlExpression item, SqlExpression values, bool negated);

        /// <summary>
        ///     Creates a new <see cref="InExpression" /> which represents an IN operation in a SQL tree.
        /// </summary>
        /// <param name="item"> An item to look into values. </param>
        /// <param name="subquery"> A subquery in which item is searched. </param>
        /// <param name="negated"> A value indicating if the item should be present in the values or absent. </param>
        /// <returns> An expression representing an IN operation in a SQL tree. </returns>
        InExpression In(SqlExpression item, SelectExpression subquery, bool negated);

        /// <summary>
        ///     Creates a new <see cref="InExpression" /> which represents a LIKE in a SQL tree.
        /// </summary>
        /// <param name="match"> An expression on which LIKE is applied. </param>
        /// <param name="pattern"> A pattern to search. </param>
        /// <param name="escapeChar"> An optional escape character to use in LIKE. </param>
        /// <returns> An expression representing a LIKE in a SQL tree. </returns>
        LikeExpression Like(SqlExpression match, SqlExpression pattern, SqlExpression? escapeChar = null);

        /// <summary>
        ///     Creates a new <see cref="SqlConstantExpression" /> which represents a constant in a SQL tree.
        /// </summary>
        /// <param name="value"> A value. </param>
        /// <param name="typeMapping"> The <see cref="RelationalTypeMapping" /> associated with the expression. </param>
        /// <returns> An expression representing a LIKE in a SQL tree. </returns>
        SqlConstantExpression Constant(object? value, RelationalTypeMapping? typeMapping = null);

        /// <summary>
        ///     Creates a new <see cref="SqlFragmentExpression" /> which represents a SQL token.
        /// </summary>
        /// <param name="sql"> A string token to print in SQL tree. </param>
        /// <returns> An expression representing a SQL token. </returns>
        SqlFragmentExpression Fragment(string sql);

        /// <summary>
        ///     Creates a new <see cref="SelectExpression" /> which represents a SELECT in a SQL tree projecting a <see cref="SqlExpression" />
        ///     or 1 from no table and without any composition.
        /// </summary>
        /// <param name="projection"> A <see cref="SqlExpression" /> to project. </param>
        /// <returns> An expression representing a SELECT in a SQL tree. </returns>
        SelectExpression Select(SqlExpression? projection);

        /// <summary>
        ///     Creates a new <see cref="SelectExpression" /> which represents a SELECT in a SQL tree projecting an entity type from
        ///     a table source created using default mapping in the model.
        /// </summary>
        /// <param name="entityType"> An entity type to project. </param>
        /// <returns> An expression representing a SELECT in a SQL tree. </returns>
        SelectExpression Select(IEntityType entityType);

        /// <summary>
        ///     Creates a new <see cref="SelectExpression" /> which represents a SELECT in a SQL tree projecting an entity type from
        ///     a table source.
        /// </summary>
        /// <param name="entityType"> An entity type to project. </param>
        /// <param name="tableExpressionBase"> A table source to project from. </param>
        /// <returns> An expression representing a SELECT in a SQL tree. </returns>
        SelectExpression Select(IEntityType entityType, TableExpressionBase tableExpressionBase);

        /// <summary>
        ///     Creates a new <see cref="SelectExpression" /> which represents a SELECT in a SQL tree projecting an entity type from
        ///     a table source created using a custom SQL.
        /// </summary>
        /// <param name="entityType"> An entity type to project. </param>
        /// <param name="sql"> A custom SQL for the table source. </param>
        /// <param name="sqlArguments"> An expression representing parameters passed to the custom SQL. </param>
        /// <returns> An expression representing a SELECT in a SQL tree. </returns>
        [Obsolete("Use overload which takes TableExpressionBase by passing FromSqlExpression directly.")]
        SelectExpression Select(IEntityType entityType, string sql, Expression sqlArguments);
    }
}
