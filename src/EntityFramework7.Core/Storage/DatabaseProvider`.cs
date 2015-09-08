// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Storage
{
    public class DatabaseProvider<TProviderServices, TOptionsExtension> : IDatabaseProvider
        where TProviderServices : class, IDatabaseProviderServices
        where TOptionsExtension : class, IDbContextOptionsExtension
    {
        public virtual IDatabaseProviderServices GetProviderServices(IServiceProvider serviceProvider)
            => Check.NotNull(serviceProvider, nameof(serviceProvider)).GetRequiredService<TProviderServices>();

        public virtual bool IsConfigured(IDbContextOptions options)
            => Check.NotNull(options, nameof(options)).Extensions.OfType<TOptionsExtension>().Any();
    }
}
