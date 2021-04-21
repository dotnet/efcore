// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Scaffolding;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Diagnostics.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class SqliteDatabaseCleaner : RelationalDatabaseCleaner
    {
        protected override IDatabaseModelFactory CreateDatabaseModelFactory(ILoggerFactory loggerFactory)
        {
            // NOTE: You may need to update AddEntityFrameworkDesignTimeServices() too
            var services = new ServiceCollection()
                .AddSingleton<TypeMappingSourceDependencies>()
                .AddSingleton<RelationalTypeMappingSourceDependencies>()
                .AddSingleton<ValueConverterSelectorDependencies>()
                .AddSingleton<DiagnosticSource>(new DiagnosticListener(DbLoggerCategory.Name))
                .AddSingleton<ILoggingOptions, LoggingOptions>()
                .AddSingleton<IDbContextLogger, NullDbContextLogger>()
                .AddSingleton<LoggingDefinitions, SqliteLoggingDefinitions>()
                .AddSingleton(typeof(IDiagnosticsLogger<>), typeof(DiagnosticsLogger<>))
                .AddSingleton<IValueConverterSelector, ValueConverterSelector>()
                .AddSingleton<IInterceptors, Interceptors>()
                .AddLogging();
            new SqliteDesignTimeServices().ConfigureDesignTimeServices(services);

            return services
                .BuildServiceProvider()
                .GetRequiredService<IDatabaseModelFactory>();
        }

        protected override bool AcceptForeignKey(DatabaseForeignKey foreignKey)
            => false;

        protected override bool AcceptIndex(DatabaseIndex index)
            => false;

        protected override string BuildCustomSql(DatabaseModel databaseModel)
            => "PRAGMA foreign_keys=OFF;";

        protected override void OpenConnection(IRelationalConnection connection)
        {
            connection.Open();

            SpatialiteLoader.TryLoad(connection.DbConnection);
        }
    }
}
