// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.SQLite.Metadata;
using Microsoft.Data.Entity.SQLite.Migrations;

namespace Microsoft.Data.Entity.SQLite
{
    public class SQLiteModelDiffer : ModelDiffer
    {
        public SQLiteModelDiffer(
            [NotNull] SQLiteMetadataExtensionProvider extensionProvider,
            [NotNull] SQLiteTypeMapper typeMapper,
            [NotNull] SQLiteMigrationOperationFactory operationFactory,
            [NotNull] SQLiteMigrationOperationProcessor operationProcessor)
            : base(
                extensionProvider,
                typeMapper,
                operationFactory,
                operationProcessor)
        {
        }

        public virtual new SQLiteMetadataExtensionProvider ExtensionProvider
        {
            get { return (SQLiteMetadataExtensionProvider)base.ExtensionProvider; }
        }

        public virtual new SQLiteTypeMapper TypeMapper
        {
            get { return (SQLiteTypeMapper)base.TypeMapper; }
        }

        public virtual new SQLiteMigrationOperationFactory OperationFactory
        {
            get { return (SQLiteMigrationOperationFactory)base.OperationFactory; }
        }

        public virtual new SQLiteMigrationOperationProcessor OperationProcessor
        {
            get { return (SQLiteMigrationOperationProcessor)base.OperationProcessor; }
        }
    }
}
