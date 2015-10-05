// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    // TODO unify with startup convention from commands
    public interface IDesignTimeMetadataProviderFactory
    {
        void AddMetadataProviderServices([NotNull] IServiceCollection serviceCollection);
    }
}
