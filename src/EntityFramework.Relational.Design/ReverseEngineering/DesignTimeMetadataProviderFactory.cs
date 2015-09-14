// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class DesignTimeMetadataProviderFactory : IDesignTimeMetadataProviderFactory
    {
        public virtual void AddMetadataProviderServices([NotNull] IServiceCollection serviceCollection)
        {
            Check.NotNull(serviceCollection, nameof(serviceCollection));

            serviceCollection.AddScoped<ModelUtilities, ModelUtilities>()
                .AddScoped<ReverseEngineeringGenerator>()
                .AddScoped<CSharpUtilities>();
        }
    }
}
