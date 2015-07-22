// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Design.CodeGeneration;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Entity.Relational.Design.Templating;
using Microsoft.Data.Entity.Relational.Design.Templating.Compilation;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Relational.Design.ReverseEngineering
{
    public abstract class DesignTimeMetadataProviderFactory : IDesignTimeMetadataProviderFactory
    {
        public virtual void AddMetadataProviderServices([NotNull] IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<CSharpCodeGeneratorHelper, CSharpCodeGeneratorHelper>();
            serviceCollection.AddScoped<ModelUtilities, ModelUtilities>();
            serviceCollection.AddScoped<ICompilationService, RoslynCompilationService>();
            serviceCollection.AddScoped<MetadataReferencesProvider, MetadataReferencesProvider>();
            serviceCollection.AddScoped<ITemplating, RazorTemplating>();
            serviceCollection.AddScoped<ReverseEngineeringGenerator, ReverseEngineeringGenerator>();
        }
    }
}
