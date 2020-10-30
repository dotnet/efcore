// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Sqlite.Query.SqlExpressions.Internal;
using ExpressionExtensions = Microsoft.EntityFrameworkCore.Query.ExpressionExtensions;

namespace Microsoft.EntityFrameworkCore.Sqlite.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqliteSqlExpressionFactory : SqlExpressionFactory
{
    private readonly RelationalTypeMapping _boolTypeMapping;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqliteSqlExpressionFactory(SqlExpressionFactoryDependencies dependencies)
        : base(dependencies)
        => _boolTypeMapping = dependencies.TypeMappingSource.FindMapping(typeof(bool), dependencies.Model)!;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlFunctionExpression Strftime(
        Type returnType,
        string format,
        SqlExpression timestring,
        IEnumerable<SqlExpression>? modifiers = null,
        RelationalTypeMapping? typeMapping = null)
    {
        modifiers ??= Enumerable.Empty<SqlExpression>();

        // If the inner call is another strftime then shortcut a double call
        if (timestring is SqlFunctionExpression rtrimFunction
            && rtrimFunction.Name == "rtrim"
            && rtrimFunction.Arguments!.Count == 2
            && rtrimFunction.Arguments[0] is SqlFunctionExpression rtrimFunction2
            && rtrimFunction2.Name == "rtrim"
            && rtrimFunction2.Arguments!.Count == 2
            && rtrimFunction2.Arguments[0] is SqlFunctionExpression strftimeFunction
            && strftimeFunction.Name == "strftime"
            && strftimeFunction.Arguments!.Count > 1)
        {
            // Use its timestring parameter directly in place of ours
            timestring = strftimeFunction.Arguments[1];

            // Prepend its modifier arguments (if any) to the current call
            modifiers = strftimeFunction.Arguments.Skip(2).Concat(modifiers);
        }

        if (timestring is SqlFunctionExpression dateFunction
            && dateFunction.Name == "date")
        {
            timestring = dateFunction.Arguments![0];
            modifiers = dateFunction.Arguments.Skip(1).Concat(modifiers);
        }

        var finalArguments = new[] { Constant(format), timestring }.Concat(modifiers);

        return Function(
            "strftime",
            finalArguments,
            nullable: true,
            argumentsPropagateNullability: finalArguments.Select(_ => true),
            returnType,
            typeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual SqlFunctionExpression Date(
        Type returnType,
        SqlExpression timestring,
        IEnumerable<SqlExpression>? modifiers = null,
        RelationalTypeMapping? typeMapping = null)
    {
        modifiers ??= Enumerable.Empty<SqlExpression>();

        if (timestring is SqlFunctionExpression dateFunction
            && dateFunction.Name == "date")
        {
            timestring = dateFunction.Arguments![0];
            modifiers = dateFunction.Arguments.Skip(1).Concat(modifiers);
        }

        var finalArguments = new[] { timestring }.Concat(modifiers);

        return Function(
            "date",
            finalArguments,
            nullable: true,
            argumentsPropagateNullability: finalArguments.Select(_ => true),
            returnType,
            typeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual GlobExpression Glob(SqlExpression match, SqlExpression pattern)
    {
        var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(match, pattern)
            ?? Dependencies.TypeMappingSource.FindMapping(match.Type, Dependencies.Model);

        match = ApplyTypeMapping(match, inferredTypeMapping);
        pattern = ApplyTypeMapping(pattern, inferredTypeMapping);

        return new GlobExpression(match, pattern, _boolTypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual RegexpExpression Regexp(SqlExpression match, SqlExpression pattern)
    {
        var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(match, pattern)
            ?? Dependencies.TypeMappingSource.FindMapping(match.Type, Dependencies.Model);

        match = ApplyTypeMapping(match, inferredTypeMapping);
        pattern = ApplyTypeMapping(pattern, inferredTypeMapping);

        return new RegexpExpression(match, pattern, _boolTypeMapping);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    [return: NotNullIfNotNull("sqlExpression")]
    public override SqlExpression? ApplyTypeMapping(SqlExpression? sqlExpression, RelationalTypeMapping? typeMapping)
        => sqlExpression == null || sqlExpression.TypeMapping != null
            ? sqlExpression
            : sqlExpression switch
            {
                GlobExpression globExpression => ApplyTypeMappingOnGlob(globExpression),
                RegexpExpression regexpExpression => ApplyTypeMappingOnRegexp(regexpExpression),
                _ => base.ApplyTypeMapping(sqlExpression, typeMapping)
            };

    private SqlExpression ApplyTypeMappingOnGlob(GlobExpression globExpression)
    {
        var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(globExpression.Match, globExpression.Pattern)
            ?? Dependencies.TypeMappingSource.FindMapping(globExpression.Match.Type, Dependencies.Model);

        var match = ApplyTypeMapping(globExpression.Match, inferredTypeMapping);
        var pattern = ApplyTypeMapping(globExpression.Pattern, inferredTypeMapping);

        return match != globExpression.Match || pattern != globExpression.Pattern || globExpression.TypeMapping != _boolTypeMapping
            ? new GlobExpression(match, pattern, _boolTypeMapping)
            : globExpression;
    }

    private SqlExpression? ApplyTypeMappingOnRegexp(RegexpExpression regexpExpression)
    {
        var inferredTypeMapping = ExpressionExtensions.InferTypeMapping(regexpExpression.Match, regexpExpression.Pattern)
            ?? Dependencies.TypeMappingSource.FindMapping(regexpExpression.Match.Type, Dependencies.Model);

        var match = ApplyTypeMapping(regexpExpression.Match, inferredTypeMapping);
        var pattern = ApplyTypeMapping(regexpExpression.Pattern, inferredTypeMapping);

        return match != regexpExpression.Match || pattern != regexpExpression.Pattern || regexpExpression.TypeMapping != _boolTypeMapping
            ? new RegexpExpression(match, pattern, _boolTypeMapping)
            : regexpExpression;
    }
}
