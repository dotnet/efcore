// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;

/// <summary>
///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
///     the same compatibility standards as public APIs. It may be changed or removed without notice in
///     any release. You should only use it directly in your code with extreme caution and knowing that
///     doing so can result in application failures when updating to a new Entity Framework Core release.
/// </summary>
public class SqlServerZeroLimitConverter : ExpressionVisitor
{
    private readonly ISqlExpressionFactory _sqlExpressionFactory;

    private CacheSafeParameterFacade _parametersFacade;

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public SqlServerZeroLimitConverter(ISqlExpressionFactory sqlExpressionFactory)
    {
        _sqlExpressionFactory = sqlExpressionFactory;
        _parametersFacade = null!;
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public virtual Expression Process(Expression queryExpression, CacheSafeParameterFacade parametersFacade)
    {
        _parametersFacade = parametersFacade;

        return Visit(queryExpression);
    }

    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    protected override Expression VisitExtension(Expression extensionExpression)
    {
        // SQL Server doesn't support 0 in the FETCH NEXT x ROWS ONLY clause. We use this clause when translating LINQ Take(), but
        // only if there's also a Skip(), otherwise we translate to SQL TOP(x), which does allow 0.
        // Check for this case, and replace with a false predicate (since no rows should be returned).
        if (extensionExpression is SelectExpression { Offset: not null, Limit: not null } selectExpression)
        {
            if (IsZero(selectExpression.Limit))
            {
                return selectExpression.Update(
                    selectExpression.Tables,
                    selectExpression.GroupBy.Count > 0 ? selectExpression.Predicate : _sqlExpressionFactory.Constant(false),
                    selectExpression.GroupBy,
                    selectExpression.GroupBy.Count > 0 ? _sqlExpressionFactory.Constant(false) : null,
                    selectExpression.Projection,
                    orderings: [],
                    offset: null,
                    limit: null);
            }

            bool IsZero(SqlExpression? sqlExpression)
                => sqlExpression switch
                {
                    SqlConstantExpression { Value: int i } => i == 0,
                    SqlParameterExpression p => _parametersFacade.GetParametersAndDisableSqlCaching()[p.Name] is 0,
                    _ => false
                };
        }

        return base.VisitExtension(extensionExpression);
    }
}
