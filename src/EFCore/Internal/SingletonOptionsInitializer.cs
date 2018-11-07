// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class SingletonOptionsInitializer : ISingletonOptionsInitializer
    {
        private volatile bool _isInitialized;
        private readonly object _lock = new object();

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual void EnsureInitialized(
            IServiceProvider serviceProvider,
            IDbContextOptions options)
        {
            if (!_isInitialized)
            {
                lock (_lock)
                {
                    if (!_isInitialized)
                    {
                        foreach (var singletonOptions in serviceProvider.GetService<IEnumerable<ISingletonOptions>>())
                        {
                            singletonOptions.Initialize(options);
                        }

                        _isInitialized = true;
                    }
                }
            }

            foreach (var singletonOptions in serviceProvider.GetService<IEnumerable<ISingletonOptions>>())
            {
                singletonOptions.Validate(options);
            }
        }
    }
}
