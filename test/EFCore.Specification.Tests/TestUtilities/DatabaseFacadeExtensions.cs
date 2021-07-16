// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public static class DatabaseFacadeExtensions
    {
        public static bool EnsureCreatedResiliently(this DatabaseFacade facade)
            => facade.CreateExecutionStrategy().Execute(facade, f => f.EnsureCreated());

        public static Task<bool> EnsureCreatedResilientlyAsync(this DatabaseFacade façade, CancellationToken cancellationToken = default)
            => façade.CreateExecutionStrategy().ExecuteAsync(façade, (f, ct) => f.EnsureCreatedAsync(ct), cancellationToken);
    }
}
