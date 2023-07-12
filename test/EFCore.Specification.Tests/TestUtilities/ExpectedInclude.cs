// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class ExpectedInclude<TEntity> : IExpectedInclude
{
    public MemberInfo IncludeMember { get; }
    public string NavigationPath { get; }

    public ExpectedInclude(Expression<Func<TEntity, object>> include, string navigationPath = "")
    {
        IncludeMember = ((MemberExpression)include.Body).Member;
        NavigationPath = navigationPath;
    }
}
