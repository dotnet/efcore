// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Utilities;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteMigrationOperationSqlGeneratorFactory : IMigrationOperationSqlGeneratorFactory
    {
        private readonly SqliteMetadataExtensionProvider _extensionProvider;

        public SqliteMigrationOperationSqlGeneratorFactory(
            [NotNull] SqliteMetadataExtensionProvider extensionProvider)
        {
            Check.NotNull(extensionProvider, "extensionProvider");

            _extensionProvider = extensionProvider;
        }

        public virtual SqliteMetadataExtensionProvider ExtensionProvider
        {
            get { return _extensionProvider; }
        }

        public virtual SqliteMigrationOperationSqlGenerator Create()
        {
            return Create(new Model());
        }

        public virtual SqliteMigrationOperationSqlGenerator Create([NotNull] IModel targetModel)
        {
            Check.NotNull(targetModel, "targetModel");

            return
                new SqliteMigrationOperationSqlGenerator(
                    ExtensionProvider, 
                    new SqliteTypeMapper())
                    {
                        TargetModel = targetModel,
                    };
        }

        MigrationOperationSqlGenerator IMigrationOperationSqlGeneratorFactory.Create()
        {
            return Create();
        }

        MigrationOperationSqlGenerator IMigrationOperationSqlGeneratorFactory.Create(IModel targetModel)
        {
            return Create(targetModel);
        }
    }
}
