// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.Query
{
    public interface IAsyncEnumerator : IEnumerator, IDisposable
    {
        Task<bool> MoveNextAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
