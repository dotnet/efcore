// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Migrations.Builders;

namespace Microsoft.Data.Entity.Migrations
{
    public abstract class Migration
    {
        // TODO: Hide?
        public abstract string Id { get; }
        public virtual string ProductVersion => null;

        public virtual void BuildTargetModel([NotNull] ModelBuilder modelBuilder)
        {
        }

        public abstract void Up([NotNull] MigrationBuilder migrationBuilder);
        public abstract void Down([NotNull] MigrationBuilder migrationBuilder);
    }
}
