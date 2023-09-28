// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class ExpectedFilteredInclude<TEntity, TIncluded> : ExpectedInclude<TEntity>
{
    public Func<IEnumerable<TIncluded>, IEnumerable<TIncluded>> IncludeFilter { get; }

    public bool AssertOrder { get; }

    public ExpectedFilteredInclude(
        Expression<Func<TEntity, IEnumerable<TIncluded>>> include,
        string navigationPath = "",
        Func<IEnumerable<TIncluded>, IEnumerable<TIncluded>> includeFilter = null,
        bool assertOrder = false)
        : base(Convert(include), navigationPath)
    {
        IncludeFilter = includeFilter;
        AssertOrder = assertOrder;
    }

    private static Expression<Func<TEntity, object>> Convert(Expression<Func<TEntity, IEnumerable<TIncluded>>> include)
        => Expression.Lambda<Func<TEntity, object>>(include.Body, include.Parameters);
}
