// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/> and multiple registrations
    ///         are allowed. This means that each <see cref="DbContext"/> instance will use its own
    ///         set of instances of this service.
    ///         The implementations may depend on other services registered with any lifetime.
    ///         The implementations do not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class ProxiesConventionSetCustomizer : IConventionSetCustomizer
    {
        private readonly IDbContextOptions _options;
        private readonly IConstructorBindingFactory _constructorBindingFactory;
        private readonly IProxyFactory _proxyFactory;
        private readonly IDiagnosticsLogger<DbLoggerCategory.Model> _logger;
        private readonly LazyLoaderParameterBindingFactoryDependencies _lazyLoaderParameterBindingFactoryDependencies;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ProxiesConventionSetCustomizer(
            [NotNull] IDbContextOptions options,
            [NotNull] IConstructorBindingFactory constructorBindingFactory,
            [NotNull] IProxyFactory proxyFactory,
            [NotNull] IDiagnosticsLogger<DbLoggerCategory.Model> logger,
            [NotNull] LazyLoaderParameterBindingFactoryDependencies lazyLoaderParameterBindingFactoryDependencies)
        {
            _options = options;
            _constructorBindingFactory = constructorBindingFactory;
            _proxyFactory = proxyFactory;
            _logger = logger;
            _lazyLoaderParameterBindingFactoryDependencies = lazyLoaderParameterBindingFactoryDependencies;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ConventionSet ModifyConventions(ConventionSet conventionSet)
        {
            conventionSet.ModelFinalizedConventions.Add(
                new ProxyBindingRewriter(
                    _lazyLoaderParameterBindingFactoryDependencies,
                    _proxyFactory,
                    _constructorBindingFactory,
                    _logger,
                    _options.FindExtension<ProxiesOptionsExtension>()));

            return conventionSet;
        }
    }
}
