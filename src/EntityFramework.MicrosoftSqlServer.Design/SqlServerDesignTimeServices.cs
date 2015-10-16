// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.SqlServer.Design.ReverseEngineering;
using Microsoft.Data.Entity.SqlServer.Design.Utilities;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.SqlServer.Design
{
    public class SqlServerDesignTimeServices
    {
        public virtual void ConfigureDesignTimeServices([NotNull] IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<MetadataModelProvider, SqlServerMetadataModelProvider>()
                .AddSingleton<IRelationalAnnotationProvider, SqlServerAnnotationProvider>()
                .AddSingleton<SqlServerLiteralUtilities>()
                .AddSingleton<ConfigurationFactory, SqlServerConfigurationFactory>()
                .AddSingleton<IRelationalTypeMapper, SqlServerTypeMapper>()
                .AddSingleton<IMetadataReader, SqlServerMetadataReader>()
                .AddSingleton<DbContextWriter>()
                .AddSingleton<EntityTypeWriter>()
                .AddSingleton<CodeWriter, StringBuilderCodeWriter>();
        }
    }
}
