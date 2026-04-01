// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

/// <summary>
///     Assembly-wide fixture that starts the Cosmos emulator (via testcontainer or
///     by connecting to an existing endpoint) before any tests run, and disposes the
///     container after all tests complete.
/// </summary>
public class CosmosEmulatorFixture : IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        try
        {
            await TestEnvironment.InitializeAsync().ConfigureAwait(false);
        }
        catch
        {
            // Swallow so the assembly runner doesn't abort; tests will be skipped
            // by CosmosDbConfiguredCondition when the connection isn't reachable.
        }
    }

    public async Task DisposeAsync()
        => await TestEnvironment.DisposeAsync().ConfigureAwait(false);
}
