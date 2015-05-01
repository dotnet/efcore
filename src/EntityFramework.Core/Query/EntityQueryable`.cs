// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;
using Remotion.Linq;

namespace Microsoft.Data.Entity.Query
{
    public class EntityQueryable<TResult>
        : QueryableBase<TResult>, IAsyncEnumerable<TResult>
    {
        public EntityQueryable([NotNull] IEntityQueryProvider provider)
            : base(Check.NotNull(provider, nameof(provider)))
        {
        }

        public EntityQueryable([NotNull] IEntityQueryProvider provider, [NotNull] Expression expression)
            : base(
                Check.NotNull(provider, nameof(provider)),
                Check.NotNull(expression, nameof(expression)))
        {
        }

        IAsyncEnumerator<TResult> IAsyncEnumerable<TResult>.GetEnumerator()
            => ((IAsyncQueryProvider)Provider).ExecuteAsync<TResult>(Expression).GetEnumerator();
    }
}
