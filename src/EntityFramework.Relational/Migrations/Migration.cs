// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Migrations.Builders;

namespace Microsoft.Data.Entity.Relational.Migrations
{
    public abstract class Migration
    {
        public abstract void Up([NotNull] MigrationBuilder migrationBuilder);

        public abstract void Down([NotNull] MigrationBuilder migrationBuilder);
    }
}
