// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.SqlServer.Infrastructure.Internal;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerSqlTranslatingExpressionVisitor : RelationalSqlTranslatingExpressionVisitor
{
    private readonly SqlServerQueryCompilationContext _queryCompilationContext;
    private readonly ISqlExpressionFactory _sqlExpressionFactory;
    private readonly IRelationalTypeMappingSource _typeMappingSource;
    private readonly ISqlServerSingletonOptions _sqlServerSingletonOptions;

    private static readonly HashSet<string> DateTimeDataTypes
        =
        [
            "time",
            "date",
            "datetime",
            "datetime2",
            "datetimeoffset"
        ];

    private static readonly HashSet<Type> DateTimeClrTypes
        =
        [
            typeof(TimeOnly),
            typeof(DateOnly),
            typeof(TimeSpan),
            typeof(DateTime),
            typeof(DateTimeOffset)
        ];

    private static readonly HashSet<ExpressionType> ArithmeticOperatorTypes
        =
        [
            ExpressionType.Add,
            ExpressionType.Subtract,
            ExpressionType.Multiply,
            ExpressionType.Divide,
            ExpressionType.Modulo
        ];

    private static readonly MethodInfo StringStartsWithMethodInfoString
        = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(string)])!;

    private static readonly MethodInfo StringStartsWithMethodInfoChar
        = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(char)])!;

    private static readonly MethodInfo StringEndsWithMethodInfoString
        = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [typeof(string)])!;

    private static readonly MethodInfo StringEndsWithMethodInfoChar
        = typeof(string).GetRuntimeMethod(nameof(string.EndsWith), [typeof(char)])!;

    private static readonly MethodInfo StringContainsMethodInfoString
        = typeof(string).GetRuntimeMethod(nameof(string.Contains), [typeof(string)])!;

    private static readonly MethodInfo StringContainsMethodInfoChar
        = typeof(string).GetRuntimeMethod(nameof(string.Contains), [typeof(char)])!;

    private static readonly MethodInfo StringJoinMethodInfo
        = typeof(string).GetRuntimeMethod(nameof(string.Join), [typeof(string), typeof(string[])])!;

    private static readonly MethodInfo EscapeLikePatternParameterMethod =
        typeof(SqlServerSqlTranslatingExpressionVisitor).GetTypeInfo().GetDeclaredMethod(nameof(ConstructLikePatternParameter))!;

    private const char LikeEscapeChar = '\\';
    private const string LikeEscapeString = "\\";

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerSqlTranslatingExpressionVisitor(
        RelationalSqlTranslatingExpressionVisitorDependencies dependencies,
        SqlServerQueryCompilationContext queryCompilationContext,
        QueryableMethodTranslatingExpressionVisitor queryableMethodTranslatingExpressionVisitor,
        ISqlServerSingletonOptions sqlServerSingletonOptions)
        : base(dependencies, queryCompilationContext, queryableMethodTranslatingExpressionVisitor)
    {
        _queryCompilationContext = queryCompilationContext;
        _sqlExpressionFactory = dependencies.SqlExpressionFactory;
        _typeMappingSource = dependencies.TypeMappingSource;
        _sqlServerSingletonOptions = sqlServerSingletonOptions;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitBinary(BinaryExpression binaryExpression)
    {
        if (binaryExpression.NodeType == ExpressionType.ArrayIndex
            && binaryExpression.Left.Type == typeof(byte[]))
        {
            return TranslateByteArrayElementAccess(
                binaryExpression.Left,
                binaryExpression.Right,
                binaryExpression.Type);
        }

        var visitedExpression = base.VisitBinary(binaryExpression);

        if (visitedExpression is SqlBinaryExpression sqlBinaryExpression
            && ArithmeticOperatorTypes.Contains(sqlBinaryExpression.OperatorType))
        {
            var inferredProviderType = GetProviderType(sqlBinaryExpression.Left) ?? GetProviderType(sqlBinaryExpression.Right);
            if (inferredProviderType != null)
            {
                if (DateTimeDataTypes.Contains(inferredProviderType))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }
            }
            else
            {
                var leftType = sqlBinaryExpression.Left.Type;
                var rightType = sqlBinaryExpression.Right.Type;
                if (DateTimeClrTypes.Contains(leftType)
                    || DateTimeClrTypes.Contains(rightType))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }
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
    protected override Expression VisitUnary(UnaryExpression unaryExpression)
    {
        if (unaryExpression.NodeType == ExpressionType.ArrayLength
            && unaryExpression.Operand.Type == typeof(byte[]))
        {
            if (!(base.Visit(unaryExpression.Operand) is SqlExpression sqlExpression))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var isBinaryMaxDataType = GetProviderType(sqlExpression) == "varbinary(max)" || sqlExpression is SqlParameterExpression;
            var dataLengthSqlFunction = Dependencies.SqlExpressionFactory.Function(
                "DATALENGTH",
                new[] { sqlExpression },
                nullable: true,
                argumentsPropagateNullability: Statics.TrueArrays[1],
                isBinaryMaxDataType ? typeof(long) : typeof(int));

            return isBinaryMaxDataType
                ? Dependencies.SqlExpressionFactory.Convert(dataLengthSqlFunction, typeof(int))
                : dataLengthSqlFunction;
        }

        return base.VisitUnary(unaryExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitMethodCall(MethodCallExpression methodCallExpression)
    {
        var method = methodCallExpression.Method;

        if (method.IsGenericMethod
            && method.GetGenericMethodDefinition() == EnumerableMethods.ElementAt
            && methodCallExpression.Arguments[0].Type == typeof(byte[]))
        {
            return TranslateByteArrayElementAccess(
                methodCallExpression.Arguments[0],
                methodCallExpression.Arguments[1],
                methodCallExpression.Type);
        }

        if ((method == StringStartsWithMethodInfoString || method == StringStartsWithMethodInfoChar)
            && TryTranslateStartsEndsWithContains(
                methodCallExpression.Object!, methodCallExpression.Arguments[0], StartsEndsWithContains.StartsWith, out var translation1))
        {
            return translation1;
        }

        if ((method == StringEndsWithMethodInfoString || method == StringEndsWithMethodInfoChar)
            && TryTranslateStartsEndsWithContains(
                methodCallExpression.Object!, methodCallExpression.Arguments[0], StartsEndsWithContains.EndsWith, out var translation2))
        {
            return translation2;
        }

        if ((method == StringContainsMethodInfoString || method == StringContainsMethodInfoChar)
            && TryTranslateStartsEndsWithContains(
                methodCallExpression.Object!, methodCallExpression.Arguments[0], StartsEndsWithContains.Contains, out var translation3))
        {
            return translation3;
        }

        // Translate non-aggregate string.Join to CONCAT_WS (for aggregate string.Join, see SqlServerStringAggregateMethodTranslator)
        if (method == StringJoinMethodInfo
            && methodCallExpression.Arguments[1] is NewArrayExpression newArrayExpression
            && ((_sqlServerSingletonOptions.EngineType == SqlServerEngineType.SqlServer
                    && _sqlServerSingletonOptions.SqlServerCompatibilityLevel >= 140)
                || (_sqlServerSingletonOptions.EngineType == SqlServerEngineType.AzureSql
                    && _sqlServerSingletonOptions.AzureSqlCompatibilityLevel >= 140)
                || (_sqlServerSingletonOptions.EngineType == SqlServerEngineType.AzureSynapse)))
        {
            if (TranslationFailed(methodCallExpression.Arguments[0], Visit(methodCallExpression.Arguments[0]), out var delimiter))
            {
                return QueryCompilationContext.NotTranslatedExpression;
            }

            var arguments = new SqlExpression[newArrayExpression.Expressions.Count + 1];
            arguments[0] = delimiter!;

            var isUnicode = delimiter!.TypeMapping?.IsUnicode == true;

            for (var i = 0; i < newArrayExpression.Expressions.Count; i++)
            {
                var argument = newArrayExpression.Expressions[i];
                if (TranslationFailed(argument, Visit(argument), out var sqlArgument))
                {
                    return QueryCompilationContext.NotTranslatedExpression;
                }

                // CONCAT_WS returns a type with a length that varies based on actual inputs (i.e. the sum of all argument lengths, plus
                // the length needed for the delimiters). We don't know column values (or even parameter values, so we always return max.
                // We do vary return varchar(max) or nvarchar(max) based on whether we saw any nvarchar mapping.
                if (sqlArgument!.TypeMapping?.IsUnicode == true)
                {
                    isUnicode = true;
                }

                // CONCAT_WS filters out nulls, but string.Join treats them as empty strings; so coalesce (which is a no-op for non-nullable
                // arguments).
                arguments[i + 1] = Dependencies.SqlExpressionFactory.Coalesce(sqlArgument, _sqlExpressionFactory.Constant(string.Empty));
            }

            // CONCAT_WS never returns null; a null delimiter is interpreted as an empty string, and null arguments are skipped
            // (but we coalesce them above in any case).
            return Dependencies.SqlExpressionFactory.Function(
                "CONCAT_WS",
                arguments,
                nullable: false,
                argumentsPropagateNullability: new bool[arguments.Length],
                typeof(string),
                _typeMappingSource.FindMapping(isUnicode ? "nvarchar(max)" : "varchar(max)"));
        }

        return base.VisitMethodCall(methodCallExpression);

        bool TryTranslateStartsEndsWithContains(
            Expression instance,
            Expression pattern,
            StartsEndsWithContains methodType,
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

                        string s when !s.Any(IsLikeWildChar)
                            => _sqlExpressionFactory.Like(
                                translatedInstance,
                                _sqlExpressionFactory.Constant(
                                    methodType switch
                                    {
                                        StartsEndsWithContains.StartsWith => s + '%',
                                        StartsEndsWithContains.EndsWith => '%' + s,
                                        StartsEndsWithContains.Contains => $"%{s}%",

                                        _ => throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null)
                                    })),

                        // Azure Synapse does not support ESCAPE clause in LIKE
                        // fallback to translation like with column/expression
                        string when _sqlServerSingletonOptions.EngineType is SqlServerEngineType.AzureSynapse
                            => TranslateWithoutLike(patternIsNonEmptyConstantString: true),

                        string s => _sqlExpressionFactory.Like(
                            translatedInstance,
                            _sqlExpressionFactory.Constant(
                                methodType switch
                                {
                                    StartsEndsWithContains.StartsWith => EscapeLikePattern(s) + '%',
                                    StartsEndsWithContains.EndsWith => '%' + EscapeLikePattern(s),
                                    StartsEndsWithContains.Contains => $"%{EscapeLikePattern(s)}%",

                                    _ => throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null)
                                }),
                            _sqlExpressionFactory.Constant(LikeEscapeString)),

                        char s when !IsLikeWildChar(s)
                            => _sqlExpressionFactory.Like(
                                translatedInstance,
                                _sqlExpressionFactory.Constant(
                                    methodType switch
                                    {
                                        StartsEndsWithContains.StartsWith => s + "%",
                                        StartsEndsWithContains.EndsWith => "%" + s,
                                        StartsEndsWithContains.Contains => $"%{s}%",

                                        _ => throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null)
                                    })),

                        char s => _sqlExpressionFactory.Like(
                            translatedInstance,
                            _sqlExpressionFactory.Constant(
                                methodType switch
                                {
                                    StartsEndsWithContains.StartsWith => LikeEscapeChar + s + "%",
                                    StartsEndsWithContains.EndsWith => "%" + LikeEscapeChar + s,
                                    StartsEndsWithContains.Contains => $"%{LikeEscapeChar}{s}%",

                                    _ => throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null)
                                }),
                            _sqlExpressionFactory.Constant(LikeEscapeString)),

                        _ => throw new UnreachableException()
                    };

                    return true;
                }

                // Azure Synapse does not support ESCAPE clause in LIKE
                // fall through to translation like with column/expression
                case SqlParameterExpression patternParameter
                    when _sqlServerSingletonOptions.EngineType is not SqlServerEngineType.AzureSynapse:
                {
                    // The pattern is a parameter, register a runtime parameter that will contain the rewritten LIKE pattern, where
                    // all special characters have been escaped.
                    var lambda = Expression.Lambda(
                        Expression.Call(
                            EscapeLikePatternParameterMethod,
                            QueryCompilationContext.QueryContextParameter,
                            Expression.Constant(patternParameter.Name),
                            Expression.Constant(methodType)),
                        QueryCompilationContext.QueryContextParameter);

                    var escapedPatternParameter =
                        _queryCompilationContext.RegisterRuntimeParameter(
                            $"{patternParameter.Name}_{methodType.ToString().ToLower(CultureInfo.InvariantCulture)}", lambda);

                    translation = _sqlExpressionFactory.Like(
                        translatedInstance,
                        new SqlParameterExpression(escapedPatternParameter.Name!, escapedPatternParameter.Type, stringTypeMapping),
                        _sqlExpressionFactory.Constant(LikeEscapeString));

                    return true;
                }

                default:
                    // The pattern is a column or a complex expression; the possible special characters in the pattern cannot be escaped,
                    // preventing us from translating to LIKE.
                    translation = TranslateWithoutLike();
                    return true;
            }

            SqlExpression TranslateWithoutLike(bool patternIsNonEmptyConstantString = false)
            {
                return methodType switch
                {
                    // For StartsWith/EndsWith, use LEFT or RIGHT instead to extract substring and compare:
                    // WHERE instance IS NOT NULL AND pattern IS NOT NULL AND LEFT(instance, LEN(pattern)) = pattern
                    // This is less efficient than LIKE (i.e. StartsWith does an index scan instead of seek), but we have no choice.
                    // Note that we compensate for the case where both the instance and the pattern are null (null.StartsWith(null)); a
                    // simple equality would yield true in that case, but we want false. We technically
                    StartsEndsWithContains.StartsWith or StartsEndsWithContains.EndsWith
                        => _sqlExpressionFactory.AndAlso(
                            _sqlExpressionFactory.IsNotNull(translatedInstance),
                            _sqlExpressionFactory.AndAlso(
                                _sqlExpressionFactory.IsNotNull(translatedPattern),
                                _sqlExpressionFactory.Equal(
                                    _sqlExpressionFactory.Function(
                                        methodType is StartsEndsWithContains.StartsWith ? "LEFT" : "RIGHT",
                                        new[]
                                        {
                                                translatedInstance,
                                                _sqlExpressionFactory.Function(
                                                    "LEN",
                                                    new[] { translatedPattern },
                                                    nullable: true,
                                                    argumentsPropagateNullability: Statics.TrueArrays[1],
                                                    typeof(int))
                                        },
                                        nullable: true,
                                        argumentsPropagateNullability: Statics.TrueArrays[2],
                                        typeof(string),
                                        stringTypeMapping),
                                    translatedPattern))),

                    // For Contains, just use CHARINDEX and check if the result is greater than 0.
                    StartsEndsWithContains.Contains when patternIsNonEmptyConstantString
                        => _sqlExpressionFactory.AndAlso(
                            _sqlExpressionFactory.IsNotNull(translatedInstance),
                            CharIndexGreaterThanZero()),

                    // For Contains, just use CHARINDEX and check if the result is greater than 0.
                    // Add a check to return null when the pattern is an empty string (and the string isn't null)
                    StartsEndsWithContains.Contains
                        => _sqlExpressionFactory.AndAlso(
                            _sqlExpressionFactory.IsNotNull(translatedInstance),
                            _sqlExpressionFactory.AndAlso(
                                _sqlExpressionFactory.IsNotNull(translatedPattern),
                                _sqlExpressionFactory.OrElse(
                                    CharIndexGreaterThanZero(),
                                    _sqlExpressionFactory.Like(
                                        translatedPattern,
                                        _sqlExpressionFactory.Constant(string.Empty, stringTypeMapping))))),

                    _ => throw new UnreachableException()
                };

                SqlExpression CharIndexGreaterThanZero()
                    => _sqlExpressionFactory.GreaterThan(
                        _sqlExpressionFactory.Function(
                            "CHARINDEX",
                            new[] { translatedPattern, translatedInstance },
                            nullable: true,
                            argumentsPropagateNullability: Statics.TrueArrays[2],
                            typeof(int)),
                        _sqlExpressionFactory.Constant(0));
            }
        }
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public static string? ConstructLikePatternParameter(
        QueryContext queryContext,
        string baseParameterName,
        StartsEndsWithContains methodType)
        => queryContext.ParameterValues[baseParameterName] switch
        {
            null => null,

            // In .NET, all strings start/end with the empty string, but SQL LIKE return false for empty patterns.
            // Return % which always matches instead.
            "" => "%",

            string s => methodType switch
            {
                StartsEndsWithContains.StartsWith => EscapeLikePattern(s) + '%',
                StartsEndsWithContains.EndsWith => '%' + EscapeLikePattern(s),
                StartsEndsWithContains.Contains => $"%{EscapeLikePattern(s)}%",
                _ => throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null)
            },

            char s when !IsLikeWildChar(s) => methodType switch
            {
                StartsEndsWithContains.StartsWith => s + "%",
                StartsEndsWithContains.EndsWith => "%" + s,
                StartsEndsWithContains.Contains => $"%{s}%",
                _ => throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null)
            },

            char s => methodType switch
            {
                StartsEndsWithContains.StartsWith => LikeEscapeChar + s + "%",
                StartsEndsWithContains.EndsWith => "%" + LikeEscapeChar + s,
                StartsEndsWithContains.Contains => $"%{LikeEscapeChar}{s}%",
                _ => throw new ArgumentOutOfRangeException(nameof(methodType), methodType, null)
            },

            _ => throw new UnreachableException()
        };

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [EntityFrameworkInternal]
    public enum StartsEndsWithContains
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        StartsWith,

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        EndsWith,

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        Contains
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private static bool IsLikeWildChar(char c)
        => c is '%' or '_' or '['; // See https://docs.microsoft.com/en-us/sql/t-sql/language-elements/like-transact-sql

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    private static string EscapeLikePattern(string pattern)
    {
        int i;
        for (i = 0; i < pattern.Length; i++)
        {
            var c = pattern[i];
            if (IsLikeWildChar(c) || c == LikeEscapeChar)
            {
                break;
            }
        }

        if (i == pattern.Length) // No special characters were detected, just return the original pattern string
        {
            return pattern;
        }

        var builder = new StringBuilder(pattern, 0, i, pattern.Length + 10);

        for (; i < pattern.Length; i++)
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
    public override SqlExpression? GenerateGreatest(IReadOnlyList<SqlExpression> expressions, Type resultType)
    {
        // Docs: https://learn.microsoft.com/sql/t-sql/functions/logical-functions-greatest-transact-sql
        if (_sqlServerSingletonOptions.EngineType == SqlServerEngineType.SqlServer
            && _sqlServerSingletonOptions.SqlServerCompatibilityLevel < 160)
        {
            return null;
        }

        if (_sqlServerSingletonOptions.EngineType == SqlServerEngineType.AzureSql
            && _sqlServerSingletonOptions.AzureSqlCompatibilityLevel < 160)
        {
            return null;
        }

        var resultTypeMapping = ExpressionExtensions.InferTypeMapping(expressions);

        // If one or more arguments aren't NULL, then NULL arguments are ignored during comparison.
        // If all arguments are NULL, then GREATEST returns NULL.
        return _sqlExpressionFactory.Function(
            "GREATEST", expressions, nullable: true, Enumerable.Repeat(false, expressions.Count), resultType, resultTypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public override SqlExpression? GenerateLeast(IReadOnlyList<SqlExpression> expressions, Type resultType)
    {
        // Docs: https://learn.microsoft.com/sql/t-sql/functions/logical-functions-least-transact-sql
        if (_sqlServerSingletonOptions.EngineType == SqlServerEngineType.SqlServer
            && _sqlServerSingletonOptions.SqlServerCompatibilityLevel < 160)
        {
            return null;
        }

        if (_sqlServerSingletonOptions.EngineType == SqlServerEngineType.AzureSql
            && _sqlServerSingletonOptions.AzureSqlCompatibilityLevel < 160)
        {
            return null;
        }

        var resultTypeMapping = ExpressionExtensions.InferTypeMapping(expressions);

        // If one or more arguments aren't NULL, then NULL arguments are ignored during comparison.
        // If all arguments are NULL, then LEAST returns NULL.
        return _sqlExpressionFactory.Function(
            "LEAST", expressions, nullable: true, Enumerable.Repeat(false, expressions.Count), resultType, resultTypeMapping);
    }

    private Expression TranslateByteArrayElementAccess(Expression array, Expression index, Type resultType)
    {
        var visitedArray = Visit(array);
        var visitedIndex = Visit(index);

        return visitedArray is SqlExpression sqlArray
            && visitedIndex is SqlExpression sqlIndex
                ? Dependencies.SqlExpressionFactory.Convert(
                    Dependencies.SqlExpressionFactory.Function(
                        "SUBSTRING",
                        new[]
                        {
                            sqlArray,
                            Dependencies.SqlExpressionFactory.Add(
                                Dependencies.SqlExpressionFactory.ApplyDefaultTypeMapping(sqlIndex),
                                Dependencies.SqlExpressionFactory.Constant(1)),
                            Dependencies.SqlExpressionFactory.Constant(1)
                        },
                        nullable: true,
                        argumentsPropagateNullability: Statics.TrueArrays[3],
                        typeof(byte[])),
                    resultType)
                : QueryCompilationContext.NotTranslatedExpression;
    }

    [DebuggerStepThrough]
    private static bool TranslationFailed(Expression? original, Expression? translation, out SqlExpression? castTranslation)
    {
        if (original != null
            && translation is not SqlExpression)
        {
            castTranslation = null;
            return true;
        }

        castTranslation = translation as SqlExpression;
        return false;
    }

    private static string? GetProviderType(SqlExpression expression)
        => expression.TypeMapping?.StoreType;
}
