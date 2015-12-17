// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DNX451 || DNXCORE50

// ReSharper disable once CheckNamespace

using JetBrains.Annotations;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection ImportDnxServices(
            [NotNull] this IServiceCollection services)
        {
            services.TryAddSingleton(PlatformServices.Default.Application);
            services.TryAddSingleton(PlatformServices.Default.Runtime);
            services.TryAddSingleton(PlatformServices.Default.AssemblyLoadContextAccessor);
            services.TryAddSingleton(PlatformServices.Default.AssemblyLoaderContainer);
            services.TryAddSingleton(PlatformServices.Default.LibraryManager);

            return services;
        }
    }
}

#endif
