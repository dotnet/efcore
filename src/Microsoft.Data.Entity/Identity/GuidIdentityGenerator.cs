// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.Identity
{
    public class GuidIdentityGenerator : IIdentityGenerator<Guid>
    {
        public virtual Task<Guid> NextAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.FromResult(Guid.NewGuid());
        }

        async Task<object> IIdentityGenerator.NextAsync(CancellationToken cancellationToken)
        {
            return await NextAsync(cancellationToken).ConfigureAwait(false);
        }
    }
}
