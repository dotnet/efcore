// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Internal
{
    public interface IDbContextServices : IDisposable
    {
        IDbContextServices Initialize(
            [NotNull] IServiceProvider scopedProvider,
            [NotNull] IDbContextOptions contextOptions,
            [NotNull] DbContext context,
            ServiceProviderSource serviceProviderSource);

        DbContext Context { get; }
        IModel Model { get; }
        IDbContextOptions ContextOptions { get; }
        IDatabaseProviderServices DatabaseProviderServices { get; }
        IServiceProvider ServiceProvider { get; }
    }
}
