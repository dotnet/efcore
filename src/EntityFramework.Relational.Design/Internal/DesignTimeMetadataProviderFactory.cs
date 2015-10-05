// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Scaffolding.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.Scaffolding.Internal
{
    // TODO use startup convention
    public abstract class DesignTimeMetadataProviderFactory : IDesignTimeMetadataProviderFactory
    {
        public virtual void AddMetadataProviderServices([NotNull] IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            serviceCollection
                .AddSingleton<IFileService, FileSystemFileService>()
                .AddSingleton<ModelUtilities>()
                .AddSingleton<ReverseEngineeringGenerator>()
                .AddSingleton<CSharpUtilities>();
        }
    }
}
