// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
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
    public class ServiceProviderCache
    {
        private readonly ConcurrentDictionary<long, IServiceProvider> _configurations
            = new ConcurrentDictionary<long, IServiceProvider>();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public static ServiceProviderCache Instance { get; } = new ServiceProviderCache();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IServiceProvider GetOrAdd([NotNull] IDbContextOptions options)
        {
            // Decided that this hashing algorithm is robust enough. See issue #762.
            unchecked
            {
                var key = options.Extensions.Aggregate(0, (t, e) => (t * 397) ^ e.GetType().GetHashCode());

                var replacedServices = options.FindExtension<CoreOptionsExtension>()?.ReplacedServices;
                if (replacedServices != null)
                {
                    key = replacedServices.Aggregate(key, (t, e) => (t * 397) ^ e.Value.GetHashCode());
                }

                return _configurations.GetOrAdd(
                    key,
                    k =>
                        {
                            var services = new ServiceCollection();
                            var builder = services.AddEntityFramework();

                            foreach (var extension in options.Extensions)
                            {
                                extension.ApplyServices(builder);
                            }

                            if (replacedServices != null)
                            {
                                foreach (var descriptor in services.ToList())
                                {
                                    Type replacementType;
                                    if (replacedServices.TryGetValue(descriptor.ServiceType, out replacementType))
                                    {
                                        services[services.IndexOf(descriptor)]
                                            = new ServiceDescriptor(descriptor.ServiceType, replacementType, descriptor.Lifetime);
                                    }
                                }
                            }

                            return services.BuildServiceProvider();
                        });
            }
        }
    }
}
