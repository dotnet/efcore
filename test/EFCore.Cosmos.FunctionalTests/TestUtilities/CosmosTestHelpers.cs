// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Cosmos.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Cosmos.Internal;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore.TestUtilities;

public class CosmosTestHelpers : TestHelpers
{
    protected CosmosTestHelpers()
    {
    }

    public static CosmosTestHelpers Instance { get; } = new();

    public override ModelAsserter ModelAsserter
        => CosmosModelAsserter.Instance;

    public override IServiceCollection AddProviderServices(IServiceCollection services)
        => services.AddEntityFrameworkCosmos();

    public override DbContextOptionsBuilder UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
        => TestEnvironment.UseTokenCredential
            ? optionsBuilder.UseCosmos(
                TestEnvironment.DefaultConnection,
                TestEnvironment.TokenCredential,
                "UnitTests")
            : optionsBuilder.UseCosmos(
                TestEnvironment.DefaultConnection,
                TestEnvironment.AuthToken,
                "UnitTests");

    private static readonly string SyncMessage
        = CoreStrings.WarningAsErrorTemplate(
            CosmosEventId.SyncNotSupported.ToString(),
            CosmosResources.LogSyncNotSupported(new TestLogger<CosmosLoggingDefinitions>()).GenerateMessage(),
            "CosmosEventId.SyncNotSupported");

    public override LoggingDefinitions LoggingDefinitions { get; } = new CosmosLoggingDefinitions();

    public async Task NoSyncTest(bool async, Func<bool, Task> testCode)
    {
        try
        {
            await testCode(async);
            Assert.True(async);
        }
        catch (InvalidOperationException e)
        {
            if (e.Message != SyncMessage)
            {
                throw;
            }

            Assert.False(async);
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException?.Message != SyncMessage)
            {
                throw;
            }

            Assert.False(async);
        }
    }

    public void NoSyncTest(Action testCode)
    {
        try
        {
            testCode();
            Assert.Fail("Sync code did not fail.");
        }
        catch (InvalidOperationException e)
        {
            if (e.Message != SyncMessage)
            {
                throw;
            }
        }
        catch (DbUpdateException e)
        {
            if (e.InnerException?.Message != SyncMessage)
            {
                throw;
            }
        }
    }
}
