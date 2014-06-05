// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Utilities;

// ReSharper disable once CheckNamespace

namespace Microsoft.Framework.DependencyInjection
{
    internal static class ServiceProviderExtensions
    {
        public static TService TryGetService<TService>([NotNull] this IServiceProvider serviceProvider)
            where TService : class
        {
            Check.NotNull(serviceProvider, "serviceProvider");

            var services = serviceProvider.GetService<IEnumerable<TService>>();

            return services == null ? null : services.FirstOrDefault();
        }

        public static object TryGetService([NotNull] this IServiceProvider serviceProvider, Type serviceType)
        {
            Check.NotNull(serviceProvider, "serviceProvider");
            Check.NotNull(serviceType, "serviceType");

            // Use try-catch here to avoid use of MakeGenericType for IEnumerbale pattern
            try
            {
                return serviceProvider.GetService(serviceType);
            }
            catch
            {
                return null;
            }
        }
    }
}
