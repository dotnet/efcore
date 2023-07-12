// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore;

public class InMemoryFixture
{
    public static IServiceProvider DefaultServiceProvider { get; }
        = BuildServiceProvider();

    public static IServiceProvider DefaultSensitiveServiceProvider { get; }
        = BuildServiceProvider();

    public static IServiceProvider DefaultNullabilityCheckProvider { get; }
        = BuildServiceProvider();

    public static IServiceProvider DefaultNullabilitySensitiveCheckProvider { get; }
        = BuildServiceProvider();

    public readonly IServiceProvider ServiceProvider;

    public InMemoryFixture()
    {
        ServiceProvider = BuildServiceProvider();
    }

    public static ServiceProvider BuildServiceProvider(ILoggerFactory loggerFactory)
        => BuildServiceProvider(new ServiceCollection().AddSingleton(loggerFactory));

    public static ServiceProvider BuildServiceProvider(IServiceCollection providerServices = null)
        => InMemoryTestStoreFactory.Instance.AddProviderServices(
                providerServices
                ?? new ServiceCollection())
            .BuildServiceProvider(validateScopes: true);
}
