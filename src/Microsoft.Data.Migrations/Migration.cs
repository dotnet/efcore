// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Migrations.Builders;

namespace Microsoft.Data.Migrations
{
    public abstract class Migration
    {
        public abstract void Up([NotNull] MigrationBuilder migrationBuilder);
        public abstract void Down([NotNull] MigrationBuilder migrationBuilder);
    }
}
