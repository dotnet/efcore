// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Query
{
    public class EntityQueryable<TResult> 
        : QueryableBase<TResult>, IAsyncEnumerable<TResult>, IIncludableQueryable<TResult, object>
    {
        public EntityQueryable([NotNull] EntityQueryProvider provider)
            : base(Check.NotNull(provider, "provider"))
        {
        }

        public EntityQueryable([NotNull] EntityQueryProvider provider, [NotNull] Expression expression)
            : base(
                Check.NotNull(provider, "provider"),
                Check.NotNull(expression, "expression"))
        {
        }

        IAsyncEnumerator<TResult> IAsyncEnumerable<TResult>.GetEnumerator()
        {
            return ((EntityQueryProvider)Provider).AsyncQuery<TResult>(Expression).GetEnumerator();
        }
    }
}
