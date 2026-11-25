// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class DatabaseFacadeExtensions
{
    public static bool EnsureCreatedResiliently(this DatabaseFacade databaseFacade)
        => databaseFacade.CreateExecutionStrategy().Execute(databaseFacade, f => f.EnsureCreated());

    public static Task<bool> EnsureCreatedResilientlyAsync(
        this DatabaseFacade databaseFacade,
        CancellationToken cancellationToken = default)
        => databaseFacade.CreateExecutionStrategy().ExecuteAsync(
            databaseFacade, (f, ct) => f.EnsureCreatedAsync(ct), cancellationToken);
}
