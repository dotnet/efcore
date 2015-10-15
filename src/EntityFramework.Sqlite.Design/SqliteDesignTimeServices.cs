// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Relational.Design;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Internal;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering;
using Microsoft.Data.Entity.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.Sqlite.Design
{
    public class SqliteDesignTimeServices
    {
        public virtual void ConfigureDesignTimeServices([NotNull] IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<MetadataModelProvider, RelationalMetadataModelProvider>()
                .AddSingleton<IRelationalTypeMapper, SqliteTypeMapper>()
                .AddSingleton<IRelationalAnnotationProvider, SqliteAnnotationProvider>()
                .AddSingleton<ConfigurationFactory, SqliteConfigurationFactory>()
                .AddSingleton<DbContextWriter>()
                .AddSingleton<EntityTypeWriter>()
                .AddSingleton<CodeWriter, StringBuilderCodeWriter>()
                .AddSingleton<IMetadataReader, SqliteMetadataReader>();
        }
    }
}
