// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Microsoft.EntityFrameworkCore.TestUtilities
{
    public class TestRelationalMigrationSqlGenerator : MigrationsSqlGenerator
    {
        public TestRelationalMigrationSqlGenerator(MigrationsSqlGeneratorDependencies dependencies)
            : base(dependencies)
        {
        }

        protected override void Generate(RenameTableOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
        }

        protected override void Generate(DropIndexOperation operation, IModel model, MigrationCommandListBuilder builder, bool terminate = true)
        {
        }

        protected override void Generate(RenameSequenceOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
        }

        protected override void Generate(RenameColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
        }

        protected override void Generate(EnsureSchemaOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
        }

        protected override void Generate(RenameIndexOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
        }

        protected override void Generate(AlterColumnOperation operation, IModel model, MigrationCommandListBuilder builder)
        {
        }
    }
}
