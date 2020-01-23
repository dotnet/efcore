// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ServiceProviderCache
    {
        private readonly ConcurrentDictionary<long, (IServiceProvider ServiceProvider, IDictionary<string, string> DebugInfo)>
            _configurations
                = new ConcurrentDictionary<long, (IServiceProvider, IDictionary<string, string>)>();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static ServiceProviderCache Instance { get; } = new ServiceProviderCache();

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual IServiceProvider GetOrAdd([NotNull] IDbContextOptions options, bool providerRequired)
        {
            var coreOptionsExtension = options.FindExtension<CoreOptionsExtension>();
            var internalServiceProvider = coreOptionsExtension?.InternalServiceProvider;
            if (internalServiceProvider != null)
            {
                ValidateOptions(options);

                var optionsInitializer = internalServiceProvider.GetService<ISingletonOptionsInitializer>();
                if (optionsInitializer == null)
                {
                    throw new InvalidOperationException(CoreStrings.NoEfServices);
                }

                if (providerRequired)
                {
                    optionsInitializer.EnsureInitialized(internalServiceProvider, options);
                }

                return internalServiceProvider;
            }

            if (coreOptionsExtension?.ServiceProviderCachingEnabled == false)
            {
                return BuildServiceProvider().ServiceProvider;
            }

            var key = options.Extensions
                .OrderBy(e => e.GetType().Name)
                .Aggregate(0L, (t, e) => (t * 397) ^ ((long)e.GetType().GetHashCode() * 397) ^ e.Info.GetServiceProviderHashCode());

            return _configurations.GetOrAdd(key, k => BuildServiceProvider()).ServiceProvider;

            (IServiceProvider ServiceProvider, IDictionary<string, string> DebugInfo) BuildServiceProvider()
            {
                ValidateOptions(options);

                var debugInfo = new Dictionary<string, string>();
                foreach (var optionsExtension in options.Extensions)
                {
                    optionsExtension.Info.PopulateDebugInfo(debugInfo);
                }

                debugInfo = debugInfo.OrderBy(v => debugInfo.Keys).ToDictionary(d => d.Key, v => v.Value);

                var services = new ServiceCollection();
                var hasProvider = ApplyServices(options, services);

                var replacedServices = coreOptionsExtension?.ReplacedServices;
                if (replacedServices != null)
                {
                    var updatedServices = new ServiceCollection();
                    foreach (var descriptor in services)
                    {
                        if (replacedServices.TryGetValue((descriptor.ServiceType, descriptor.ImplementationType), out var replacementType))
                        {
                            ((IList<ServiceDescriptor>)updatedServices).Add(
                                new ServiceDescriptor(descriptor.ServiceType, replacementType, descriptor.Lifetime));
                        }
                        else if (replacedServices.TryGetValue((descriptor.ServiceType, null), out replacementType))
                        {
                            ((IList<ServiceDescriptor>)updatedServices).Add(
                                new ServiceDescriptor(descriptor.ServiceType, replacementType, descriptor.Lifetime));
                        }
                        else
                        {
                            ((IList<ServiceDescriptor>)updatedServices).Add(descriptor);
                        }
                    }

                    services = updatedServices;
                }

                var serviceProvider = services.BuildServiceProvider();

                if (hasProvider)
                {
                    serviceProvider
                        .GetRequiredService<ISingletonOptionsInitializer>()
                        .EnsureInitialized(serviceProvider, options);
                }

                using (var scope = serviceProvider.CreateScope())
                {
                    var scopedProvider = scope.ServiceProvider;

                    // If loggingDefinitions is null, then there is no provider yet
                    var loggingDefinitions = scopedProvider.GetService<LoggingDefinitions>();
                    if (loggingDefinitions != null)
                    {
                        // Because IDbContextOptions cannot yet be resolved from the internal provider
                        var logger = new DiagnosticsLogger<DbLoggerCategory.Infrastructure>(
                            ScopedLoggerFactory.Create(scopedProvider, options),
                            scopedProvider.GetService<ILoggingOptions>(),
                            scopedProvider.GetService<DiagnosticSource>(),
                            loggingDefinitions,
                            new NullDbContextLogger());

                        if (_configurations.Count == 0)
                        {
                            logger.ServiceProviderCreated(serviceProvider);
                        }
                        else
                        {
                            logger.ServiceProviderDebugInfo(
                                debugInfo,
                                _configurations.Values.Select(v => v.DebugInfo).ToList());

                            if (_configurations.Count >= 20)
                            {
                                logger.ManyServiceProvidersCreatedWarning(
                                    _configurations.Values.Select(e => e.ServiceProvider).ToList());
                            }
                        }

                        var applicationServiceProvider = options.FindExtension<CoreOptionsExtension>()?.ApplicationServiceProvider;
                        if (applicationServiceProvider?.GetService<IRegisteredServices>() != null)
                        {
                            logger.RedundantAddServicesCallWarning(serviceProvider);
                        }
                    }
                }

                return (serviceProvider, debugInfo);
            }
        }

        private static void ValidateOptions(IDbContextOptions options)
        {
            foreach (var extension in options.Extensions)
            {
                extension.Validate(options);
            }
        }

        private static bool ApplyServices(IDbContextOptions options, ServiceCollection services)
        {
            var coreServicesAdded = false;

            foreach (var extension in options.Extensions)
            {
                extension.ApplyServices(services);

                if (extension.Info.IsDatabaseProvider)
                {
                    coreServicesAdded = true;
                }
            }

            if (coreServicesAdded)
            {
                return true;
            }

            new EntityFrameworkServicesBuilder(services).TryAddCoreServices();

            return false;
        }
    }
}
