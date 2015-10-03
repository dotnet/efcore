// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal.Templating;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal.Templating.Compilation;
using Microsoft.Data.Entity.SqlServer.Design.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerDesignTimeMetadataProviderFactory : DesignTimeMetadataProviderFactory
    {
        public override void AddMetadataProviderServices([NotNull] IServiceCollection serviceCollection)
        {
            base.AddMetadataProviderServices(serviceCollection);
            serviceCollection
                .AddSingleton<MetadataReferencesProvider>()
                .AddSingleton<ICompilationService, RoslynCompilationService>()
                .AddSingleton<RazorTemplating>()
                .AddSingleton<IDatabaseMetadataModelProvider, SqlServerMetadataModelProvider>()
                .AddSingleton<IRelationalAnnotationProvider, SqlServerAnnotationProvider>()
                .AddSingleton<SqlServerLiteralUtilities>()
                .AddSingleton<ConfigurationFactory, SqlServerConfigurationFactory>()
                .AddSingleton<CodeWriter, RazorTemplateCodeWriter>();
        }
    }
}
