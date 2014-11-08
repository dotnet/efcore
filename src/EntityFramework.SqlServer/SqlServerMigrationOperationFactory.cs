// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations;
using Microsoft.Data.Entity.Migrations.Model;
using Microsoft.Data.Entity.SqlServer.Metadata;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerMigrationOperationFactory : MigrationOperationFactory
    {
        public SqlServerMigrationOperationFactory(
            [NotNull] SqlServerMetadataExtensionProvider extensionProvider)
            : base(extensionProvider)
        {
        }

        public virtual new SqlServerMetadataExtensionProvider ExtensionProvider
        {
            get { return (SqlServerMetadataExtensionProvider)base.ExtensionProvider; }
        }

        public override AddPrimaryKeyOperation AddPrimaryKeyOperation(IKey target)
        {
            var operation = base.AddPrimaryKeyOperation(target);
            var isClustered = ExtensionProvider.Extensions(target).IsClustered;

            if (isClustered.HasValue)
            {
                operation.IsClustered = isClustered.Value;
            }

            return operation;
        }

        public override CreateIndexOperation CreateIndexOperation(IIndex target)
        {
            var operation = base.CreateIndexOperation(target);
            var isClustered = ExtensionProvider.Extensions(target).IsClustered;

            if (isClustered.HasValue)
            {
                operation.IsClustered = isClustered.Value;
            }

            return operation;
        }
    }
}
