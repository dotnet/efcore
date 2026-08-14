// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.TestUtilities;

public static class ServiceCollectionExtensions
{
    private static readonly MethodInfo _addDbContext
        = typeof(EntityFrameworkServiceCollectionExtensions)
            .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkServiceCollectionExtensions.AddDbContext))
            .Single(
                mi => mi.GetParameters().Length == 4
                    && mi.GetParameters()[1].ParameterType == typeof(Action<IServiceProvider, DbContextOptionsBuilder>)
                    && mi.GetGenericArguments().Length == 1);

    public static IServiceCollection AddDbContext(
        this IServiceCollection serviceCollection,
        Type contextType,
        Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
        ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
        ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
        => (IServiceCollection)_addDbContext.MakeGenericMethod(contextType)
            .Invoke(null, [serviceCollection, optionsAction, contextLifetime, optionsLifetime])!;

    private static readonly MethodInfo _addDbContextPool
        = typeof(EntityFrameworkServiceCollectionExtensions)
            .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkServiceCollectionExtensions.AddPooledDbContextFactory))
            .Single(
                mi => mi.GetParameters().Length == 3
                    && mi.GetParameters()[1].ParameterType == typeof(Action<IServiceProvider, DbContextOptionsBuilder>)
                    && mi.GetGenericArguments().Length == 1);

    public static IServiceCollection AddPooledDbContextFactory(
        this IServiceCollection serviceCollection,
        Type contextType,
        Action<IServiceProvider, DbContextOptionsBuilder> optionsAction)
        => (IServiceCollection)_addDbContextPool.MakeGenericMethod(contextType)
            .Invoke(null, [serviceCollection, optionsAction, 128])!;
}
