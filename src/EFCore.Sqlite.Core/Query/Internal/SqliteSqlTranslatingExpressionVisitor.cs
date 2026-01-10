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
public class SqliteSqlTranslatingExpressionVisitor(
    RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
    QueryCompilationContext queryCompilationContext,
    QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor)
    : RelationalSqlTranslatingExpressionVisitor(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
{
    private readonly QueryCompilationContext _queryCompilationContext = queryCompilationContext;
    private readonly ISqlExpressionFactory _sqlExpressionFactory = dependencies.SqlExpressionFactory;

    private static readonly MethodInfo StringStartsWithMethodInfoString
        = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(string)])!;

    private static readonly MethodInfo StringStartsWithMethodInfoChar
        = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(char)])!;

    private static readonly MethodInfo StringEndsWithMethodInfoString
        = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [typeof(string)])!;

    private static readonly MethodInfo StringEndsWithMethodInfoChar
        = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [typeof(char)])!;

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
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        var translation = base.VisitUnary(unaryExpression);

        if (translation is SqlUnaryExpression { OperatorType: ExpressionType.Negate } sqlUnary)
        {
            return GetProviderType(sqlUnary.Operand) switch
            {
                var t when t == typeof(decimal)
                    => Dependencies.SqlExpressionFactory.Function(
                    name: "ef_negate",
                    [sqlUnary.Operand],
                    nullable: true,
                    [true],
                    translation.Type),

                var t when t == typeof(TimeOnly) || t == typeof(TimeSpan)
                    => QueryCompilationContext.NotTranslatedExpression,

                _ => translation
            };
        }

        if (translation == QueryCompilationContext.NotTranslatedExpression
            && unaryExpression.NodeType == ExpressionType.ArrayLength
            && unaryExpression.Operand.Type == typeof(byte[]))
        {
            return Visit(unaryExpression.Operand) is SqlExpression sqlExpression
                ? Dependencies.SqlExpressionFactory.Function(
                    "length",
                    [sqlExpression],
                    nullable: true,
                    argumentsPropagateNullability: Statics.TrueArrays[1],
                    typeof(int))
                : QueryCompilationContext.NotTranslatedExpression;
        }

        return translation;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        if (base.VisitBinary(binaryExpression) is not SqlExpression translation)
        {
            return QueryCompilationContext.NotTranslatedExpression;
        }

        if (translation is SqlBinaryExpression sqlBinary)
        {
            switch (sqlBinary)
            {
                case { OperatorType: ExpressionType.ExclusiveOr }:
                    return QueryCompilationContext.NotTranslatedExpression;

                case { OperatorType: ExpressionType.Modulo }
                    when ModuloFunctions.TryGetValue(GetProviderType(sqlBinary.Left), out var function)
                        || ModuloFunctions.TryGetValue(GetProviderType(sqlBinary.Right), out function):
                {
                    return Dependencies.SqlExpressionFactory.Function(
                        function,
                        [sqlBinary.Left, sqlBinary.Right],
                        nullable: true,
                        argumentsPropagateNullability: Statics.FalseArrays[2],
                        translation.Type,
                        translation.TypeMapping);
                }

                case { } when AttemptDecimalCompare(sqlBinary):
                    return DoDecimalCompare(translation, sqlBinary.OperatorType, sqlBinary.Left, sqlBinary.Right);

                case { } when AttemptDecimalArithmetic(sqlBinary):
                    return DoDecimalArithmetics(translation, sqlBinary.OperatorType, sqlBinary.Left, sqlBinary.Right);

                case { }
                    when RestrictedBinaryExpressions.TryGetValue(sqlBinary.OperatorType, out var restrictedTypes)
                        && (restrictedTypes.Contains(GetProviderType(sqlBinary.Left))
                            || restrictedTypes.Contains(GetProviderType(sqlBinary.Right))):
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }
            }
        }

        return translation;
    }

    /// <inheritdoc />
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        if (base.VisitMethodCall(methodCallExpression) is var translation
            && translation != QueryCompilationContext.NotTranslatedExpression)
        {
            return translation;
        }

        var method = methodCallExpression.Method;
        var declaringType = method.DeclaringType;
        var @object = methodCallExpression.Object;
        var arguments = methodCallExpression.Arguments;

        switch (method.Name)
        {
            // https://learn.microsoft.com/dotnet/api/system.string.startswith#system-string-startswith(system-string)
            // https://learn.microsoft.com/dotnet/api/system.string.startswith#system-string-startswith(system-char)
            // https://learn.microsoft.com/dotnet/api/system.string.endswith#system-string-endswith(system-string)
            // https://learn.microsoft.com/dotnet/api/system.string.endswith#system-string-endswith(system-char)
            case nameof(string.StartsWith) or nameof(string.EndsWith)
                when methodCallExpression.Object is not null
                    && declaringType == typeof(string)
                    && arguments is [Expression value]
                    && (value.Type == typeof(string) || value.Type == typeof(char)):
            {
                return TranslateStartsEndsWith(
                    methodCallExpression.Object,
                    value,
                    method.Name is nameof(string.StartsWith));
            }

            // We translate EF.Functions.JsonExists here and not in a method translator since we need to support JsonExists over a complex
            // property, which requires special handling.
            case nameof(RelationalDbFunctionsExtensions.JsonExists)
                when declaringType == typeof(RelationalDbFunctionsExtensions)
                    && @object is null
                    && arguments is [_, var json, var path]:
            {
                if (Translate(path) is not SqlExpression translatedPath)
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

#pragma warning disable EF1001 // TranslateProjection() is pubternal
                var translatedJson = TranslateProjection(json) switch
                {
                    // The JSON argument is a scalar string property
                    SqlExpression scalar => scalar,

                    // The JSON argument is a complex JSON property
                    RelationalStructuralTypeShaperExpression { ValueBufferExpression: JsonQueryExpression { JsonColumn: var c } } => c,
                    _ => null
                };
#pragma warning restore EF1001

                return translatedJson is null
                    ? QueryCompilationContext.NotTranslatedExpression
                    : _sqlExpressionFactory.IsNotNull(
                        _sqlExpressionFactory.Function(
                            "json_type",
                            [translatedJson, translatedPath],
                            nullable: true,
                            // Note that json_type() does propagate nullability; however, our query pipeline assumes that if arguments
                            // propagate nullability, that's the *only* reason for the function to return null; this means that if the
                            // arguments are non-nullable, the IS NOT NULL wrapping check can be optimized away.
                            argumentsPropagateNullability: [false, false],
                            typeof(int)));
            }
        }

        return QueryCompilationContext.NotTranslatedExpression;

        Expression TranslateStartsEndsWith(Expression instance, Expression pattern, bool startsWith)
        {
            if (Visit(instance) is not SqlExpression translatedInstance
                || Visit(pattern) is not SqlExpression translatedPattern)
            {
                return QueryCompilationContext.NotTranslatedExpression;
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
                    return patternConstant.Value switch
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

                        char s => IsLikeWildChar(s)
                            ? _sqlExpressionFactory.Like(
                                translatedInstance,
                                _sqlExpressionFactory.Constant(startsWith ? LikeEscapeString + s + "%" : '%' + LikeEscapeString + s),
                                _sqlExpressionFactory.Constant(LikeEscapeString))
                            : _sqlExpressionFactory.Like(
                                translatedInstance,
                                _sqlExpressionFactory.Constant(startsWith ? s + "%" : "%" + s)),

                        _ => throw new UnreachableException()
                    };
                }

                case SqlParameterExpression patternParameter:
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

                    return _sqlExpressionFactory.Like(
                        translatedInstance,
                        new SqlParameterExpression(escapedPatternParameter.Name!, escapedPatternParameter.Type, stringTypeMapping),
                        _sqlExpressionFactory.Constant(LikeEscapeString));
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
                        return
                            _sqlExpressionFactory.AndAlso(
                                _sqlExpressionFactory.IsNotNull(translatedInstance),
                                _sqlExpressionFactory.AndAlso(
                                    _sqlExpressionFactory.IsNotNull(translatedPattern),
                                    _sqlExpressionFactory.OrElse(
                                        _sqlExpressionFactory.Equal(
                                            _sqlExpressionFactory.Function(
                                                "substr",
                                                [
                                                    translatedInstance,
                                                    _sqlExpressionFactory.Constant(1),
                                                    _sqlExpressionFactory.Function(
                                                        "length",
                                                        [translatedPattern],
                                                        nullable: true,
                                                        argumentsPropagateNullability: Statics.TrueArrays[1],
                                                        typeof(int))
                                                ],
                                                nullable: true,
                                                argumentsPropagateNullability: [true, false, false],
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
                        return
                            _sqlExpressionFactory.AndAlso(
                                _sqlExpressionFactory.IsNotNull(translatedInstance),
                                _sqlExpressionFactory.AndAlso(
                                    _sqlExpressionFactory.IsNotNull(translatedPattern),
                                    _sqlExpressionFactory.OrElse(
                                        _sqlExpressionFactory.Equal(
                                            _sqlExpressionFactory.Function(
                                                "substr",
                                                [
                                                    translatedInstance,
                                                    _sqlExpressionFactory.Negate(
                                                        _sqlExpressionFactory.Function(
                                                            "length",
                                                            [translatedPattern],
                                                            nullable: true,
                                                            argumentsPropagateNullability: Statics.TrueArrays[1],
                                                            typeof(int)))
                                                ],
                                                nullable: true,
                                                argumentsPropagateNullability: Statics.TrueArrays[2],
                                                typeof(string),
                                                stringTypeMapping),
                                            translatedPattern),
                                        _sqlExpressionFactory.Equal(translatedPattern, _sqlExpressionFactory.Constant(string.Empty)))));
                    }
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal] // Can be called from precompiled shapers
    public static string? ConstructLikePatternParameter(
        QueryContext queryContext,
        string baseParameterName,
        bool startsWith)
        => queryContext.Parameters[baseParameterName] switch
        {
            null => null,

            // In .NET, all strings start/end with the empty string, but SQL LIKE return false for empty patterns.
            // Return % which always matches instead.
            "" => "%",

            string s => startsWith ? EscapeLikePattern(s) + '%' : '%' + EscapeLikePattern(s),

            char s when IsLikeWildChar(s) => startsWith ? LikeEscapeString + s + '%' : '%' + LikeEscapeString + s,

            char s => startsWith ? s + "%" : "%" + s,

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

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override SqlExpression GenerateGreatest(IReadOnlyList<SqlExpression> expressions, Type resultType)
    {
        // Docs: https://sqlite.org/lang_corefunc.html#max_scalar
        var resultTypeMapping = ExpressionExtensions.InferTypeMapping(expressions);

        // The multi-argument max() function returns the argument with the maximum value, or return NULL if any argument is NULL.
        return _sqlExpressionFactory.Function(
            "max", expressions, nullable: true, Enumerable.Repeat(true, expressions.Count), resultType, resultTypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override SqlExpression GenerateLeast(IReadOnlyList<SqlExpression> expressions, Type resultType)
    {
        // Docs: https://sqlite.org/lang_corefunc.html#min_scalar
        var resultTypeMapping = ExpressionExtensions.InferTypeMapping(expressions);

        // The multi-argument min() function returns the argument with the minimum value, or return NULL if any argument is NULL.
        return _sqlExpressionFactory.Function(
            "min", expressions, nullable: true, Enumerable.Repeat(true, expressions.Count), resultType, resultTypeMapping);
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

    private SqlExpression DoDecimalCompare(SqlExpression visitedExpression, ExpressionType op, SqlExpression left, SqlExpression right)
    {
        var actual = Dependencies.SqlExpressionFactory.Function(
            name: "ef_compare",
            [left, right],
            nullable: true,
            [true, true],
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
            ExpressionType.Divide => DecimalDivisionExpressionFactoryMethod(ResolveFunctionNameFromExpressionType(op), left, right),
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
                [left, right],
                nullable: true,
                [true, true],
                visitedExpression.Type);

        Expression DecimalDivisionExpressionFactoryMethod(string name, SqlExpression left, SqlExpression right)
            => Dependencies.SqlExpressionFactory.Function(
                name,
                [left, right],
                nullable: true,
                [false, false],
                visitedExpression.Type);

        Expression DecimalSubtractExpressionFactoryMethod(SqlExpression left, SqlExpression right)
        {
            var subtrahend = Dependencies.SqlExpressionFactory.Function(
                "ef_negate",
                [right],
                nullable: true,
                [true],
                visitedExpression.Type);

            return DecimalArithmeticExpressionFactoryMethod(ResolveFunctionNameFromExpressionType(op), left, subtrahend);
        }
    }
}
