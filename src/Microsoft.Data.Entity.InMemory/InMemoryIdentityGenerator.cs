// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Identity;

namespace Microsoft.Data.Entity.InMemory
{
    public class InMemoryIdentityGenerator : IIdentityGenerator<long>
    {
        private static long _current;

        public Task<long> NextAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(Interlocked.Increment(ref _current));
        }

        async Task<object> IIdentityGenerator.NextAsync(CancellationToken cancellationToken)
        {
            return await NextAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
