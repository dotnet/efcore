// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
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
}
