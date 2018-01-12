// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ProxiesConventionSetBuilder : IConventionSetBuilder
    {
        private readonly IDbContextOptions _options;
        private readonly IConstructorBindingFactory _constructorBindingFactory;
        private readonly IProxyFactory _proxyFactory;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ProxiesConventionSetBuilder(
            [NotNull] IDbContextOptions options,
            [NotNull] IConstructorBindingFactory constructorBindingFactory,
            [NotNull] IProxyFactory proxyFactory)
        {
            _options = options;
            _constructorBindingFactory = constructorBindingFactory;
            _proxyFactory = proxyFactory;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ConventionSet AddConventions(ConventionSet conventionSet)
        {
            conventionSet.ModelBuiltConventions.Add(
                new ProxyBindingRewriter(
                    _proxyFactory,
                    _constructorBindingFactory,
                    _options.FindExtension<ProxiesOptionsExtension>()));

            return conventionSet;
        }
    }
}
