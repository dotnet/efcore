// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Storage
{
    public interface IDatabaseProvider
    {
        IDatabaseProviderServices GetProviderServices([NotNull] IServiceProvider serviceProvider);
        bool IsConfigured([NotNull] IDbContextOptions options);
    }
}
