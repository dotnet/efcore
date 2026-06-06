// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.Query;

/// <summary>
///     A factory for creating <see cref="SqlExpression" /> instances.
/// </summary>
/// <remarks>
///     The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
///     <see cref="DbContext" /> instance will use its own instance of this service.
///     The implementation may depend on other services registered with any lifetime.
///     The implementation does not need to be thread-safe.
/// </remarks>
public interface ISqlExpressionFactory
{
    /// <summary>
    ///     Applies type mapping to the given <see cref="SqlExpression" />.
    /// </summary>
    /// <param name="sqlExpression">A SQL expression to apply type mapping.</param>
    /// <param name="typeMapping">A type mapping to apply.</param>
    /// <returns>A SQL expression with given type mapping applied.</returns>
    [return: NotNullIfNotNull(nameof(sqlExpression))]
    SqlExpression? ApplyTypeMapping(SqlExpression? sqlExpression, RelationalTypeMapping? typeMapping);

    /// <summary>
    ///     Applies default type mapping to given <see cref="SqlExpression" />.
    /// </summary>
    /// <param name="sqlExpression">A SQL Expression to apply default type mapping.</param>
    /// <returns>A SQL expression with default type mapping applied.</returns>
    [return: NotNullIfNotNull(nameof(sqlExpression))]
    SqlExpression? ApplyDefaultTypeMapping(SqlExpression? sqlExpression);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> with the given arguments.
    /// </summary>
    /// <param name="operatorType">An <see cref="ExpressionType" /> representing SQL unary operator.</param>
    /// <param name="operand">A <see cref="SqlExpression" /> to apply unary operator on.</param>
    /// <param name="type">The type of the created expression.</param>
    /// <param name="typeMapping">A type mapping to be assigned to the created expression.</param>
    /// <param name="existingExpression">An optional expression that can be re-used if it matches the new expression.</param>
    /// <returns>A <see cref="SqlExpression" /> with the given arguments.</returns>
    SqlExpression? MakeUnary(
        ExpressionType operatorType,
        SqlExpression operand,
        Type type,
        RelationalTypeMapping? typeMapping = null,
        SqlExpression? existingExpression = null);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> with the given arguments.
    /// </summary>
    /// <param name="operatorType">An <see cref="ExpressionType" /> representing SQL unary operator.</param>
    /// <param name="left">The left operand of binary operation.</param>
    /// <param name="right">The right operand of binary operation.</param>
    /// <param name="typeMapping">A type mapping to be assigned to the created expression.</param>
    /// <param name="existingExpression">An optional expression that can be re-used if it matches the new expression.</param>
    /// <returns>A <see cref="SqlExpression" /> with the given arguments.</returns>
    SqlExpression? MakeBinary(
        ExpressionType operatorType,
        SqlExpression left,
        SqlExpression right,
        RelationalTypeMapping? typeMapping,
        SqlExpression? existingExpression = null);

    // Comparison
    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents an equality comparison.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>An expression representing a SQL equality comparison.</returns>
    SqlExpression Equal(SqlExpression left, SqlExpression right);

    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents an inequality comparison.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>An expression representing a SQL inequality comparison.</returns>
    SqlExpression NotEqual(SqlExpression left, SqlExpression right);

    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents a greater than comparison.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>An expression representing a SQL greater than comparison.</returns>
    SqlExpression GreaterThan(SqlExpression left, SqlExpression right);

    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents a greater than or equal comparison.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>An expression representing a SQL greater than or equal comparison.</returns>
    SqlExpression GreaterThanOrEqual(SqlExpression left, SqlExpression right);

    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents a less than comparison.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>An expression representing a SQL less than comparison.</returns>
    SqlExpression LessThan(SqlExpression left, SqlExpression right);

    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents a less than or equal comparison.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>An expression representing a SQL less than or equal comparison.</returns>
    SqlExpression LessThanOrEqual(SqlExpression left, SqlExpression right);

    // Logical
    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents a logical AND operation.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>An expression representing a SQL AND operation.</returns>
    SqlExpression AndAlso(SqlExpression left, SqlExpression right);

    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents a logical OR operation.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <returns>An expression representing a SQL OR operation.</returns>
    SqlExpression OrElse(SqlExpression left, SqlExpression right);

    // Arithmetic
    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents an addition.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="typeMapping">A type mapping to be assigned to the created expression.</param>
    /// <returns>An expression representing a SQL addition.</returns>
    SqlExpression Add(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents a subtraction.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="typeMapping">A type mapping to be assigned to the created expression.</param>
    /// <returns>An expression representing a SQL subtraction.</returns>
    SqlExpression Subtract(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents a multiplication.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="typeMapping">A type mapping to be assigned to the created expression.</param>
    /// <returns>An expression representing a SQL multiplication.</returns>
    SqlExpression Multiply(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents a division.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="typeMapping">A type mapping to be assigned to the created expression.</param>
    /// <returns>An expression representing a SQL division.</returns>
    SqlExpression Divide(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents a modulo operation.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="typeMapping">A type mapping to be assigned to the created expression.</param>
    /// <returns>An expression representing a SQL modulo operation.</returns>
    SqlExpression Modulo(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null);

    // Bitwise
    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents a bitwise AND operation.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="typeMapping">A type mapping to be assigned to the created expression.</param>
    /// <returns>An expression representing a SQL bitwise AND operation.</returns>
    SqlExpression And(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents a bitwise OR operation.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="typeMapping">A type mapping to be assigned to the created expression.</param>
    /// <returns>An expression representing a SQL bitwise OR operation.</returns>
    SqlExpression Or(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null);

    // Other
    /// <summary>
    ///     Creates a <see cref="SqlExpression" /> which represents a COALESCE operation.
    /// </summary>
    /// <param name="left">The left operand.</param>
    /// <param name="right">The right operand.</param>
    /// <param name="typeMapping">A type mapping to be assigned to the created expression.</param>
    /// <returns>An expression representing a SQL COALESCE operation.</returns>
    SqlExpression Coalesce(SqlExpression left, SqlExpression right, RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represent equality to null.
    /// </summary>
    /// <param name="operand">A <see cref="SqlExpression" /> to compare to null.</param>
    /// <returns>An expression representing IS NULL construct in a SQL tree.</returns>
    SqlExpression IsNull(SqlExpression operand);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represent inequality to null.
    /// </summary>
    /// <param name="operand">A <see cref="SqlExpression" /> to compare to non null.</param>
    /// <returns>An expression representing IS NOT NULL construct in a SQL tree.</returns>
    SqlExpression IsNotNull(SqlExpression operand);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represent casting a SQL expression to different type.
    /// </summary>
    /// <param name="operand">A <see cref="SqlExpression" /> to cast.</param>
    /// <param name="type">The return type of the expression after cast.</param>
    /// <param name="typeMapping">A relational type mapping to use for conversion.</param>
    /// <returns>An expression representing cast operation in a SQL tree.</returns>
    SqlExpression Convert(SqlExpression operand, Type type, RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represent a NOT operation in a SQL tree.
    /// </summary>
    /// <param name="operand">A <see cref="SqlExpression" /> to apply NOT on.</param>
    /// <returns>An expression representing a NOT operation in a SQL tree.</returns>
    SqlExpression Not(SqlExpression operand);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represent a negation operation in a SQL tree.
    /// </summary>
    /// <param name="operand">A <see cref="SqlExpression" /> to apply NOT on.</param>
    /// <returns>An expression representing a negation operation in a SQL tree.</returns>
    SqlExpression Negate(SqlExpression operand);

    /// <summary>
    ///     Creates a new <see cref="CaseExpression" /> which represent a CASE statement in a SQL tree.
    /// </summary>
    /// <param name="operand">An expression to compare with <see cref="CaseWhenClause.Test" /> in <paramref name="whenClauses" />.</param>
    /// <param name="whenClauses">A list of <see cref="CaseWhenClause" /> to compare or evaluate and get result from.</param>
    /// <param name="elseResult">A value to return if no <paramref name="whenClauses" /> matches, if any.</param>
    /// <param name="existingExpression">An optional expression that can be re-used if it matches the new expression.</param>
    /// <returns>An expression representing a CASE statement in a SQL tree.</returns>
    SqlExpression Case(
        SqlExpression? operand,
        IReadOnlyList<CaseWhenClause> whenClauses,
        SqlExpression? elseResult,
        SqlExpression? existingExpression = null);

    /// <summary>
    ///     Creates a new <see cref="CaseExpression" /> which represent a CASE statement in a SQL tree.
    /// </summary>
    /// <param name="whenClauses">A list of <see cref="CaseWhenClause" /> to evaluate condition and get result from.</param>
    /// <param name="elseResult">A value to return if no <paramref name="whenClauses" /> matches, if any.</param>
    /// <returns>An expression representing a CASE statement in a SQL tree.</returns>
    SqlExpression Case(IReadOnlyList<CaseWhenClause> whenClauses, SqlExpression? elseResult);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represents a function call in a SQL tree.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <param name="arguments">The arguments of the function.</param>
    /// <param name="nullable">A bool value indicating whether this function can return null.</param>
    /// <param name="argumentsPropagateNullability">A list of bool values indicating whether individual arguments propagate null to result.</param>
    /// <param name="returnType">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <returns>An expression representing a function call in a SQL tree.</returns>
    SqlExpression Function(
        string name,
        IEnumerable<SqlExpression> arguments,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represents a function call in a SQL tree.
    /// </summary>
    /// <param name="schema">The schema in which the function is defined.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="arguments">The arguments of the function.</param>
    /// <param name="nullable">A bool value indicating whether this function can return null.</param>
    /// <param name="argumentsPropagateNullability">A list of bool values indicating whether individual arguments propagate null to result.</param>
    /// <param name="returnType">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <returns>An expression representing a function call in a SQL tree.</returns>
    SqlExpression Function(
        string? schema,
        string name,
        IEnumerable<SqlExpression> arguments,
        bool nullable,
        IEnumerable<bool> argumentsPropagateNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represents a function call in a SQL tree.
    /// </summary>
    /// <param name="instance">An expression on which the function is applied.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="arguments">The arguments of the function.</param>
    /// <param name="nullable">A bool value indicating whether this function can return null.</param>
    /// <param name="instancePropagatesNullability">A value indicating if instance propagates null to result.</param>
    /// <param name="argumentsPropagateNullability">A list of bool values indicating whether individual arguments propagate null to result.</param>
    /// <param name="returnType">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <returns>An expression representing a function call in a SQL tree.</returns>
    SqlExpression Function(
        SqlExpression instance,
        string name,
        IEnumerable<SqlExpression> arguments,
        bool nullable,
        bool instancePropagatesNullability,
        IEnumerable<bool> argumentsPropagateNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represents a niladic function call in a SQL tree.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <param name="nullable">A bool value indicating whether this function can return null.</param>
    /// <param name="returnType">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <returns>An expression representing a function call in a SQL tree.</returns>
    SqlExpression NiladicFunction(
        string name,
        bool nullable,
        Type returnType,
        RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represents a niladic function call in a SQL tree.
    /// </summary>
    /// <param name="schema">The schema in which the function is defined.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="nullable">A bool value indicating whether this function can return null.</param>
    /// <param name="returnType">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <returns>An expression representing a function call in a SQL tree.</returns>
    SqlExpression NiladicFunction(
        string schema,
        string name,
        bool nullable,
        Type returnType,
        RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represents a niladic function call in a SQL tree.
    /// </summary>
    /// <param name="instance">An expression on which the function is applied.</param>
    /// <param name="name">The name of the function.</param>
    /// <param name="nullable">A bool value indicating whether this function can return null.</param>
    /// <param name="instancePropagatesNullability">A value indicating if instance propagates null to result.</param>
    /// <param name="returnType">The <see cref="Type" /> of the expression.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <returns>An expression representing a function call in a SQL tree.</returns>
    SqlExpression NiladicFunction(
        SqlExpression instance,
        string name,
        bool nullable,
        bool instancePropagatesNullability,
        Type returnType,
        RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a new <see cref="ExistsExpression" /> which represents an EXISTS operation in a SQL tree.
    /// </summary>
    /// <param name="subquery">A subquery to check existence of.</param>
    /// <returns>An expression representing an EXISTS operation in a SQL tree.</returns>
    SqlExpression Exists(SelectExpression subquery);

    /// <summary>
    ///     Creates a new <see cref="InExpression" /> which represents an IN operation in a SQL tree.
    /// </summary>
    /// <param name="item">An item to look into values.</param>
    /// <param name="subquery">A subquery in which item is searched.</param>
    /// <returns>An expression representing an IN operation in a SQL tree.</returns>
    SqlExpression In(SqlExpression item, SelectExpression subquery);

    /// <summary>
    ///     Creates a new <see cref="InExpression" /> which represents an IN operation in a SQL tree.
    /// </summary>
    /// <param name="item">An item to look into values.</param>
    /// <param name="values">A list of values in which item is searched.</param>
    /// <returns>An expression representing an IN operation in a SQL tree.</returns>
    SqlExpression In(SqlExpression item, IReadOnlyList<SqlExpression> values);

    /// <summary>
    ///     Creates a new <see cref="InExpression" /> which represents an IN operation in a SQL tree.
    /// </summary>
    /// <param name="item">An item to look into values.</param>
    /// <param name="valuesParameter">A parameterized list of values in which the item is searched.</param>
    /// <returns>An expression representing an IN operation in a SQL tree.</returns>
    SqlExpression In(SqlExpression item, SqlParameterExpression valuesParameter);

    /// <summary>
    ///     Creates a new <see cref="InExpression" /> which represents a LIKE in a SQL tree.
    /// </summary>
    /// <param name="match">An expression on which LIKE is applied.</param>
    /// <param name="pattern">A pattern to search.</param>
    /// <param name="escapeChar">An optional escape character to use in LIKE.</param>
    /// <returns>An expression representing a LIKE in a SQL tree.</returns>
    SqlExpression Like(SqlExpression match, SqlExpression pattern, SqlExpression? escapeChar = null);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represents a constant in a SQL tree.
    /// </summary>
    /// <param name="value">A value.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <returns>An expression representing a constant in a SQL tree.</returns>
    SqlExpression Constant(object value, RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represents a constant in a SQL tree.
    /// </summary>
    /// <param name="value">A value.</param>
    /// <param name="type">The type for the constant. Useful when value is null.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <returns>An expression representing a constant in a SQL tree.</returns>
    SqlExpression Constant(object? value, Type type, RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represents a constant in a SQL tree.
    /// </summary>
    /// <param name="value">A value.</param>
    /// <param name="sensitive"><see langword="true" /> if the expression contains sensitive values; otherwise, <see langword="false" />.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <returns>An expression representing a constant in a SQL tree.</returns>
    SqlExpression Constant(object value, bool sensitive, RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represents a constant in a SQL tree.
    /// </summary>
    /// <param name="value">A value.</param>
    /// <param name="type">The type for the constant. Useful when value is null.</param>
    /// <param name="sensitive"><see langword="true" /> if the expression contains sensitive values; otherwise, <see langword="false" />.</param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <returns>An expression representing a constant in a SQL tree.</returns>
    SqlExpression Constant(object? value, Type type, bool sensitive, RelationalTypeMapping? typeMapping = null);

    /// <summary>
    ///     Creates a new <see cref="SqlExpression" /> which represents a SQL token.
    /// </summary>
    /// <param name="sql">A string token to print in SQL tree.</param>
    /// <param name="type">The <see cref="Type" /> of the expression. Defaults to <see langword="void" />. </param>
    /// <param name="typeMapping">The <see cref="RelationalTypeMapping" /> associated with the expression.</param>
    /// <returns>An expression representing a SQL token.</returns>
    SqlExpression Fragment(string sql, Type? type = null, RelationalTypeMapping? typeMapping = null);
}
