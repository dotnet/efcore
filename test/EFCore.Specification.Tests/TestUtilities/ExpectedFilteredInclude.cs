// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
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
}
