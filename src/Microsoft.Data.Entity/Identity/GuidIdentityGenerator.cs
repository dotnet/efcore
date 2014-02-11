// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.Identity
{
    public class GuidIdentityGenerator : IIdentityGenerator<Guid>
    {
        public virtual Task<Guid> NextAsync()
        {
            return Task.FromResult(Guid.NewGuid());
        }
    }
}
