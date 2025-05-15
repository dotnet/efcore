// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;
internal class RuntimeQueryFilter : IQueryFilter
{
    public virtual LambdaExpression Expression { get; }

    public virtual string? Key { get; }

    public bool IsAnonymous { get; }

    public RuntimeQueryFilter(IQueryFilter queryFilter, Func<LambdaExpression, LambdaExpression> rewriter)
    {
        ArgumentNullException.ThrowIfNull(queryFilter?.Expression);

        Expression = rewriter(queryFilter.Expression);
        Key = queryFilter.Key;
        IsAnonymous = queryFilter.IsAnonymous;
    }
}
