// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Net.Sockets;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;

namespace Microsoft.EntityFrameworkCore.TestUtilities;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
public class CosmosDbConfiguredConditionAttribute : Attribute, ITestCondition
{
    private static readonly SemaphoreSlim _semaphore = new(1);

    private static bool? _isConnectionAvailable;

    public string SkipReason
        => "Unable to connect to Cosmos DB. Please install/start the emulator service or configure a valid endpoint.";

    public async ValueTask<bool> IsMetAsync()
    {
        if (_isConnectionAvailable.HasValue)
        {
            return _isConnectionAvailable.Value;
        }

        await _semaphore.WaitAsync();
        try
        {
            if (_isConnectionAvailable.HasValue)
            {
                return _isConnectionAvailable.Value;
            }

            await using var testStore = CosmosTestStoreFactory.Instance.Create(nameof(CosmosDbConfiguredConditionAttribute));
            try
            {
                await testStore.InitializeAsync(null, (Func<DbContext>?)null);
                _isConnectionAvailable = true;
            }
            catch (AggregateException aggregate)
            {
                if (aggregate.Flatten().InnerExceptions.Any(IsNotConfigured))
                {
                    _isConnectionAvailable = false;
                }
                else
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                if (IsNotConfigured(e))
                {
                    _isConnectionAvailable = false;
                }
                else
                {
                    throw;
                }
            }
        }
        finally
        {
            _semaphore.Release();
        }

        return _isConnectionAvailable.Value;
    }

    private static bool IsNotConfigured(Exception exception)
        => exception switch
        {
            HttpRequestException re => re.InnerException is SocketException // Exception in Mac/Linux
                || re.InnerException is IOException { InnerException: SocketException }, // Exception in Windows
            _ => exception.Message.Contains(
                "The input authorization token can't serve the request. Please check that the expected payload is built as per the protocol, and check the key being used.",
                StringComparison.Ordinal),
        };
}
