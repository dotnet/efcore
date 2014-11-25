// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Sqlite.Metadata;
using Microsoft.Data.Entity.Sqlite.Migrations;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteModelDiffer : ModelDiffer
    {
        public SqliteModelDiffer(
            [NotNull] SqliteMetadataExtensionProvider extensionProvider,
            [NotNull] SqliteTypeMapper typeMapper,
            [NotNull] SqliteMigrationOperationFactory operationFactory,
            [NotNull] SqliteMigrationOperationProcessor operationProcessor)
            : base(
                extensionProvider,
                typeMapper,
                operationFactory,
                operationProcessor)
        {
        }

        public virtual new SqliteMetadataExtensionProvider ExtensionProvider
        {
            get { return (SqliteMetadataExtensionProvider)base.ExtensionProvider; }
        }

        public virtual new SqliteTypeMapper TypeMapper
        {
            get { return (SqliteTypeMapper)base.TypeMapper; }
        }

        public virtual new SqliteMigrationOperationFactory OperationFactory
        {
            get { return (SqliteMigrationOperationFactory)base.OperationFactory; }
        }

        public virtual new SqliteMigrationOperationProcessor OperationProcessor
        {
            get { return (SqliteMigrationOperationProcessor)base.OperationProcessor; }
        }
    }
}
