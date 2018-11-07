// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ProxiesOptionsExtension : IDbContextOptionsExtensionWithDebugInfo
    {
        private bool _useLazyLoadingProxies;
        private string _logFragment;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ProxiesOptionsExtension()
        {
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected ProxiesOptionsExtension([NotNull] ProxiesOptionsExtension copyFrom)
        {
            _useLazyLoadingProxies = copyFrom._useLazyLoadingProxies;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected virtual ProxiesOptionsExtension Clone() => new ProxiesOptionsExtension(this);

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool UseLazyLoadingProxies => _useLazyLoadingProxies;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual ProxiesOptionsExtension WithLazyLoading(bool useLazyLoadingProxies = true)
        {
            var clone = Clone();

            clone._useLazyLoadingProxies = useLazyLoadingProxies;

            return clone;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual long GetServiceProviderHashCode() => _useLazyLoadingProxies ? 541 : 0;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["Proxies:" + nameof(ProxiesExtensions.UseLazyLoadingProxies)]
                = (_useLazyLoadingProxies ? 541 : 0).ToString(CultureInfo.InvariantCulture);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Validate(IDbContextOptions options)
        {
            if (_useLazyLoadingProxies)
            {
                var internalServiceProvider = options.FindExtension<CoreOptionsExtension>()?.InternalServiceProvider;
                if (internalServiceProvider != null)
                {
                    using (var scope = internalServiceProvider.CreateScope())
                    {
                        if (scope.ServiceProvider
                                .GetService<IEnumerable<IConventionSetBuilder>>()
                                ?.Any(s => s is ProxiesConventionSetBuilder) == false)
                        {
                            throw new InvalidOperationException(ProxiesStrings.ProxyServicesMissing);
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool ApplyServices(IServiceCollection services)
        {
            services.AddEntityFrameworkProxies();

            return false;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual string LogFragment
            => _logFragment
                ?? (_logFragment = _useLazyLoadingProxies
                        ? "using lazy-loading proxies "
                        : "");
    }
}
