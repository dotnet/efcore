// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.SqlServer.Design.Utilities;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering
{
    public class SqlServerDesignTimeMetadataProviderFactory : DesignTimeMetadataProviderFactory
    {
        public override void AddMetadataProviderServices([NotNull] IServiceCollection serviceCollection)
        {
            base.AddMetadataProviderServices(serviceCollection);
            serviceCollection.AddScoped<IDatabaseMetadataModelProvider, SqlServerMetadataModelProvider>()
                .AddScoped<IRelationalMetadataExtensionProvider, SqlServerMetadataExtensionProvider>()
                .AddScoped<SqlServerLiteralUtilities>()
                .AddScoped<ModelConfigurationFactory, SqlServerModelConfigurationFactory>();
        }
    }
}
