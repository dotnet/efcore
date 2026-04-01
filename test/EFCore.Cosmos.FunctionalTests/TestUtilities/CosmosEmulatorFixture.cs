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
    public Exception? InitializationException { get; private set; }

    public async Task InitializeAsync()
    {
        try
        {
            await TestEnvironment.InitializeAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            // Store the exception but don't rethrow — the assembly runner would abort all
            // tests instead of letting CosmosDbConfiguredCondition skip them gracefully.
            InitializationException = ex;
        }
    }

    public async Task DisposeAsync()
        => await TestEnvironment.DisposeAsync().ConfigureAwait(false);
}
