// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query
{
    public interface IAsyncQueryProvider : IQueryProvider
    {
        IAsyncEnumerable<TResult> ExecuteAsync<TResult>([NotNull] Expression expression);
        Task<TResult> ExecuteAsync<TResult>([NotNull] Expression expression, CancellationToken cancellationToken);
    }
}
