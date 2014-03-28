// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Query
{
    public interface IAsyncQueryProvider : IQueryProvider
    {
        Task<object> ExecuteAsync([NotNull] Expression expression, CancellationToken cancellationToken);
        Task<T> ExecuteAsync<T>([NotNull] Expression expression, CancellationToken cancellationToken);
    }
}
