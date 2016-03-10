// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public interface IDbContextServices
    {
        IDbContextServices Initialize(
            [NotNull] IServiceProvider scopedProvider,
            [NotNull] IDbContextOptions contextOptions,
            [NotNull] DbContext context);

        DbContext Context { get; }
        IModel Model { get; }
        ILoggerFactory LoggerFactory { get; }
        IMemoryCache MemoryCache { get; }
        IDbContextOptions ContextOptions { get; }
        IDatabaseProviderServices DatabaseProviderServices { get; }
        IServiceProvider InternalServiceProvider { get; }
    }
}
