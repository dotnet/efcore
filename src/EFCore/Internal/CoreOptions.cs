// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class CoreSingletonOptions : ICoreSingletonOptions
    {
        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Initialize(IDbContextOptions options)
        {
            var coreOptions = options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

            AreDetailedErrorsEnabled = coreOptions.DetailedErrorsEnabled;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void Validate(IDbContextOptions options)
        {
            var coreOptions = options.FindExtension<CoreOptionsExtension>() ?? new CoreOptionsExtension();

            if (AreDetailedErrorsEnabled != coreOptions.DetailedErrorsEnabled)
            {
                Debug.Assert(coreOptions.InternalServiceProvider != null);

                throw new InvalidOperationException(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.EnableDetailedErrors),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)));
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual bool AreDetailedErrorsEnabled { get; private set; }
    }
}
