// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.XuGu.Scaffolding.Internal;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Design.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.XuGu.Design.Internal
{
    public class XGDesignTimeServices : IDesignTimeServices
    {
        public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkXG();
            new EntityFrameworkRelationalDesignServicesBuilder(serviceCollection)
                .TryAdd<ICSharpRuntimeAnnotationCodeGenerator, XGCSharpRuntimeAnnotationCodeGenerator>()
                .TryAdd<IAnnotationCodeGenerator, XGAnnotationCodeGenerator>()
                .TryAdd<IDatabaseModelFactory, XGDatabaseModelFactory>()
                .TryAdd<IProviderConfigurationCodeGenerator, XGCodeGenerator>()
                .TryAddCoreServices();
        }
    }
}
