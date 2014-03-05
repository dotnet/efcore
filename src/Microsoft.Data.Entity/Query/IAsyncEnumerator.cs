// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.Query
{
    public interface IAsyncEnumerator : IDisposable
    {
        Task<bool> MoveNextAsync(CancellationToken cancellationToken);

        object Current { get; }
    }
}
