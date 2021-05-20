// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Sqlite.Scaffolding.Internal;
using Microsoft.Extensions.DependencyInjection;

[assembly: DesignTimeProviderServices("Microsoft.EntityFrameworkCore.Sqlite.Design.Internal.SqliteDesignTimeServices")]

namespace Microsoft.EntityFrameworkCore.Sqlite.Design.Internal
{
    /// <summary>
    ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///     any release. You should only use it directly in your code with extreme caution and knowing that
    ///     doing so can result in application failures when updating to a new Entity Framework Core release.
    /// </summary>
    public class SqliteDesignTimeServices : IDesignTimeServices
    {
        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual void ConfigureDesignTimeServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddEntityFrameworkSqlite();
            new EntityFrameworkRelationalDesignServicesBuilder(serviceCollection)
                .TryAdd<ICSharpRuntimeAnnotationCodeGenerator, SqliteCSharpRuntimeAnnotationCodeGenerator>()
                .TryAdd<IDatabaseModelFactory, SqliteDatabaseModelFactory>()
                .TryAdd<IProviderConfigurationCodeGenerator, SqliteCodeGenerator>()
                .TryAddCoreServices();
        }
    }
}
