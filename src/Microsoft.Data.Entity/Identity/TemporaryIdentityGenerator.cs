// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.Identity
{
    public class TemporaryIdentityGenerator : IIdentityGenerator<int>
    {
        private int _current = 0;

        public Task<int> NextAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(Interlocked.Decrement(ref _current));
        }

        async Task<object> IIdentityGenerator.NextAsync(CancellationToken cancellationToken)
        {
            return await NextAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
