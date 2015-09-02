// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query
{
    public interface IQueryCompiler
    {
        TResult Execute<TResult>([NotNull] Expression query);

        IAsyncEnumerable<TResult> ExecuteAsync<TResult>([NotNull] Expression query);

        Task<TResult> ExecuteAsync<TResult>([NotNull] Expression query, CancellationToken cancellationToken);
    }
}
