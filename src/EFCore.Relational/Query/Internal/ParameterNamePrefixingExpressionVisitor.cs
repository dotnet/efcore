// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Query.Internal;

/// <summary>
///     Prefixes the names of all parameters in a finalized query expression, so that the SQL generated for several
///     queries can be combined into a single command without parameter name collisions. Invariant names, which key
///     the parameter values, are left untouched.
/// </summary>
/// <remarks>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </remarks>
public class ParameterNamePrefixingExpressionVisitor(string prefix) : ExpressionVisitor
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression node)
        => node switch
        {
            SqlParameterExpression parameter
                => new SqlParameterExpression(
                    parameter.InvariantName,
                    prefix + parameter.Name,
                    parameter.Type,
                    parameter.IsNullable,
                    parameter.TranslationMode,
                    parameter.TypeMapping),

            // FromSql arguments have already been expanded into a composite parameter whose sub-parameters are
            // referenced by name from the raw SQL; rename those as well.
            FromSqlExpression { Arguments: ConstantExpression { Value: CompositeRelationalParameter composite } } fromSql
                => fromSql.Update(
                    Expression.Constant(
                        new CompositeRelationalParameter(
                            composite.InvariantName,
                            composite.RelationalParameters.Select(PrefixSubParameter).ToList()))),

            _ => base.VisitExtension(node)
        };

    private IRelationalParameter PrefixSubParameter(IRelationalParameter parameter)
        => parameter is TypeMappedRelationalParameter typeMapped
            ? new TypeMappedRelationalParameter(
                prefix + typeMapped.InvariantName,
                prefix + typeMapped.Name,
                typeMapped.RelationalTypeMapping,
                typeMapped.IsNullable,
                typeMapped.Direction)
            : parameter;
}
