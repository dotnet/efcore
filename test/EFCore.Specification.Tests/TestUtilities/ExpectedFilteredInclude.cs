// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class ExpectedFilteredInclude<TEntity, TIncluded> : ExpectedInclude<TEntity>
    {
        public Func<IEnumerable<TIncluded>, IEnumerable<TIncluded>> IncludeFilter { get; }

        public ExpectedFilteredInclude(
            Func<TEntity, IEnumerable<TIncluded>> include,
            string includedName,
            string navigationPath = "",
            Func<IEnumerable<TIncluded>, IEnumerable<TIncluded>> includeFilter = null)
            : base(include, includedName, navigationPath)
        {
            IncludeFilter = includeFilter;
        }
    }
}
