// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
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

        string IMigrationMetadata.Name
        {
            get { throw new NotImplementedException(); }
        }

        string IMigrationMetadata.Timestamp
        {
            get { throw new NotImplementedException(); }
        }

        IModel IMigrationMetadata.TargetModel
        {
            get { throw new NotImplementedException(); }
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
