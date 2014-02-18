// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Identity;

namespace Microsoft.Data.InMemory
{
    public class InMemoryIdentityGenerator : IIdentityGenerator<long>
    {
        private static long _current;

        public Task<long> NextAsync()
        {
            return Task.FromResult(Interlocked.Increment(ref _current));
        }

        async Task<object> IIdentityGenerator.NextAsync()
        {
            return await NextAsync();
        }
    }
}
