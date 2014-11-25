// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Sqlite.Metadata;

namespace Microsoft.Data.Entity.Sqlite.Migrations
{
    public class SqliteMigrationOperationFactory : MigrationOperationFactory
    {
        public SqliteMigrationOperationFactory(
            [NotNull] SqliteMetadataExtensionProvider extensionProvider)
            : base(extensionProvider)
        {
        }

        public virtual new SqliteMetadataExtensionProvider ExtensionProvider
        {
            get { return (SqliteMetadataExtensionProvider)base.ExtensionProvider; }
        }
    }
}
