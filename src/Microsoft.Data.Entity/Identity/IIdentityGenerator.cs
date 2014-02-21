// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Data.Entity.Identity
{
    public interface IIdentityGenerator
    {
        Task<object> NextAsync(CancellationToken cancellationToken);
    }
}
