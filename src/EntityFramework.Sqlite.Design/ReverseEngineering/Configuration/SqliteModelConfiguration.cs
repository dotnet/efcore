// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Design.ReverseEngineering.Configuration;
using Microsoft.Data.Entity.Relational.Design.Utilities;
using Microsoft.Data.Sqlite;

namespace Microsoft.Data.Entity.Sqlite.Design.ReverseEngineering.Configuration
{
    public class SqliteModelConfiguration : ModelConfiguration
    {
        public SqliteModelConfiguration(
            [NotNull] IModel model,
            [NotNull] CustomConfiguration customConfiguration,
            [NotNull] IRelationalMetadataExtensionProvider extensionsProvider,
            [NotNull] CSharpUtilities cSharpUtilities,
            [NotNull] ModelUtilities modelUtilities)
            : base(model, customConfiguration, extensionsProvider, cSharpUtilities, modelUtilities)
        {
        }

        public override string UseMethodName => nameof(SqliteDbContextOptionsBuilderExtensions.UseSqlite);

        public override string ClassName()
        {
            var sqliteConnection = new SqliteConnectionStringBuilder(CustomConfiguration.ConnectionString);
            if (sqliteConnection.DataSource != null)
            {
                var fileName = Path.GetFileNameWithoutExtension(sqliteConnection.DataSource);
                return CSharpUtilities.GenerateCSharpIdentifier(
                    fileName + DbContextSuffix, null);
            }

            return base.ClassName();
        }
    }
}
