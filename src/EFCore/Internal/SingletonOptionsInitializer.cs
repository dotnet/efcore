// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <inheritdoc />
    public class SingletonOptionsInitializer : ISingletonOptionsInitializer
    {
        private volatile bool _isInitialized;
        private readonly object _lock = new object();

        /// <inheritdoc />
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
