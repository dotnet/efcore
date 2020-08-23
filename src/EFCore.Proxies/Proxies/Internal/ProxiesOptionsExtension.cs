// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Proxies.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ProxiesOptionsExtension : IDbContextOptionsExtension
    {
        private DbContextOptionsExtensionInfo _info;
        private bool _useLazyLoadingProxies;
        private bool _useChangeTrackingProxies;
        private bool _checkEquality;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ProxiesOptionsExtension()
        {
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected ProxiesOptionsExtension([NotNull] ProxiesOptionsExtension copyFrom)
        {
            _useLazyLoadingProxies = copyFrom._useLazyLoadingProxies;
            _useChangeTrackingProxies = copyFrom._useChangeTrackingProxies;
            _checkEquality = copyFrom._checkEquality;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual DbContextOptionsExtensionInfo Info
            => _info ??= new ExtensionInfo(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected virtual ProxiesOptionsExtension Clone()
            => new ProxiesOptionsExtension(this);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool UseLazyLoadingProxies
            => _useLazyLoadingProxies;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool UseChangeTrackingProxies
            => _useChangeTrackingProxies;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool CheckEquality
            => _checkEquality;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool UseProxies
            => UseLazyLoadingProxies || UseChangeTrackingProxies;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ProxiesOptionsExtension WithLazyLoading(bool useLazyLoadingProxies = true)
        {
            var clone = Clone();

            clone._useLazyLoadingProxies = useLazyLoadingProxies;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ProxiesOptionsExtension WithChangeTracking(bool useChangeTrackingProxies = true, bool checkEquality = true)
        {
            var clone = Clone();

            clone._useChangeTrackingProxies = useChangeTrackingProxies;
            clone._checkEquality = checkEquality;

            return clone;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Validate(IDbContextOptions options)
        {
            if (UseProxies)
            {
                var internalServiceProvider = options.FindExtension<CoreOptionsExtension>()?.InternalServiceProvider;
                if (internalServiceProvider != null)
                {
                    using var scope = internalServiceProvider.CreateScope();
                    var conventionPlugins = scope.ServiceProvider.GetService<IEnumerable<IConventionSetPlugin>>();
                    if (conventionPlugins?.Any(s => s is ProxiesConventionSetPlugin) == false)
                    {
                        throw new InvalidOperationException(ProxiesStrings.ProxyServicesMissing);
                    }
                }
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ApplyServices(IServiceCollection services)
            => services.AddEntityFrameworkProxies();

        private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            private string _logFragment;

            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            private new ProxiesOptionsExtension Extension
                => (ProxiesOptionsExtension)base.Extension;

            public override bool IsDatabaseProvider
                => false;

            public override string LogFragment
                => _logFragment ??= Extension.UseLazyLoadingProxies && Extension.UseChangeTrackingProxies
                    ? "using lazy-loading and change tracking proxies "
                    : Extension.UseLazyLoadingProxies
                        ? "using lazy-loading proxies "
                        : Extension.UseChangeTrackingProxies
                            ? "using change tracking proxies "
                            : "";

            public override long GetServiceProviderHashCode()
                => Extension.UseProxies ? 541 : 0;

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                debugInfo["Proxies:" + nameof(ProxiesExtensions.UseLazyLoadingProxies)]
                    = (Extension._useLazyLoadingProxies ? 541 : 0).ToString(CultureInfo.InvariantCulture);

                debugInfo["Proxies:" + nameof(ProxiesExtensions.UseChangeTrackingProxies)]
                    = (Extension._useChangeTrackingProxies ? 541 : 0).ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
