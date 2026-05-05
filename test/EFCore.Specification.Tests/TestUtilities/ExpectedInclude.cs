// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class ExpectedInclude<TEntity>(Expression<Func<TEntity, object>> include, string navigationPath = "") : IExpectedInclude
{
    public MemberInfo IncludeMember { get; } = ((MemberExpression)include.Body).Member;
    public string NavigationPath { get; } = navigationPath;
}
