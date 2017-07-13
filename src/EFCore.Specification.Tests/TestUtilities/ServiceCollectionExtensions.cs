// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public static class ServiceCollectionExtensions
    {
        private static readonly MethodInfo _addDbContext
            = typeof(EntityFrameworkServiceCollectionExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkServiceCollectionExtensions.AddDbContext))
                .Single(mi => mi.GetParameters().Length == 4
                              && mi.GetParameters()[1].ParameterType == typeof(Action<IServiceProvider, DbContextOptionsBuilder>));

        public static IServiceCollection AddDbContext(
            this IServiceCollection serviceCollection,
            Type contextType,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            => (IServiceCollection)_addDbContext.MakeGenericMethod(contextType)
                .Invoke(null, new object[] { serviceCollection, optionsAction, contextLifetime, optionsLifetime });
    }
}
