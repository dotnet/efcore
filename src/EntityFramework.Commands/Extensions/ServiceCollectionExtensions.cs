// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if DNX451 || DNXCORE50

// ReSharper disable once CheckNamespace

using System;
using JetBrains.Annotations;
using Microsoft.Dnx.Runtime;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection ImportDnxServices(
            [NotNull] this IServiceCollection services,
            [CanBeNull] IServiceProvider dnxServices)
        {
            if (dnxServices != null)
            {
                var runtimeServices = dnxServices.GetRequiredService<IRuntimeServices>();
                foreach (var service in runtimeServices.Services)
                {
                    services.AddTransient(service, _ => dnxServices.GetService(service));
                }
            }

            return services;
        }
    }
}

#endif
