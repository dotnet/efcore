// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Query
{
    public interface ICompiledQueryCache
    {
        TResult Execute<TResult>(
            [NotNull] Expression query,
            [NotNull] IDatabase database,
            [NotNull] QueryContext queryContext);

        IAsyncEnumerable<TResult> ExecuteAsync<TResult>(
            [NotNull] Expression query,
            [NotNull] IDatabase database,
            [NotNull] QueryContext queryContext);

        Task<TResult> ExecuteAsync<TResult>(
            [NotNull] Expression query,
            [NotNull] IDatabase database,
            [NotNull] QueryContext queryContext,
            CancellationToken cancellationToken);
    }
}
