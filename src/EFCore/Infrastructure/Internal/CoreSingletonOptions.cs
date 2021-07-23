// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
    ///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
    ///     </para>
    /// </summary>
    public class CoreSingletonOptions : ICoreSingletonOptions
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Initialize(IDbContextOptions options)
        {
            var coreOptions = options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

            AreDetailedErrorsEnabled = coreOptions.DetailedErrorsEnabled;
            AreThreadSafetyChecksEnabled = coreOptions.ThreadSafetyChecksEnabled;
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void Validate(IDbContextOptions options)
        {
            var coreOptions = options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

            if (AreDetailedErrorsEnabled != coreOptions.DetailedErrorsEnabled)
            {
                Check.DebugAssert(coreOptions.InternalServiceProvider != null, "InternalServiceProvider is null");

                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.EnableDetailedErrors),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (AreThreadSafetyChecksEnabled != coreOptions.ThreadSafetyChecksEnabled)
            {
                Check.DebugAssert(coreOptions.InternalServiceProvider != null, "InternalServiceProvider is null");

                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.EnableThreadSafetyChecks),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }
        }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool AreDetailedErrorsEnabled { get; private set; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual bool AreThreadSafetyChecksEnabled { get; private set; }
    }
}
