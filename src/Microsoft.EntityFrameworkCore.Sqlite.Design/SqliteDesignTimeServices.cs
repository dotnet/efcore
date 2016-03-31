// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class SqliteDesignTimeServices
    {
        public virtual IServiceCollection ConfigureDesignTimeServices([NotNull] IServiceCollection serviceCollection)
            => serviceCollection
                .AddSingleton<IScaffoldingModelFactory, SqliteScaffoldingModelFactory>()
                .AddSingleton<IRelationalTypeMapper, SqliteTypeMapper>()
                .AddSingleton<IDatabaseModelFactory, SqliteDatabaseModelFactory>()
                .AddSingleton<IRelationalAnnotationProvider, SqliteAnnotationProvider>();
    }
}
