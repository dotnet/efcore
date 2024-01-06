// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore;

public class ServiceProviderExtensionsTest
{
    [ConditionalFact]
    public void GetRequiredService_throws_useful_exception_if_service_not_registered()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider(validateScopes: true);

        Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetRequiredService<IPilkington>());
    }

    [ConditionalFact]
    public void Non_generic_GetRequiredService_throws_useful_exception_if_service_not_registered()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider(validateScopes: true);

        Assert.Throws<InvalidOperationException>(
            () => serviceProvider.GetRequiredService(typeof(IPilkington)));
    }

    [ConditionalFact]
    public void GetRequiredService_throws_useful_exception_if_resolution_fails()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IPilkington, Karl>();
        using var scope = serviceCollection.BuildServiceProvider(validateScopes: true).CreateScope();
        var serviceProvider = scope.ServiceProvider;

        Assert.Equal(
            KarlQuote,
            Assert.Throws<NotSupportedException>(
                () => serviceProvider.GetRequiredService<IPilkington>()).Message);
    }

    [ConditionalFact]
    public void Non_generic_GetRequiredService_throws_useful_exception_if_resolution_fails()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IPilkington, Karl>();

        using var scope = serviceCollection.BuildServiceProvider(validateScopes: true).CreateScope();
        var serviceProvider = scope.ServiceProvider;

        Assert.Equal(
            KarlQuote,
            Assert.Throws<NotSupportedException>(
                () => serviceProvider.GetRequiredService(typeof(IPilkington))).Message);
    }

    [ConditionalFact]
    public void GetService_returns_null_if_service_not_registered()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider(validateScopes: true);

        Assert.Null(serviceProvider.GetService<IPilkington>());
    }

    [ConditionalFact]
    public void Non_generic_GetService_returns_null_if_service_not_registered()
    {
        var serviceProvider = new ServiceCollection().BuildServiceProvider(validateScopes: true);

        Assert.Null(serviceProvider.GetService(typeof(IPilkington)));
    }

    [ConditionalFact]
    public void GetService_throws_useful_exception_if_resolution_fails()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IPilkington, Karl>();
        using var scope = serviceCollection.BuildServiceProvider(validateScopes: true).CreateScope();
        var serviceProvider = scope.ServiceProvider;

        Assert.Equal(
            KarlQuote,
            Assert.Throws<NotSupportedException>(
                () => serviceProvider.GetService<IPilkington>()).Message);
    }

    [ConditionalFact]
    public void Non_generic_GetService_throws_useful_exception_if_resolution_fails()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddScoped<IPilkington, Karl>();

        using var scope = serviceCollection.BuildServiceProvider(validateScopes: true).CreateScope();
        var serviceProvider = scope.ServiceProvider;

        Assert.Equal(
            KarlQuote,
            Assert.Throws<NotSupportedException>(
                () => serviceProvider.GetService(typeof(IPilkington))).Message);
    }

    private const string KarlQuote = "You can only talk rubbish if you're aware of knowledge.";

    private interface IPilkington;

    private class Karl : IPilkington
    {
        public Karl()
        {
            throw new NotSupportedException(KarlQuote);
        }
    }
}
