// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Internal;
using Microsoft.Data.Entity.Scaffolding.Internal;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Data.Entity.Scaffolding
{
    public class SqliteDesignTimeServices
    {
        public virtual void ConfigureDesignTimeServices([NotNull] IServiceCollection serviceCollection)
        {
            serviceCollection
                .AddSingleton<IScaffoldingModelFactory, RelationalScaffoldingModelFactory>()
                .AddSingleton<IRelationalTypeMapper, SqliteTypeMapper>()
                .AddSingleton<IDatabaseModelFactory, SqliteDatabaseModelFactory>()

                // TODO remove
                .AddSingleton<IMethodNameProvider, SqliteMethodNameProvider>() 

                .AddSingleton<IRelationalAnnotationProvider, SqliteAnnotationProvider>();
        }
    }
}
