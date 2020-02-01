// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class ScopedLoggerFactory : ILoggerFactory
    {
        private readonly ILoggerFactory _underlyingFactory;
        private readonly bool _dispose;

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public ScopedLoggerFactory(
            [NotNull] ILoggerFactory loggerFactory,
            bool dispose)
        {
            _underlyingFactory = loggerFactory;
            _dispose = dispose;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public static ScopedLoggerFactory Create(
            [NotNull] IServiceProvider internalServiceProvider,
            [CanBeNull] IDbContextOptions contextOptions)
        {
            var coreOptions
                = (contextOptions ?? internalServiceProvider.GetService<IDbContextOptions>())
                ?.FindExtension<CoreOptionsExtension>();

            if (coreOptions != null)
            {
                if (coreOptions.LoggerFactory != null)
                {
                    return new ScopedLoggerFactory(coreOptions.LoggerFactory, dispose: false);
                }

                var applicationServiceProvider = coreOptions.ApplicationServiceProvider;
                if (applicationServiceProvider != null
                    && applicationServiceProvider != internalServiceProvider)
                {
                    var loggerFactory = applicationServiceProvider.GetService<ILoggerFactory>();
                    if (loggerFactory != null)
                    {
                        return new ScopedLoggerFactory(loggerFactory, dispose: false);
                    }
                }
            }

            return new ScopedLoggerFactory(new LoggerFactory(), dispose: true);
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Dispose()
        {
            if (_dispose)
            {
                _underlyingFactory.Dispose();
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual ILogger CreateLogger(string categoryName)
            => _underlyingFactory.CreateLogger(categoryName);

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void AddProvider(ILoggerProvider provider)
            => _underlyingFactory.AddProvider(provider);
    }
}
