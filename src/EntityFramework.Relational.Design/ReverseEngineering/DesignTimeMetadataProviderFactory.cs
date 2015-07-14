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
            serviceCollection.AddTransient<CSharpCodeGeneratorHelper, CSharpCodeGeneratorHelper>();
            serviceCollection.AddTransient<ModelUtilities, ModelUtilities>();
            serviceCollection.AddTransient<ICompilationService, RoslynCompilationService>();
            serviceCollection.AddTransient<MetadataReferencesProvider, MetadataReferencesProvider>();
            serviceCollection.AddTransient<ITemplating, RazorTemplating>();
            serviceCollection.AddTransient<ReverseEngineeringGenerator, ReverseEngineeringGenerator>();
        }

        public abstract IDatabaseMetadataModelProvider Create([NotNull] ServiceCollection serviceCollection);
    }
}
