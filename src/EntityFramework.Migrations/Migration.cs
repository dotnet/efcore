// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Migrations.Builders;
using Microsoft.Data.Entity.Migrations.Infrastructure;
using Microsoft.Data.Entity.Migrations.Model;

namespace Microsoft.Data.Entity.Migrations
{
    public abstract class Migration : IMigrationMetadata
    {
        public abstract void Up([NotNull] MigrationBuilder migrationBuilder);

        public abstract void Down([NotNull] MigrationBuilder migrationBuilder);

        protected abstract string MigrationId { get; }

        protected abstract string ProductVersion { get; }

        protected abstract IModel TargetModel { get; }

        string IMigrationMetadata.MigrationId
        {
            get { return MigrationId; }
        }

        string IMigrationMetadata.ProductVersion
        {
            get { return ProductVersion; }
        }

        IModel IMigrationMetadata.TargetModel
        {
            get { return TargetModel; }
        }

        IReadOnlyList<MigrationOperation> IMigrationMetadata.UpgradeOperations
        {
            get
            {
                var builder = new MigrationBuilder();
                Up(builder);
                return builder.Operations;
            }
        }

        IReadOnlyList<MigrationOperation> IMigrationMetadata.DowngradeOperations
        {
            get
            {
                var builder = new MigrationBuilder();
                Down(builder);
                return builder.Operations;
            }
        }
    }
}
