// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.SqlServer.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMigrationOperationSqlGeneratorFactory : IMigrationOperationSqlGeneratorFactory
    {
        private readonly SqlServerMetadataExtensionProvider _extensionProvider;

        public SqlServerMigrationOperationSqlGeneratorFactory(
            [NotNull] SqlServerMetadataExtensionProvider extensionProvider)
        {
            Check.NotNull(extensionProvider, "extensionProvider");

            _extensionProvider = extensionProvider;
        }

        public virtual SqlServerMetadataExtensionProvider ExtensionProvider
        {
            get { return _extensionProvider; }
        }

        public virtual SqlServerMigrationOperationSqlGenerator Create()
        {
            return Create(new Model());
        }

        public virtual SqlServerMigrationOperationSqlGenerator Create([NotNull] IModel targetModel)
        {
            Check.NotNull(targetModel, "targetModel");

            return
                new SqlServerMigrationOperationSqlGenerator(
                    ExtensionProvider,
                    new SqlServerTypeMapper())
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
