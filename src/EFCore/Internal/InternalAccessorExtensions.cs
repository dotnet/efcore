// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public static class InternalAccessorExtensions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static TService GetService<TService>([CanBeNull] IInfrastructure<IServiceProvider> accessor)
        {
            object service = null;

            if (accessor != null)
            {
                var internalServiceProvider = accessor.Instance;

                service = internalServiceProvider.GetService(typeof(TService))
                          ?? internalServiceProvider.GetService<IDbContextOptions>()
                              ?.Extensions.OfType<CoreOptionsExtension>().FirstOrDefault()
                              ?.ApplicationServiceProvider
                              ?.GetService(typeof(TService));

                if (service == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.NoProviderConfiguredFailedToResolveService(typeof(TService).DisplayName()));
                }
            }

            return (TService)service;
        }
    }
}
