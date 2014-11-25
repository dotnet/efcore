// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.SQLite.Metadata;
using Microsoft.Data.Entity.SQLite.Utilities;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteMigrationOperationSqlGeneratorFactory : IMigrationOperationSqlGeneratorFactory
    {
        private readonly SQLiteMetadataExtensionProvider _extensionProvider;

        public SQLiteMigrationOperationSqlGeneratorFactory(
            [NotNull] SQLiteMetadataExtensionProvider extensionProvider)
        {
            Check.NotNull(extensionProvider, "extensionProvider");

            _extensionProvider = extensionProvider;
        }

        public virtual SQLiteMetadataExtensionProvider ExtensionProvider
        {
            get { return _extensionProvider; }
        }

        public virtual SQLiteMigrationOperationSqlGenerator Create()
        {
            return Create(new Model());
        }

        public virtual SQLiteMigrationOperationSqlGenerator Create([NotNull] IModel targetModel)
        {
            Check.NotNull(targetModel, "targetModel");

            return
                new SQLiteMigrationOperationSqlGenerator(
                    ExtensionProvider, 
                    new SQLiteTypeMapper())
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
