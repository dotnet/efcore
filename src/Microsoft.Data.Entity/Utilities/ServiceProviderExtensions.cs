// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity;

namespace Microsoft.Framework.DependencyInjection
{
    internal static class ServiceProviderExtensions
    {
        public static TService GetRequiredService<TService>(this IServiceProvider serviceProvider)
            where TService : class
        {
            // TODO: Consider checking for null serviceProvider when used with improperly constructed config.

            var service = serviceProvider.GetService<TService>();

            if (service != null)
            {
                return service;
            }

            throw new InvalidOperationException(Strings.FormatMissingConfigurationItem(typeof(TService)));
        }
    }
}
