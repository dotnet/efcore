// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Migrations;
using Microsoft.Data.Entity.Relational.Migrations.MigrationsModel;
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

        public override Column Column(IProperty property)
        {
            var column = base.Column(property);

            // TODO: This is essentially duplicated logic from the selector; combine if possible
            if (property.GenerateValueOnAdd)
            {
                var strategy = property.SqlServer().ValueGenerationStrategy
                               ?? property.EntityType.Model.SqlServer().ValueGenerationStrategy;

                if (strategy == SqlServerValueGenerationStrategy.Identity
                    || (strategy == null
                        && property.PropertyType.IsInteger()
                        && property.PropertyType != typeof(byte)
                        && property.PropertyType != typeof(byte?)))
                {
                    column.IsIdentity = true;
                }
            }

            return column;
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
