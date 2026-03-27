// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

#nullable disable

public class ExpectedFilteredInclude<TEntity, TIncluded>(
    Expression<Func<TEntity, IEnumerable<TIncluded>>> include,
    string navigationPath = "",
    Func<IEnumerable<TIncluded>, IEnumerable<TIncluded>> includeFilter = null,
    bool assertOrder = false) : ExpectedInclude<TEntity>(Convert(include), navigationPath)
{
    public Func<IEnumerable<TIncluded>, IEnumerable<TIncluded>> IncludeFilter { get; } = includeFilter;

    public bool AssertOrder { get; } = assertOrder;

    private static Expression<Func<TEntity, object>> Convert(Expression<Func<TEntity, IEnumerable<TIncluded>>> include)
        => Expression.Lambda<Func<TEntity, object>>(include.Body, include.Parameters);
}
