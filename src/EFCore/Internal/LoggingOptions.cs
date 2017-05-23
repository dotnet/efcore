// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class LoggingOptions : ILoggingOptions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Initialize(IDbContextOptions options)
        {
            var coreOptions = options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

            IsSensitiveDataLoggingEnabled = coreOptions.IsSensitiveDataLoggingEnabled;
            WarningsConfiguration = coreOptions.WarningsConfiguration;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Validate(IDbContextOptions options)
        {
            var coreOptions = options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

            if (IsSensitiveDataLoggingEnabled != coreOptions.IsSensitiveDataLoggingEnabled)
            {
                Debug.Assert(coreOptions.InternalServiceProvider != null);

                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.EnableSensitiveDataLogging),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }

            if (WarningsConfiguration?.GetServiceProviderHashCode() != coreOptions.WarningsConfiguration?.GetServiceProviderHashCode())
            {
                Debug.Assert(coreOptions.InternalServiceProvider != null);

                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.ConfigureWarnings),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsSensitiveDataLoggingEnabled { get; private set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool IsSensitiveDataLoggingWarned { get; set; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual WarningsConfiguration WarningsConfiguration { get; private set; }
    }
}
