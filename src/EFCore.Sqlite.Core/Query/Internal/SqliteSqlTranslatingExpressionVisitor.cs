// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
{
    private readonly QueryCompilationContext _queryCompilationContext;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    private static readonly MethodInfo StringStartsWithMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(string)])!;

    private static readonly MethodInfo StringEndsWithMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [typeof(string)])!;

    private static readonly MethodInfo EscapeLikePatternParameterMethod =
        typeof(SqliteSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ConstructLikePatternParameter))!;

    private const char LikeEscapeChar = '\\';
    private const string LikeEscapeString = "\\";

    private static readonly IReadOnlyDictionary<ExpressionType, IReadOnlyCollection<Type>> RestrictedBinaryExpressions
        = new Dictionary<ExpressionType, IReadOnlyCollection<Type>>
        {
            [ExpressionType.Add] = new HashSet<Type>
            {
                typeof(DateOnly),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeOnly),
                typeof(TimeSpan)
            },
            [ExpressionType.Divide] = new HashSet<Type>
            {
                typeof(TimeOnly),
                typeof(TimeSpan),
                typeof(ulong)
            },
            [ExpressionType.GreaterThan] = new HashSet<Type>
            {
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(ulong)
            },
            [ExpressionType.GreaterThanOrEqual] = new HashSet<Type>
            {
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(ulong)
            },
            [ExpressionType.LessThan] = new HashSet<Type>
            {
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(ulong)
            },
            [ExpressionType.LessThanOrEqual] = new HashSet<Type>
            {
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(ulong)
            },
            [ExpressionType.Modulo] = new HashSet<Type> { typeof(ulong) },
            [ExpressionType.Multiply] = new HashSet<Type>
            {
                typeof(TimeOnly),
                typeof(TimeSpan),
                typeof(ulong)
            },
            [ExpressionType.Subtract] = new HashSet<Type>
            {
                typeof(DateOnly),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeOnly),
                typeof(TimeSpan)
            }
        };

    private static readonly IReadOnlyDictionary<Type, string> ModuloFunctions = new Dictionary<Type, string>
    {
        { typeof(decimal), "ef_mod" },
        { typeof(double), "mod" },
        { typeof(float), "mod" }
    };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteSqlTranslatingExpressionVisitor(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
        QueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
        : base(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
    {
        _queryCompilationContext = queryCompilationContext;
        _sqlExpressionFactory = dependencies.SqlExpressionFactory;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        if (unaryExpression.NodeType == ExpressionType.ArrayLength
            && unaryExpression.Operand.Type == typeof(byte[]))
        {
            return Visit(unaryExpression.Operand) is SqlExpression sqlExpression
                ? Dependencies.SqlExpressionFactory.Function(
                    "length",
                    new[] { sqlExpression },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true },
                    typeof(int))
                : QueryCompilationContext.NotTranslatedExpression;
        }

        var visitedExpression = base.VisitUnary(unaryExpression);
        if (visitedExpression == QueryCompilationContext.NotTranslatedExpression)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        if (visitedExpression is SqlUnaryExpression { OperatorType: ExpressionType.Negate } sqlUnary)
        {
            var operandType = GetProviderType(sqlUnary.Operand);
            if (operandType == typeof(decimal))
            {
                return Dependencies.SqlExpressionFactory.Function(
                    name: "ef_negate",
                    new[] { sqlUnary.Operand },
                    nullable: true,
                    new[] { true },
                    visitedExpression.Type);
            }

            if (operandType == typeof(TimeOnly)
                || operandType == typeof(TimeSpan))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }

        return visitedExpression;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        // See issue#16428
        //if (binaryExpression.NodeType == ExpressionType.ArrayIndex
        //    && binaryExpression.Left.Type == typeof(byte[]))
        //{
        //    var left = Visit(binaryExpression.Left);
        //    var right = Visit(binaryExpression.Right);

        //    if (left is SqlExpression leftSql
        //        && right is SqlExpression rightSql)
        //    {
        //        return Dependencies.SqlExpressionFactory.Function(
        //            "unicode",
        //            new SqlExpression[]
        //            {
        //                Dependencies.SqlExpressionFactory.Function(
        //                    "substr",
        //                    new SqlExpression[]
        //                    {
        //                        leftSql,
        //                        Dependencies.SqlExpressionFactory.Add(
        //                            Dependencies.SqlExpressionFactory.ApplyDefaultTypeMapping(rightSql),
        //                            Dependencies.SqlExpressionFactory.Constant(1)),
        //                        Dependencies.SqlExpressionFactory.Constant(1)
        //                    },
        //                    nullable: true,
        //                    argumentsPropagateNullability: new[] { true, true, true },
        //                    typeof(byte[]))
        //            },
        //            nullable: true,
        //            argumentsPropagateNullability: new[] { true },
        //            binaryExpression.Type);
        //    }
        //}

        if (!(base.VisitBinary(binaryExpression) is SqlExpression visitedExpression))
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        if (visitedExpression is SqlBinaryExpression sqlBinary)
        {
            if (sqlBinary.OperatorType == ExpressionType.Modulo
                && (ModuloFunctions.TryGetValue(GetProviderType(sqlBinary.Left), out var function)
                    || ModuloFunctions.TryGetValue(GetProviderType(sqlBinary.Right), out function)))
            {
                return Dependencies.SqlExpressionFactory.Function(
                    function,
                    new[] { sqlBinary.Left, sqlBinary.Right },
                    nullable: true,
                    argumentsPropagateNullability: new[] { true, true },
                    visitedExpression.Type,
                    visitedExpression.TypeMapping);
            }

            if (AttemptDecimalCompare(sqlBinary))
            {
                return DoDecimalCompare(visitedExpression, sqlBinary.OperatorType, sqlBinary.Left, sqlBinary.Right);
            }

            if (AttemptDecimalArithmetic(sqlBinary))
            {
                return DoDecimalArithmetics(visitedExpression, sqlBinary.OperatorType, sqlBinary.Left, sqlBinary.Right);
            }

            if (RestrictedBinaryExpressions.TryGetValue(sqlBinary.OperatorType, out var restrictedTypes)
                && (restrictedTypes.Contains(GetProviderType(sqlBinary.Left))
                    || restrictedTypes.Contains(GetProviderType(sqlBinary.Right))))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }
        }

        return visitedExpression;
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var method = methodCallExpression.Method;

        if (method == StringStartsWithMethodInfo
            && TryTranslateStartsEndsWith(
                methodCallExpression.Object!, methodCallExpression.Arguments[0], startsWith: true, out var translation1))
        {
            return translation1;
        }

        if (method == StringEndsWithMethodInfo
            && TryTranslateStartsEndsWith(
                methodCallExpression.Object!, methodCallExpression.Arguments[0], startsWith: false, out var translation2))
        {
            return translation2;
        }

        return base.VisitMethodCall(methodCallExpression);

        bool TryTranslateStartsEndsWith(
            Expression instance,
            Expression pattern,
            bool startsWith,
            [NotNullWhen(true)] out SqlExpression? translation)
        {
            if (Visit(instance) is not SqlExpression translatedInstance
                || Visit(pattern) is not SqlExpression translatedPattern)
            {
                translation = null;
                return false;
            }

            var stringTypeMapping = ExpressionExtensions.InferTypeMapping(translatedInstance, translatedPattern);

            translatedInstance = _sqlExpressionFactory.ApplyTypeMapping(translatedInstance, stringTypeMapping);
            translatedPattern = _sqlExpressionFactory.ApplyTypeMapping(translatedPattern, stringTypeMapping);

            switch (translatedPattern)
            {
                case SqlConstantExpression patternConstant:
                {
                    // The pattern is constant. Aside from null and empty string, we escape all special characters (%, _, \) and send a
                    // simple LIKE
                    translation = patternConstant.Value switch
                    {
                        null => _sqlExpressionFactory.Like(
                            translatedInstance,
                            _sqlExpressionFactory.Constant(null, typeof(string), stringTypeMapping)),

                        // In .NET, all strings start with/end with/contain the empty string, but SQL LIKE return false for empty patterns.
                        // Return % which always matches instead.
                        // Note that we don't just return a true constant, since null strings shouldn't match even an empty string
                        // (but SqlNullabilityProcess will convert this to a true constant if the instance is non-nullable)
                        "" => _sqlExpressionFactory.Like(translatedInstance, _sqlExpressionFactory.Constant("%")),

                        string s => s.Any(IsLikeWildChar)
                            ? _sqlExpressionFactory.Like(
                                translatedInstance,
                                _sqlExpressionFactory.Constant(startsWith ? EscapeLikePattern(s) + '%' : '%' + EscapeLikePattern(s)),
                                _sqlExpressionFactory.Constant(LikeEscapeString))
                            : _sqlExpressionFactory.Like(
                                translatedInstance,
                                _sqlExpressionFactory.Constant(startsWith ? s + '%' : '%' + s)),

                        _ => throw new UnreachableException()
                    };

                    return true;
                }

                case SqlParameterExpression patternParameter
                    when patternParameter.Name.StartsWith(QueryCompilationContext.QueryParameterPrefix, StringComparison.Ordinal):
                {
                    // The pattern is a parameter, register a runtime parameter that will contain the rewritten LIKE pattern, where
                    // all special characters have been escaped.
                    var lambda = Expression.Lambda(
                        Expression.Call(
                            EscapeLikePatternParameterMethod,
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(patternParameter.Name),
                            Expression.Constant(startsWith)),
                        QueryCompilationContext.QueryContextParameter);

                    var escapedPatternParameter =
                        _queryCompilationContext.RegisterRuntimeParameter(
                            $"{patternParameter.Name}_{(startsWith ? "startswith" : "endswith")}", lambda);

                    translation = _sqlExpressionFactory.Like(
                        translatedInstance,
                        new SqlParameterExpression(escapedPatternParameter.Name!, escapedPatternParameter.Type, stringTypeMapping),
                        _sqlExpressionFactory.Constant(LikeEscapeString));

                    return true;
                }

                default:
                    // The pattern is a column or a complex expression; the possible special characters in the pattern cannot be escaped,
                    // preventing us from translating to LIKE.
                    if (startsWith)
                    {
                        // Generate: WHERE instance IS NOT NULL AND pattern IS NOT NULL AND (substr(instance, 1, length(pattern)) = pattern OR pattern = '')
                        // Note that the empty string pattern needs special handling, since in .NET it returns true for all non-null
                        // instances, but substr(instance, 0) returns the entire string in SQLite.
                        // Note that we compensate for the case where both the instance and the pattern are null (null.StartsWith(null)); a
                        // simple equality would yield true in that case, but we want false. We technically
                        translation = _sqlExpressionFactory.AndAlso(
                            _sqlExpressionFactory.IsNotNull(translatedInstance),
                            _sqlExpressionFactory.AndAlso(
                                _sqlExpressionFactory.IsNotNull(translatedPattern),
                                _sqlExpressionFactory.OrElse(
                                    _sqlExpressionFactory.Equal(
                                        _sqlExpressionFactory.Function(
                                            "substr",
                                            new[]
                                            {
                                                translatedInstance,
                                                _sqlExpressionFactory.Constant(1),
                                                _sqlExpressionFactory.Function(
                                                    "length",
                                                    new[] { translatedPattern },
                                                    nullable: true,
                                                    argumentsPropagateNullability: new[] { true },
                                                    typeof(int))
                                            },
                                            nullable: true,
                                            argumentsPropagateNullability: new[] { true, false, true },
                                            typeof(string),
                                            stringTypeMapping),
                                        translatedPattern),
                                    _sqlExpressionFactory.Equal(translatedPattern, _sqlExpressionFactory.Constant(string.Empty)))));
                    }
                    else
                    {
                        // Generate: WHERE instance IS NOT NULL AND pattern IS NOT NULL AND (substr(instance, -length(pattern)) = pattern OR pattern = '')
                        // Note that the empty string pattern needs special handling, since in .NET it returns true for all non-null
                        // instances, but substr(instance, 0) returns the entire string in SQLite.
                        // Note that we compensate for the case where both the instance and the pattern are null (null.StartsWith(null)); a
                        // simple equality would yield true in that case, but we want false. We technically
                        translation =
                            _sqlExpressionFactory.AndAlso(
                                _sqlExpressionFactory.IsNotNull(translatedInstance),
                                _sqlExpressionFactory.AndAlso(
                                    _sqlExpressionFactory.IsNotNull(translatedPattern),
                                    _sqlExpressionFactory.OrElse(
                                        _sqlExpressionFactory.Equal(
                                            _sqlExpressionFactory.Function(
                                                "substr",
                                                new[]
                                                {
                                                    translatedInstance,
                                                    _sqlExpressionFactory.Negate(
                                                        _sqlExpressionFactory.Function(
                                                            "length",
                                                            new[] { translatedPattern },
                                                            nullable: true,
                                                            argumentsPropagateNullability: new[] { true },
                                                            typeof(int)))
                                                },
                                                nullable: true,
                                                argumentsPropagateNullability: new[] { true, true },
                                                typeof(string),
                                                stringTypeMapping),
                                            translatedPattern),
                                        _sqlExpressionFactory.Equal(translatedPattern, _sqlExpressionFactory.Constant(string.Empty)))));
                    }

                    return true;
            }
        }
    }

    private static string? ConstructLikePatternParameter(
        QueryContext queryContext,
        string baseParameterName,
        bool startsWith)
        => queryContext.ParameterValues[baseParameterName] switch
        {
            null => null,

            // In .NET, all strings start/end with the empty string, but SQL LIKE return false for empty patterns.
            // Return % which always matches instead.
            "" => "%",

            string s => startsWith ? EscapeLikePattern(s) + '%' : '%' + EscapeLikePattern(s),

            _ => throw new UnreachableException()
        };

    // See https://www.sqlite.org/lang_expr.html
    private static bool IsLikeWildChar(char c)
        => c is '%' or '_';

    private static string EscapeLikePattern(string pattern)
    {
        var builder = new StringBuilder();
        for (var i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];
            if (IsLikeWildChar(c)
                || c == LikeEscapeChar)
            {
                builder.Append(LikeEscapeChar);
            }

            builder.Append(c);
        }

        return builder.ToString();
    }

    [return: NotNullIfNotNull(nameof(expression))]
    private static Type? GetProviderType(SqlExpression? expression)
        => expression == null
            ? null
            : (expression.TypeMapping?.Converter?.ProviderClrType
                ?? expression.TypeMapping?.ClrType
                ?? expression.Type);

    private static bool AreOperandsDecimals(SqlBinaryExpression sqlExpression)
        => GetProviderType(sqlExpression.Left) == typeof(decimal)
            && GetProviderType(sqlExpression.Right) == typeof(decimal);

    private static bool AttemptDecimalCompare(SqlBinaryExpression sqlBinary)
        => AreOperandsDecimals(sqlBinary)
            && new[]
            {
                ExpressionType.GreaterThan, ExpressionType.GreaterThanOrEqual, ExpressionType.LessThan, ExpressionType.LessThanOrEqual
            }.Contains(sqlBinary.OperatorType);

    private Expression DoDecimalCompare(SqlExpression visitedExpression, ExpressionType op, SqlExpression left, SqlExpression right)
    {
        var actual = Dependencies.SqlExpressionFactory.Function(
            name: "ef_compare",
            new[] { left, right },
            nullable: true,
            new[] { true, true },
            typeof(int));
        var oracle = Dependencies.SqlExpressionFactory.Constant(value: 0);

        return op switch
        {
            ExpressionType.GreaterThan => Dependencies.SqlExpressionFactory.GreaterThan(left: actual, right: oracle),
            ExpressionType.GreaterThanOrEqual => Dependencies.SqlExpressionFactory.GreaterThanOrEqual(left: actual, right: oracle),
            ExpressionType.LessThan => Dependencies.SqlExpressionFactory.LessThan(left: actual, right: oracle),
            ExpressionType.LessThanOrEqual => Dependencies.SqlExpressionFactory.LessThanOrEqual(left: actual, right: oracle),
            _ => visitedExpression
        };
    }

    private static bool AttemptDecimalArithmetic(SqlBinaryExpression sqlBinary)
        => AreOperandsDecimals(sqlBinary)
            && new[] { ExpressionType.Add, ExpressionType.Subtract, ExpressionType.Multiply, ExpressionType.Divide }.Contains(
                sqlBinary.OperatorType);

    private Expression DoDecimalArithmetics(SqlExpression visitedExpression, ExpressionType op, SqlExpression left, SqlExpression right)
    {
        return op switch
        {
            ExpressionType.Add => DecimalArithmeticExpressionFactoryMethod(ResolveFunctionNameFromExpressionType(op), left, right),
            ExpressionType.Divide => DecimalArithmeticExpressionFactoryMethod(ResolveFunctionNameFromExpressionType(op), left, right),
            ExpressionType.Multiply => DecimalArithmeticExpressionFactoryMethod(ResolveFunctionNameFromExpressionType(op), left, right),
            ExpressionType.Subtract => DecimalSubtractExpressionFactoryMethod(left, right),
            _ => visitedExpression
        };

        static string ResolveFunctionNameFromExpressionType(ExpressionType expressionType)
            => expressionType switch
            {
                ExpressionType.Add => "ef_add",
                ExpressionType.Divide => "ef_divide",
                ExpressionType.Multiply => "ef_multiply",
                ExpressionType.Subtract => "ef_add",
                _ => throw new InvalidOperationException()
            };

        Expression DecimalArithmeticExpressionFactoryMethod(string name, SqlExpression left, SqlExpression right)
            => Dependencies.SqlExpressionFactory.Function(
                name,
                new[] { left, right },
                nullable: true,
                new[] { true, true },
                visitedExpression.Type);

        Expression DecimalSubtractExpressionFactoryMethod(SqlExpression left, SqlExpression right)
        {
            var subtrahend = Dependencies.SqlExpressionFactory.Function(
                "ef_negate",
                new[] { right },
                nullable: true,
                new[] { true },
                visitedExpression.Type);

            return DecimalArithmeticExpressionFactoryMethod(ResolveFunctionNameFromExpressionType(op), left, subtrahend);
        }
    }
}
