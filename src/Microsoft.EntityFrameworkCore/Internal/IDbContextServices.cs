// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Internal
{
    public interface IDbContextServices
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
